// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideToolProperties : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2005
        /// </summary>
        public override string Id => RuleId.ProvideToolProperties;

        /// <summary>
        /// Provide information that makes it easy to identify the name and version of your tool.
        ///
        /// The tool's 'name' property should be no more than three words long. This makes it easy
        /// to remember and allows it to fit into a narrow column when displaying a list of results.
        /// If you need to provide more information about your tool, use the 'fullName' property.
        ///
        /// The tool should provide either or both of the 'version' and 'semanticVersion' properties.
        /// This enables the log file consumer to determine whether the file was produced by an up
        /// to date version, and to avoid accidentally comparing log files produced by different tool
        /// versions.
        ///
        /// If 'version' is used, facilitate comparison between versions by specifying a version number
        /// that starts with an integer, optionally followed by any desired characters.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2005_ProvideToolProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_ProvideToolVersion_Text),
            nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_ProvideConciseToolName_Text),
            nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_UseNumericToolVersions_Text),
            nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_ProvideToolnformationUri_Text)
        };

        // This rule configuration parameter specifies which of the three version-related properties
        // satisfy the requirement that the tool provide version information. By default, any of
        // them is acceptable.
        public static PerLanguageOption<StringSet> AcceptableVersionProperties =>
            new PerLanguageOption<StringSet>(
                AnalyzerMoniker, nameof(AcceptableVersionProperties), defaultValue: () => DefaultAcceptableVersionProperties);

        // This rule configuration parameter specifies whether the informationUri property (which
        // helps the responsible developer learn more about the tool that produced the result) is
        // required.
        public static PerLanguageOption<bool> InformationUriRequired =>
            new PerLanguageOption<bool>(
                AnalyzerMoniker, nameof(InformationUriRequired), defaultValue: () => true);

        private static readonly string AnalyzerMoniker = MakeAnalyzerMoniker(RuleId.ProvideToolProperties, nameof(RuleId.ProvideToolProperties));

        // We instantiate this object just so we can access its property names below. It's internal
        // rather than private so we can do the same thing in the tests.
        internal static readonly ToolComponent s_dummyToolComponent = new ToolComponent();

        private static StringSet DefaultAcceptableVersionProperties =>
            new StringSet(
                new string[]
                {
                    nameof(s_dummyToolComponent.Version),
                    nameof(s_dummyToolComponent.SemanticVersion),
                    nameof(s_dummyToolComponent.DottedQuadFileVersion)
                }
            );

        private static readonly Regex s_versionRegex = new Regex(@"^\d+.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        protected override void Analyze(Tool tool, string toolPointer)
        {
            if (tool.Driver != null)
            {
                AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            if (!string.IsNullOrEmpty(toolComponent.Name))
            {
                const int MaxWords = 3;
                int wordCount = toolComponent.Name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount > MaxWords)
                {
                    string driverNamePointer = toolDriverPointer.AtProperty(SarifPropertyName.Name);

                    // {0}: The tool name '{1}' contains {2} words, which is more than the recommended
                    // maximum of {3} words. A short tool name is easy to remember and fits into a
                    // narrow column when displaying a list of results. If you need to provide more
                    // information about your tool, use the 'fullName' property.
                    LogResult(
                        driverNamePointer,
                        nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_ProvideConciseToolName_Text),
                        toolComponent.Name,
                        wordCount.ToString(),
                        MaxWords.ToString());
                }
            }

            bool informationUriRequired = this.Context.Policy.GetProperty(InformationUriRequired);
            if (informationUriRequired && toolComponent.InformationUri == null)
            {
                // {0}: The tool '{1}' does not provide 'informationUri'. This property helps the
                // developer responsible for addessing a result by providing a way to learn more
                // about the tool.
                LogResult(
                    toolDriverPointer,
                    nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_ProvideToolnformationUri_Text),
                    toolComponent.Name);
            }

            StringSet acceptableVersionProperties = this.Context.Policy.GetProperty(AcceptableVersionProperties);
            bool toolDriverProvidesVersion = false;
            toolDriverProvidesVersion |= acceptableVersionProperties.Contains(nameof(toolComponent.Version)) && !string.IsNullOrWhiteSpace(toolComponent.Version);
            toolDriverProvidesVersion |= acceptableVersionProperties.Contains(nameof(toolComponent.SemanticVersion)) && !string.IsNullOrWhiteSpace(toolComponent.SemanticVersion);
            toolDriverProvidesVersion |= acceptableVersionProperties.Contains(nameof(toolComponent.DottedQuadFileVersion)) && !string.IsNullOrWhiteSpace(toolComponent.DottedQuadFileVersion);

            if (!toolDriverProvidesVersion)
            {
                // {0}: The tool '{1}' does not provide any of the version-related properties {2}.
                // Providing version information enables the log file consumer to determine whether
                // the file was produced by an up to date version, and to avoid accidentally
                // comparing log files produced by different tool versions.
                LogResult(
                    toolDriverPointer,
                    nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_ProvideToolVersion_Text),
                    toolComponent.Name,
                    $"'{string.Join("', '", acceptableVersionProperties.Select(ToCamelCase))}'");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(toolComponent.Version))
                {
                    AnalyzeVersion(toolComponent.Name, toolComponent.Version, toolDriverPointer.AtProperty(SarifPropertyName.Version));
                }
            }
        }

        private void AnalyzeVersion(string name, string version, string pointer)
        {
            if (!s_versionRegex.IsMatch(version))
            {
                // {0}: The tool '{1}' contains the 'version' property '{2}', which is not numeric.
                // To facilitate comparison between versions, specify a 'version' that starts with
                // an integer, optionally followed by any desired characters.
                LogResult(
                    pointer,
                    nameof(RuleResources.SARIF2005_ProvideToolProperties_Warning_UseNumericToolVersions_Text),
                    name,
                    version);
            }
        }

        private static string ToCamelCase(string name)
            => name == null
                ? throw new ArgumentNullException(nameof(name))
                : name.Length == 1
                ? name.ToLowerInvariant()
                : $"{name.Substring(0, 1).ToLowerInvariant()}{name.Substring(1)}";
    }
}
