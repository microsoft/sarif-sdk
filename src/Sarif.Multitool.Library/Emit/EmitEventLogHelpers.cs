// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Shared plumbing for the emit verb chain (<c>emit-init-run</c>, <c>add-result</c>,
    /// <c>add-notification</c>, <c>emit-finalize</c>): resolves the staged event log path,
    /// reads caller-supplied JSON (file or stdin), and parses it into a
    /// <see cref="JToken"/> in a date-safe way.
    /// </summary>
    /// <remarks>
    /// The verbs share three concerns — locating <c>&lt;output&gt;.wip.jsonl</c>, sourcing
    /// the payload, and parsing it without lossy normalization — which live here so the
    /// per-verb commands can stay focused on payload-specific validation and append.
    /// </remarks>
    internal static class EmitEventLogHelpers
    {
        internal const string WipSuffix = ".wip.jsonl";

        /// <summary>
        /// Resolves the staged event-log path for an output SARIF path and verifies it exists.
        /// </summary>
        /// <param name="outputFilePath">The final SARIF file path (positional verb argument).</param>
        /// <param name="fileSystem">The file system facade.</param>
        /// <param name="wipPath">Set to the absolute event-log path on success.</param>
        /// <returns><see cref="CommandBase.SUCCESS"/> on success, <see cref="CommandBase.FAILURE"/>
        /// with a stderr message otherwise.</returns>
        internal static int TryResolveWipPath(string outputFilePath, IFileSystem fileSystem, out string wipPath)
        {
            wipPath = null;

            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                Console.Error.WriteLine("Output SARIF path is required.");
                return CommandBase.FAILURE;
            }

            string outputPath = Path.GetFullPath(outputFilePath);
            wipPath = outputPath + WipSuffix;

            if (!fileSystem.FileExists(wipPath))
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Event log '{0}' does not exist; run 'emit-init-run' first.",
                        wipPath));
                return CommandBase.FAILURE;
            }

            return CommandBase.SUCCESS;
        }

        /// <summary>
        /// Reads the caller-supplied JSON from <paramref name="inputFilePath"/> or stdin and
        /// parses it. Returns <see cref="CommandBase.SUCCESS"/> with <paramref name="payload"/>
        /// populated, or <see cref="CommandBase.FAILURE"/> with a stderr message describing
        /// what went wrong.
        /// </summary>
        /// <param name="inputFilePath">File path supplied by <c>--input</c>, or null/empty to
        /// read from stdin.</param>
        /// <param name="payloadKind">Human-readable label used in error messages ("result",
        /// "notification", ...).</param>
        /// <param name="fileSystem">The file system facade.</param>
        /// <param name="payload">Set to the parsed payload on success.</param>
        internal static int TryReadJsonPayload(
            string inputFilePath,
            string payloadKind,
            IFileSystem fileSystem,
            out JToken payload)
        {
            payload = null;
            string json;

            if (!string.IsNullOrEmpty(inputFilePath))
            {
                if (!fileSystem.FileExists(inputFilePath))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Input file '{0}' does not exist.",
                            inputFilePath));
                    return CommandBase.FAILURE;
                }

                json = fileSystem.FileReadAllText(inputFilePath);
            }
            else
            {
                if (!Console.IsInputRedirected)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Provide --input <path> or pipe the {0} JSON on stdin.",
                            payloadKind));
                    return CommandBase.FAILURE;
                }

                json = ReadStandardInputAsUtf8();
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} JSON is empty.",
                        Capitalize(payloadKind)));
                return CommandBase.FAILURE;
            }

            try
            {
                // DateParseHandling.None is critical: SARIF payloads contain ISO-8601 strings in
                // many places (timeUtc, properties bags, exception traces, custom evidence
                // fields) which Json.NET's default DateParseHandling would silently convert to
                // System.DateTime and re-serialize with a normalized format on the way back
                // out. We must preserve the producer's exact text.
                using var sr = new StringReader(json);
                using var jr = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None };
                payload = JToken.ReadFrom(jr);
            }
            catch (JsonReaderException ex)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} JSON is malformed: {1}",
                        Capitalize(payloadKind),
                        ex.Message));
                return CommandBase.FAILURE;
            }

            if (payload.Type != JTokenType.Object)
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} JSON must be a JSON object, but the parsed payload was a {1}.",
                        Capitalize(payloadKind),
                        payload.Type.ToString().ToLowerInvariant()));
                return CommandBase.FAILURE;
            }

            return CommandBase.SUCCESS;
        }

        /// <summary>
        /// Reads redirected stdin as UTF-8, bypassing <see cref="Console.InputEncoding"/>.
        /// On Windows the console's default input encoding is the active OEM codepage
        /// (often cp437 or cp850), which would mangle non-ASCII content in a piped
        /// SARIF payload. AI orchestrators routinely emit messages, URIs, and properties
        /// containing non-ASCII characters, so we must decode the raw byte stream as UTF-8
        /// regardless of the console's current code page. A BOM-stamped input is still
        /// honored — <see cref="StreamReader"/>'s detect-BOM flag handles that case.
        /// </summary>
        private static string ReadStandardInputAsUtf8()
        {
            using Stream stdin = Console.OpenStandardInput();
            using var reader = new StreamReader(
                stdin,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
