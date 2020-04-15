// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Converters.TextFormats;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts a log file from the Semmle format to the SARIF format.
    /// </summary>
    public class SemmleQLConverter : ToolFileConverterBase
    {
        // The fields are as follows:
        private enum FieldIndex
        {
            QueryName,
            QueryDescription,
            Severity,
            Message,
            RelativePath,
            Path,
            StartLine,
            StartColumn,
            EndLine,
            EndColumn
        }

        private CsvReader _parser;
        private List<Notification> _toolNotifications;

        public override string ToolName { get { return "Semmle QL"; } }

        /// <summary>
        /// Converts a Semmle log file in CSV format to a SARIF log file.
        /// </summary>
        /// <param name="input">
        /// Input stream from which to read the Semmle log.
        /// </param>
        /// <param name="output">
        /// Output string to which to write the SARIF log.
        /// </param>
        /// <param name="dataToInsert">Optionally emitted properties that should be written to log.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are null.
        /// </exception>
        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            _toolNotifications = new List<Notification>();

            List<Result> results = GetResultsFromStream(input);

            PersistResults(output, results);

            if (_toolNotifications.HasAtLeastOneNonNullValue())
            {
                output.WriteInvocations(
                    new[] { new Invocation
                    {
                        ToolExecutionNotifications = _toolNotifications
                    } });
            }
        }

        private List<Result> GetResultsFromStream(Stream input)
        {
            var results = new List<Result>();

            using (_parser = new CsvReader(input))
            {
                while (_parser.NextRow())
                {
                    results.Add(ParseResult(_parser.Current()));
                }
            }

            return results;
        }

        private Result ParseResult(List<string> fields)
        {
            string rawMessage = fields[(int)FieldIndex.Message];
            string normalizedMessage;
            IList<Location> relatedLocations = NormalizeRawMessage(rawMessage, out normalizedMessage);

            Region region = MakeRegion(fields);
            var result = new Result
            {
                Message = new Message { Text = normalizedMessage },
                Locations = new Location[]
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(GetString(fields, FieldIndex.RelativePath), UriKind.Relative),
                                UriBaseId = "$srcroot"
                            },
                            Region = region
                        }
                    }
                },
                RelatedLocations = relatedLocations
            };

            FailureLevel level = FailureLevelFromSemmleSeverity(GetString(fields, FieldIndex.Severity));
            if (level != FailureLevel.Warning)
            {
                result.Level = level;
            }

            return result;
        }

        private IList<Location> NormalizeRawMessage(string rawMessage, out string normalizedMessage)
        {
            // The rawMessage contains embedded related locations. We need to extract the related locations and reformat the rawMessage embedded links wrapped in [brackets].
            // Example rawMessage
            //     po (coming from [["hbm"|"relative://code/.../file1.cxx:176:4882:3"],["hbm"|"relative://code/.../file2.c:1873:50899:3"],["hbm"|"relative://code/.../file2.c:5783:154466:3"]]) may not have been checked for validity before call to vSync.
            // Example normalizedMessage, where 'id' is the related location id to link to
            //   Note: the first link in the message links to the first related location in the list, the second link to the second, etc.
            //     po (coming from [hbm](id)) may not have been checked for validity before call to vSync.
            // Example relatedLocations
            //     relative://code/.../file1.cxx:176:4882:3
            //     relative://code/.../file2.c:1873:50899:3
            //     relative://code/.../file2.c:5783:154466:3
            List<Location> relatedLocations = null;

            var sb = new StringBuilder();

            int count = 0;
            int linkIndex = 0;
            int index = rawMessage.IndexOf("[[");
            while (index > -1)
            {
                sb.Append(rawMessage.Substring(0, index));

                rawMessage = rawMessage.Substring(index + 2);

                index = rawMessage.IndexOf("]]");

                // embeddedLinksText contains the text for one set of embedded links except for the leading '[[' and trailing ']]'
                // "hbm"|"relative:/code/.../file1.cxx:176:4882:3"],["hbm"|"relative://code/.../file2.c:1873:50899:3"],["hbm"|"relative://code/.../file2.c:5783:154466:3"
                string embeddedLinksText = rawMessage.Substring(0, index - 1);

                // embeddedLinks splits the set of embedded links into invividual links
                // 1.  "hbm"|"relative://code/.../file1.cxx:176:4882:3"
                // 2.  "hbm"|"relative://code/.../file2.c:1873:50899:3"
                // 3.  "hbm"|"relative://code/.../file2.c:5783:154466:3"

                string[] embeddedLinks = embeddedLinksText.Split(new string[] { "],[" }, StringSplitOptions.None);

                foreach (string embeddedLink in embeddedLinks)
                {
                    string[] tokens = embeddedLink.Split(new char[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);

                    // save the text portion of the link
                    embeddedLinksText = tokens[0];

                    string location = tokens[2];
                    string[] locationTokens = location.Split(':');

                    relatedLocations = relatedLocations ?? new List<Location>();
                    PhysicalLocation physicalLocation;

                    if (locationTokens[0].Equals("file", StringComparison.OrdinalIgnoreCase))
                    {
                        // Special case for file paths, e.g.:
                        // "IComparable"|"file://C:/Windows/Microsoft.NET/Framework/v2.0.50727/mscorlib.dll:0:0:0:0"
                        physicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri($"{locationTokens[0]}:{locationTokens[1]}:{locationTokens[2]}", UriKind.Absolute)
                            },
                            Region = new Region
                            {
                                StartLine = int.Parse(locationTokens[3]),
                                ByteOffset = int.Parse(locationTokens[4]),
                                ByteLength = int.Parse(locationTokens[5])
                            }
                        };
                    }
                    else
                    {
                        physicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(locationTokens[1].Substring(1), UriKind.Relative),
                                UriBaseId = "$srcroot"
                            },
                            Region = new Region
                            {
                                StartLine = int.Parse(locationTokens[2]),
                                ByteOffset = int.Parse(locationTokens[3]),
                                ByteLength = int.Parse(locationTokens[4])
                            }
                        };
                    }

                    // TODO: Region.ByteOffset is not being handled correctly in SemmleQlConverter. Fix this!
                    // https://github.com/Microsoft/sarif-sdk/issues/1458

                    if (physicalLocation.Region.ByteLength == 0)
                    {
                        physicalLocation.Region.ByteOffset = -1;
                    }

                    var relatedLocation = new Location
                    {
                        Id = ++count,
                        PhysicalLocation = physicalLocation
                    };

                    relatedLocations.Add(relatedLocation);
                }

                // Re-add the text portion of the link in brackets with the location id in parens, e.g. [link text](id)
                sb.Append($"[{embeddedLinksText}]({relatedLocations[linkIndex++].Id})");

                rawMessage = rawMessage.Substring(index + "]]".Length);
                index = rawMessage.IndexOf("[[");
            }

            sb.Append(rawMessage);
            normalizedMessage = sb.ToString();
            return relatedLocations;
        }

        /// <summary>
        /// Create a Region object that contains only those properties required by the
        /// SARIF spec.
        /// </summary>
        /// <param name="fields">
        /// Array of fields from a CSV record.
        /// </param>
        /// <returns>
        /// A Region object that contains only those properties required by the SARIF spec.
        /// </returns>
        private Region MakeRegion(List<string> fields)
        {
            Region region = new Region
            {
                StartLine = GetInteger(fields, FieldIndex.StartLine),
                StartColumn = GetInteger(fields, FieldIndex.StartColumn),
            };

            int endLine = GetInteger(fields, FieldIndex.EndLine);
            int endColumn = GetInteger(fields, FieldIndex.EndColumn);
            if (endLine != region.StartLine)
            {
                region.EndLine = endLine;
                region.EndColumn = endColumn;
            }
            else
            {
                if (endColumn != region.StartColumn)
                {
                    region.EndColumn = endColumn;
                }
            }

            return region;
        }

        private static string GetString(List<string> fields, FieldIndex fieldIndex)
        {
            return fields[(int)fieldIndex];
        }

        private int GetInteger(List<string> fields, FieldIndex fieldIndex)
        {
            string field = GetString(fields, fieldIndex);
            int value;
            if (!int.TryParse(field, out value))
            {
                value = 0;
                AddToolNotification(
                    "InvalidInteger",
                    FailureLevel.Error,
                    ConverterResources.SemmleInvalidInteger,
                    field,
                    fieldIndex);
            }

            return value;
        }

        private FailureLevel FailureLevelFromSemmleSeverity(string semmleSeverity)
        {
            switch (semmleSeverity)
            {
                case SemmleError:
                    return FailureLevel.Error;

                case SemmleWarning:
                    return FailureLevel.Warning;

                case SemmleRecommendation:
                    return FailureLevel.Note;

                default:
                    AddToolNotification(
                        "UnknownSeverity",
                        FailureLevel.Error,
                        ConverterResources.SemmleUnknownSeverity,
                        semmleSeverity);
                    return FailureLevel.Warning;
            }
        }

        private void AddToolNotification(
            string id,
            FailureLevel level,
            string messageFormat,
            params object[] args)
        {
            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            // When the parser read the offending line, it incremented the line number,
            // so report the previous line.
            long lineNumber = _parser.RowCountRead;
            string messageWithLineNumber = string.Format(
                CultureInfo.CurrentCulture,
                ConverterResources.SemmleNotificationFormat,
                lineNumber,
                message);

            _toolNotifications.Add(new Notification
            {
                Descriptor = new ReportingDescriptorReference
                {
                    Id = id,
                },
                TimeUtc = DateTime.UtcNow,
                Level = level,
                Message = new Message { Text = messageWithLineNumber }
            });
        }

        public const string SemmleError = "error";
        public const string SemmleWarning = "warning";
        public const string SemmleRecommendation = "recommendation";
    }
}
