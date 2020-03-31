// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;

namespace SarifToCsv
{
    /// <summary>
    ///  SarifCsvColumnWriters builds and returns methods which know how to write different Result columns
    ///  to a CsvWriter.
    /// </summary>
    public static class SarifCsvColumnWriters
    {
        private static readonly Dictionary<string, Action<WriteContext>> _writers;

        static SarifCsvColumnWriters()
        {
            _writers = BuildWriters();
        }

        public static IEnumerable<string> SupportedColumns => _writers.Keys;

        /// <summary>
        ///  Get the writer for a given column, which takes a WriteContext and will output the column
        ///  for the current Result.
        /// </summary>
        /// <param name="columnName">ColumnName to write</param>
        /// <returns>Writer function for column; throws if no writer available for column name</returns>
        public static Action<WriteContext> GetWriter(string columnName)
        {
            Action<WriteContext> writer;

            if (_writers.TryGetValue(columnName, out writer))
            {
                return writer;
            }
            else if (columnName.StartsWith("Properties.", StringComparison.OrdinalIgnoreCase))
            {
                string propertyName = columnName.Substring("Properties.".Length);
                return (c) =>
                {
                    string value = "";

                    if (c.Result.PropertyNames.Contains(propertyName))
                    {
                        value = c.Result.GetSerializedPropertyValue(propertyName) ?? "";
                        if (value.StartsWith("\"")) { value = JsonConvert.DeserializeObject<string>(value); }
                    }

                    c.Writer.Write(value);
                };
            }
            else if (columnName.StartsWith("PartialFingerprints.", StringComparison.OrdinalIgnoreCase))
            {
                string propertyName = columnName.Substring("PartialFingerprints.".Length);
                return (c) =>
                {
                    string value;

                    if (!c.Result.PartialFingerprints.TryGetValue(propertyName, out value))
                    {
                        value = "";
                    }
                    
                    c.Writer.Write(value);
                };
            }
            else if (columnName.StartsWith("Fingerprints.", StringComparison.OrdinalIgnoreCase))
            {
                string propertyName = columnName.Substring("Fingerprints.".Length);
                return (c) =>
                {
                    string value;

                    if (!c.Result.Fingerprints.TryGetValue(propertyName, out value))
                    {
                        value = "";
                    }

                    c.Writer.Write(value);
                };
            }
            else
            {
                throw new ArgumentException($"SarifToCsv doesn't know how to write column name \"{columnName}\". Valid names:\r\n\t{String.Join("\r\n\t", _writers.Keys)}");
            }
        }

