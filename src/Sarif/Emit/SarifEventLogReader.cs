// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Forward-only reader for a SARIF event log.
    /// </summary>
    /// <remarks>
    /// <para>Tolerates both LF and CRLF line endings (per JSONL convention; emits LF). Tolerates a
    /// single optional UTF-8 BOM at the start of the stream; rejects BOM elsewhere.</para>
    /// <para>Unknown event <c>kind</c> values at the current schema version are skipped (forward
    /// compatibility). Unknown schema <c>v</c> on a known kind is fatal.</para>
    /// <para>Malformed JSON on any line is fatal; the reader reports the 1-based line number and
    /// reason.</para>
    /// </remarks>
    public sealed class SarifEventLogReader
    {
        private static readonly HashSet<string> s_knownKinds = new HashSet<string>(StringComparer.Ordinal)
        {
            SarifEventKinds.RunHeader,
            SarifEventKinds.Result,
            SarifEventKinds.Notification,
            SarifEventKinds.Invocation,
            SarifEventKinds.RuleDescriptor,
            SarifEventKinds.NotificationDescriptor,
        };

        /// <summary>
        /// Streams events from the given path. Unknown kinds at supported schema versions are
        /// silently skipped. Unknown <c>v</c> for a known kind throws.
        /// </summary>
        public IEnumerable<SarifEvent> Read(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Event log path must be supplied.", nameof(path));
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                int lineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    SarifEvent sarifEvent;
                    try
                    {
                        sarifEvent = JsonConvert.DeserializeObject<SarifEvent>(line);
                    }
                    catch (JsonException ex)
                    {
                        throw new SarifEventLogException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Malformed JSON on line {0} of event log '{1}': {2}",
                                lineNumber,
                                path,
                                ex.Message),
                            ex);
                    }

                    if (sarifEvent == null || string.IsNullOrEmpty(sarifEvent.Kind))
                    {
                        throw new SarifEventLogException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Event on line {0} of event log '{1}' is missing a 'kind'.",
                                lineNumber,
                                path));
                    }

                    if (sarifEvent.Version != SarifEventKinds.CurrentSchemaVersion)
                    {
                        if (s_knownKinds.Contains(sarifEvent.Kind))
                        {
                            throw new SarifEventLogException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Event on line {0} of event log '{1}' has schema version {2} for known kind '{3}'; this reader supports version {4} only.",
                                    lineNumber,
                                    path,
                                    sarifEvent.Version,
                                    sarifEvent.Kind,
                                    SarifEventKinds.CurrentSchemaVersion));
                        }

                        // Unknown kind at unknown version: forward-compatible; skip.
                        continue;
                    }

                    if (!s_knownKinds.Contains(sarifEvent.Kind))
                    {
                        // Known schema version, unknown kind: forward-compatible extension; skip.
                        continue;
                    }

                    if (sarifEvent.Payload == null)
                    {
                        sarifEvent.Payload = new JObject();
                    }

                    yield return sarifEvent;
                }
            }
        }
    }
}
