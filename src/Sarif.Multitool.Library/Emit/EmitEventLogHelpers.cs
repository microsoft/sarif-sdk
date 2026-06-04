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
    /// Shared plumbing for the emit verb chain (<c>emit-run</c>, <c>add-result</c>,
    /// <c>add-invocation</c>, <c>add-notification-reporting-descriptor</c>,
    /// <c>add-rule-reporting-descriptor</c>, <c>emit-finalize</c>): resolves
    /// the staged event log path, reads caller-supplied JSON (file or stdin), and parses it into
    /// a <see cref="JToken"/> in a date-safe way.
    /// </summary>
    /// <remarks>
    /// Shared helpers preserve payload text, including date-looking strings, until the staged
    /// event log is finalized.
    /// </remarks>
    internal static class EmitEventLogHelpers
    {
        internal const string WipSuffix = ".wip.jsonl";

        // Public documentation and repository URIs must use HTTPS.
        internal static readonly string[] DocumentationUriSchemes = new[] { Uri.UriSchemeHttps };

        // Source-root base URIs may point at a hosted source view or a local checkout.
        internal static readonly string[] BaseUriSchemes = new[] { Uri.UriSchemeHttps, Uri.UriSchemeFile };

        /// <summary>
        /// Validates that <paramref name="value"/> is either null/empty or a well-formed
        /// absolute URI whose scheme appears in <paramref name="allowedSchemes"/>.
        /// </summary>
        /// <remarks>
        /// Empty values are accepted because the corresponding flags are optional. Non-empty
        /// values must be absolute and use an allowed scheme.
        /// </remarks>
        internal static bool TryValidateUri(string value, string flagName, string[] allowedSchemes, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = null;
                return true;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value '{1}' is not a well-formed absolute URI.",
                    flagName,
                    value);
                return false;
            }

            bool schemeAllowed = false;
            for (int i = 0; i < allowedSchemes.Length; i++)
            {
                if (string.Equals(uri.Scheme, allowedSchemes[i], StringComparison.Ordinal))
                {
                    schemeAllowed = true;
                    break;
                }
            }

            if (!schemeAllowed)
            {
                errorMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} value '{1}' uses scheme '{2}'; expected one of: {3}.",
                    flagName,
                    value,
                    uri.Scheme,
                    string.Join(", ", allowedSchemes));
                return false;
            }

            errorMessage = null;
            return true;
        }

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
                        "Event log '{0}' does not exist; run 'emit-run' first.",
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
                // Preserve ISO-8601 text exactly; Json.NET otherwise normalizes date-looking strings.
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
        /// Reads redirected stdin as UTF-8, bypassing <see cref="Console.InputEncoding"/> so
        /// Windows OEM codepages cannot mangle non-ASCII SARIF payloads. A UTF-8 BOM is honored.
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