        private static Dictionary<string, Action<WriteContext>> BuildWriters()
        {
            Dictionary<string, Action<WriteContext>> writers = new Dictionary<string, Action<WriteContext>>(StringComparer.OrdinalIgnoreCase);

            // Result Basic Properties
            writers["BaselineState"] = (c) => { c.Writer.Write(c.Result.BaselineState.ToString()); };
            writers["CorrelationGuid"] = (c) => { c.Writer.Write(c.Result.CorrelationGuid ?? ""); };
            writers["Fingerprints"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.Fingerprints?.Values ?? Array.Empty<string>())); };
            writers["HostedViewerUri"] = (c) => { c.Writer.Write(c.Result.HostedViewerUri?.ToString() ?? ""); };
            writers["Guid"] = (c) => { c.Writer.Write(c.Result.Guid ?? ""); };
            writers["Kind"] = (c) => { c.Writer.Write(c.Result.Kind.ToString()); };
            writers["Level"] = (c) => { c.Writer.Write(c.Result.Level.ToString()); };
            writers["Message.Text"] = (c) => { c.Writer.Write(c.Result.Message?.Text ?? ""); };
            writers["OccurrenceCount"] = (c) => { c.Writer.Write(c.Result.OccurrenceCount); };
            writers["PartialFingerprints"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.PartialFingerprints?.Values ?? Array.Empty<string>())); };
            writers["Provenance"] = (c) => { c.Writer.Write(c.Result.Provenance?.ToString() ?? ""); };
            writers["Rank"] = (c) => { c.Writer.Write(c.Result.Rank.ToString()); };
            writers["RuleId"] = (c) => { c.Writer.Write(c.Result.GetRule(c.Run).Id ?? ""); };
            writers["RuleIndex"] = (c) => { c.Writer.Write(c.Result.RuleIndex); };
            writers["Suppressions"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.Suppressions?.Select(s => $"{s.Kind}|{s.Location}" ?? "") ?? Array.Empty<string>())); };
            writers["Tags"] = (c) => { c.Writer.Write(String.Join("; ", ((IEnumerable<string>)c.Result.Tags) ?? Array.Empty<string>())); };
            writers["WorkItemUris"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.WorkItemUris?.Select((uri) => uri.ToString()) ?? Array.Empty<string>())); };

            // (Formatted) Message
            writers["Message"] = (c) => { c.Writer.Write(c.Result.GetMessageText(c.Result.GetRule(c.Run))); };

            // Properties
            writers["Properties"] = WriteProperties;

            // PhysicalLocation Properties
            writers["Location.Tags"] = (c) => { c.Writer.Write(String.Join("; ", ((IEnumerable<string>)c.PLoc?.Tags) ?? Array.Empty<string>())); };
            writers["Location.Uri"] = (c) => { c.Writer.Write(c.PLoc?.ArtifactLocation?.FileUri(c.Run)?.ToString() ?? ""); };

            // Region Properties
            writers["Location.Region.ByteLength"] = (c) => { c.Writer.Write(c.PLoc?.Region?.ByteLength ?? -1); };
            writers["Location.Region.ByteOffset"] = (c) => { c.Writer.Write(c.PLoc?.Region?.ByteOffset ?? -1); };
            writers["Location.Region.CharLength"] = (c) => { c.Writer.Write(c.PLoc?.Region?.CharLength ?? -1); };
            writers["Location.Region.CharOffset"] = (c) => { c.Writer.Write(c.PLoc?.Region?.CharOffset ?? -1); };
            writers["Location.Region.EndColumn"] = (c) => { c.Writer.Write(c.PLoc?.Region?.EndColumn ?? -1); };
            writers["Location.Region.EndLine"] = (c) => { c.Writer.Write(c.PLoc?.Region?.EndLine ?? -1); };
            writers["Location.Region.IsBinaryRegion"] = (c) => { c.Writer.Write(c.PLoc?.Region?.IsBinaryRegion.ToString() ?? ""); };
            writers["Location.Region.Message.Text"] = (c) => { c.Writer.Write(c.PLoc?.Region?.Message?.Text ?? ""); };
            writers["Location.Region.Snippet.Text"] = (c) => { c.Writer.Write(c.PLoc?.Region?.Snippet?.Text ?? ""); };
            writers["Location.Region.SourceLanguage"] = (c) => { c.Writer.Write(c.PLoc?.Region?.SourceLanguage ?? ""); };
            writers["Location.Region.StartColumn"] = (c) => { c.Writer.Write(c.PLoc?.Region?.StartColumn ?? -1); };
            writers["Location.Region.StartLine"] = (c) => { c.Writer.Write(c.PLoc?.Region?.StartLine ?? -1); };
            writers["Location.Region.Tags"] = (c) => { c.Writer.Write(String.Join("; ", ((IEnumerable<string>)c.PLoc?.Region?.Tags) ?? Array.Empty<string>())); };

            // Run Identity Properties
            writers["Run.BaselineGuid"] = (c) => { c.Writer.Write(c.Run?.BaselineGuid ?? ""); };
            writers["Run.AutomationDetails.CorrelationGuid"] = (c) => { c.Writer.Write(c.Run?.AutomationDetails?.CorrelationGuid ?? ""); };
            writers["Run.AutomationDetails.Id"] = (c) => { c.Writer.Write(c.Run?.AutomationDetails?.Id ?? ""); };
            writers["Run.AutomationDetails.Guid"] = (c) => { c.Writer.Write(c.Run?.AutomationDetails?.Guid ?? ""); };

            // Run Basics
            writers["Run.Tool.Name"] = (c) => { c.Writer.Write(c.Run?.Tool?.Driver?.Name ?? ""); };

            // Run and Result Index (alternate identity if Guids not provided)
            writers["RunIndex"] = (c) => { c.Writer.Write(c.RunIndex); };
            writers["ResultIndex"] = (c) => { c.Writer.Write(c.ResultIndex); };


            return writers;
        }

        private static void WriteProperties(WriteContext c)
        {
            StringBuilder result = new StringBuilder();

            foreach (string propertyName in c?.Result?.PropertyNames ?? Enumerable.Empty<string>())
            {
                if (result.Length > 0) { result.Append("; "); }
                result.Append(propertyName);
                result.Append(": ");
                result.Append(c.Result.GetSerializedPropertyValue(propertyName) ?? "<null>");
            }

            c.Writer.Write(result.ToString());
        }
    }
}
