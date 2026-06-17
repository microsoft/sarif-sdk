// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// The per-row state a column accessor reads from: the run, the result, the physical location the
    /// row projects (null when the result has no location), and the result's resolved rule.
    /// </summary>
    public sealed class ProjectionContext
    {
        public Run Run { get; set; }

        public Result Result { get; set; }

        public PhysicalLocation Location { get; set; }

        public ReportingDescriptor Rule { get; set; }

        public int RunIndex { get; set; }

        public int ResultIndex { get; set; }
    }

    /// <summary>
    /// Resolves a caller-specified column name to a function that projects a single result row to that
    /// column's string value. The named registry mirrors the <c>SarifToCsv</c> sample; this verb closes
    /// two of the sample's resolution gaps so projected values follow the spec's inheritance rules:
    /// <list type="bullet">
    /// <item><description><c>Level</c> falls back to the rule's <c>defaultConfiguration.level</c>, then to
    /// the spec default <c>warning</c>, rather than echoing the raw result enum.</description></item>
    /// <item><description><c>Properties.&lt;key&gt;</c> falls back to the matching <c>rule.properties</c>
    /// value when the property is absent on the result (e.g. <c>security-severity</c>).</description></item>
    /// </list>
    /// Missing values render as empty strings, never the string <c>"null"</c> or a sentinel.
    /// </summary>
    public static class RowProjection
    {
        private const string PropertiesPrefix = "Properties.";
        private const string FingerprintsPrefix = "Fingerprints.";
        private const string PartialFingerprintsPrefix = "PartialFingerprints.";

        private static readonly Dictionary<string, Func<ProjectionContext, string>> s_accessors = BuildAccessors();

        public static IEnumerable<string> SupportedColumns => s_accessors.Keys;

        /// <summary>
        /// Returns the accessor for <paramref name="columnName"/>. Dynamic <c>Properties.</c>,
        /// <c>Fingerprints.</c>, and <c>PartialFingerprints.</c> columns resolve by key suffix; any other
        /// unrecognized name throws <see cref="ArgumentException"/>.
        /// </summary>
        public static Func<ProjectionContext, string> GetAccessor(string columnName)
        {
            if (s_accessors.TryGetValue(columnName, out Func<ProjectionContext, string> accessor))
            {
                return accessor;
            }

            if (columnName.StartsWith(PropertiesPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = columnName.Substring(PropertiesPrefix.Length);
                return c => InheritedProperty(c, key);
            }

            if (columnName.StartsWith(FingerprintsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = columnName.Substring(FingerprintsPrefix.Length);
                return c => c.Result.Fingerprints != null && c.Result.Fingerprints.TryGetValue(key, out string v) ? v : "";
            }

            if (columnName.StartsWith(PartialFingerprintsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = columnName.Substring(PartialFingerprintsPrefix.Length);
                return c => c.Result.PartialFingerprints != null && c.Result.PartialFingerprints.TryGetValue(key, out string v) ? v : "";
            }

            throw new ArgumentException(
                $"Unknown projection column \"{columnName}\". Known columns:{Environment.NewLine}\t{string.Join(Environment.NewLine + "\t", s_accessors.Keys)}");
        }

        /// <summary>
        /// Resolves the effective failure level following the spec's inheritance order: an explicit
        /// result level, else the rule's configured default, else <c>warning</c>.
        /// </summary>
        /// <remarks>
        /// The typed object model populates an absent <c>result.level</c> with the schema default
        /// (<c>warning</c>) on read, so an explicitly authored <c>warning</c> is indistinguishable from an
        /// omitted level. When the result level is <c>warning</c>, this method therefore defers to the
        /// rule's configured level — which is itself <c>warning</c> unless the rule overrides it, so the
        /// chain collapses to the spec default in the common case.
        /// </remarks>
        internal static FailureLevel EffectiveLevel(Result result, ReportingDescriptor rule)
        {
            if (result.Level != FailureLevel.Warning)
            {
                return result.Level;
            }

            return rule?.DefaultConfiguration?.Level ?? FailureLevel.Warning;
        }

        private static string InheritedProperty(ProjectionContext c, string key)
        {
            if (c.Result.TryGetProperty(key, out string value) || (c.Rule != null && c.Rule.TryGetProperty(key, out value)))
            {
                return value ?? "";
            }

            return "";
        }

        private static string ResolveFileUri(ArtifactLocation artifactLocation, Run run)
        {
            if (artifactLocation == null) { return ""; }
            if (artifactLocation.Uri != null) { return artifactLocation.Uri.ToString(); }

            if (artifactLocation.Index >= 0 && run?.Artifacts != null && artifactLocation.Index < run.Artifacts.Count)
            {
                return run.Artifacts[artifactLocation.Index].Location?.Uri?.ToString() ?? "";
            }

            return "";
        }

        private static string Number(int? value) => value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "";

        private static string Join(IEnumerable<string> values) => string.Join("; ", values ?? Array.Empty<string>());

        private static Dictionary<string, Func<ProjectionContext, string>> BuildAccessors()
        {
            var a = new Dictionary<string, Func<ProjectionContext, string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Result identity and classification.
                ["BaselineState"] = c => c.Result.BaselineState.ToString(),
                ["CorrelationGuid"] = c => c.Result.CorrelationGuid?.ToString(SarifConstants.GuidFormat) ?? "",
                ["Guid"] = c => c.Result.Guid?.ToString(SarifConstants.GuidFormat) ?? "",
                ["HostedViewerUri"] = c => c.Result.HostedViewerUri?.ToString() ?? "",
                ["Kind"] = c => c.Result.Kind.ToString(),
                ["Level"] = c => EffectiveLevel(c.Result, c.Rule).ToString(),
                ["OccurrenceCount"] = c => c.Result.OccurrenceCount.ToString(CultureInfo.InvariantCulture),
                ["Rank"] = c => c.Result.Rank.ToString(CultureInfo.InvariantCulture),
                ["RuleId"] = c => c.Rule?.Id ?? c.Result.ResolvedRuleId(c.Run) ?? "",
                ["RuleIndex"] = c => c.Result.RuleIndex.ToString(CultureInfo.InvariantCulture),

                // Messages.
                ["Message.Text"] = c => c.Result.Message?.Text ?? "",
                ["Message"] = c => c.Result.GetMessageText(c.Rule),

                // Collections projected as "; "-joined scalars.
                ["Fingerprints"] = c => Join(c.Result.Fingerprints?.Values),
                ["PartialFingerprints"] = c => Join(c.Result.PartialFingerprints?.Values),
                ["Tags"] = c => Join(c.Result.Tags),
                ["Suppressions"] = c => Join(c.Result.Suppressions?.Select(s => $"{s.Kind}|{s.Status}")),
                ["WorkItemUris"] = c => Join(c.Result.WorkItemUris?.Select(u => u.ToString())),
                ["Properties"] = WriteProperties,

                // Physical location.
                ["Location.Uri"] = c => ResolveFileUri(c.Location?.ArtifactLocation, c.Run),
                ["Location.Tags"] = c => Join(c.Location?.Tags),

                // Region.
                ["Location.Region.ByteLength"] = c => Number(c.Location?.Region?.ByteLength),
                ["Location.Region.ByteOffset"] = c => Number(c.Location?.Region?.ByteOffset),
                ["Location.Region.CharLength"] = c => Number(c.Location?.Region?.CharLength),
                ["Location.Region.CharOffset"] = c => Number(c.Location?.Region?.CharOffset),
                ["Location.Region.EndColumn"] = c => Number(c.Location?.Region?.EndColumn),
                ["Location.Region.EndLine"] = c => Number(c.Location?.Region?.EndLine),
                ["Location.Region.StartColumn"] = c => Number(c.Location?.Region?.StartColumn),
                ["Location.Region.StartLine"] = c => Number(c.Location?.Region?.StartLine),
                ["Location.Region.Message.Text"] = c => c.Location?.Region?.Message?.Text ?? "",
                ["Location.Region.Snippet.Text"] = c => c.Location?.Region?.Snippet?.Text ?? "",
                ["Location.Region.SourceLanguage"] = c => c.Location?.Region?.SourceLanguage ?? "",

                // Run identity.
                ["Run.BaselineGuid"] = c => c.Run?.BaselineGuid?.ToString(SarifConstants.GuidFormat) ?? "",
                ["Run.AutomationDetails.CorrelationGuid"] = c => c.Run?.AutomationDetails?.CorrelationGuid?.ToString(SarifConstants.GuidFormat) ?? "",
                ["Run.AutomationDetails.Guid"] = c => c.Run?.AutomationDetails?.Guid?.ToString(SarifConstants.GuidFormat) ?? "",
                ["Run.AutomationDetails.Id"] = c => c.Run?.AutomationDetails?.Id ?? "",
                ["Run.Tool.Name"] = c => c.Run?.Tool?.Driver?.Name ?? "",

                // Positional identity (when guids are absent).
                ["RunIndex"] = c => c.RunIndex.ToString(CultureInfo.InvariantCulture),
                ["ResultIndex"] = c => c.ResultIndex.ToString(CultureInfo.InvariantCulture),
            };

            return a;
        }

        private static string WriteProperties(ProjectionContext c)
        {
            var builder = new StringBuilder();

            foreach (string propertyName in c.Result.PropertyNames)
            {
                if (builder.Length > 0) { builder.Append("; "); }
                builder.Append(propertyName).Append(": ").Append(c.Result.GetSerializedPropertyValue(propertyName) ?? "");
            }

            return builder.ToString();
        }
    }
}
