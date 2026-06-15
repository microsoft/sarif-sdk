// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Describes a single batch element the verb refused to append, identified by its
    /// position in the submitted payload.
    /// </summary>
    internal sealed class BatchElementError
    {
        internal BatchElementError(int index, string message, string errorCode = null)
        {
            Index = index;
            Message = message;
            ErrorCode = errorCode;
        }

        /// <summary>The element's zero-based position in the submitted array (0 for a lone object).</summary>
        internal int Index { get; }

        /// <summary>An optional machine-readable code (e.g. <c>AI1012</c>) for the rejection.</summary>
        internal string ErrorCode { get; }

        /// <summary>A human/AI-consumable description of why the element was rejected.</summary>
        internal string Message { get; }
    }

    /// <summary>
    /// Validates a single batch element and may mutate it in place (e.g. stamping a default).
    /// Returns <c>null</c> when the element is acceptable; otherwise the error describing the
    /// rejection.
    /// </summary>
    /// <param name="element">The element to validate.</param>
    /// <param name="index">The element's zero-based position in the submitted payload.</param>
    /// <param name="batched">
    /// <c>true</c> when the payload arrived as a JSON array (batch submission); <c>false</c> when
    /// it arrived as a lone JSON object. A validator that defaults a field from receipt time uses
    /// this to refuse the default under batch submission, where one write instant cannot stand in
    /// for many elements assembled after the fact.
    /// </param>
    internal delegate BatchElementError ValidateBatchElement(JObject element, int index, bool batched);

    /// <summary>
    /// Shared orchestration for the polymorphic <c>add-*</c> emit verbs. Each verb accepts a single
    /// JSON object or an array of objects, validates every element atomically, and appends all or
    /// none: if any element is rejected the staged event log is left untouched. The outcome is
    /// reported as a structured JSON document on stdout
    /// (<c>{ "appended": N, "rejected": [ { "index": i, "errorCode": "...", "message": "..." } ] }</c>),
    /// so an AI orchestrator can correct the offending elements and retry idempotently.
    /// </summary>
    internal static class EmitBatchProcessor
    {
        internal static int Run(
            string outputFilePath,
            string inputFilePath,
            string payloadKind,
            string eventKind,
            IFileSystem fileSystem,
            Func<string, ValidateBatchElement> buildValidator)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                int code = EmitEventLogHelpers.TryResolveWipPath(outputFilePath, fileSystem, out string wipPath);
                if (code != CommandBase.SUCCESS) { return code; }

                code = EmitEventLogHelpers.TryReadJsonToken(inputFilePath, payloadKind, fileSystem, out JToken token);
                if (code != CommandBase.SUCCESS) { return code; }

                var elements = new List<JObject>();
                var errors = new List<BatchElementError>();
                bool batched;

                if (token.Type == JTokenType.Object)
                {
                    batched = false;
                    elements.Add((JObject)token);
                }
                else if (token.Type == JTokenType.Array)
                {
                    batched = true;
                    int i = 0;
                    foreach (JToken item in (JArray)token)
                    {
                        if (item.Type == JTokenType.Object)
                        {
                            elements.Add((JObject)item);
                        }
                        else
                        {
                            // Keep the index aligned so the report cites the submitted position.
                            elements.Add(null);
                            errors.Add(new BatchElementError(
                                i,
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    "{0} batch element must be a JSON object, but element {1} was a JSON {2}.",
                                    Capitalize(payloadKind),
                                    i,
                                    item.Type.ToString().ToLowerInvariant())));
                        }

                        i++;
                    }
                }
                else
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} JSON must be a JSON object or an array of objects, but the parsed payload was a {1}.",
                            Capitalize(payloadKind),
                            token.Type.ToString().ToLowerInvariant()));
                    return CommandBase.FAILURE;
                }

                ValidateBatchElement validate = buildValidator(wipPath);

                for (int index = 0; index < elements.Count; index++)
                {
                    JObject element = elements[index];
                    if (element == null) { continue; } // already captured as a structural error

                    BatchElementError error = validate(element, index, batched);
                    if (error != null) { errors.Add(error); }
                }

                // Atomic: any rejection appends nothing, so a retry of the corrected payload never
                // double-appends the elements that were already valid.
                if (errors.Count > 0)
                {
                    WriteReport(appended: 0, errors);
                    return CommandBase.FAILURE;
                }

                if (elements.Count > 0)
                {
                    using var writer = new SarifEventLogWriter(wipPath);
                    foreach (JObject element in elements)
                    {
                        writer.Append(eventKind, element);
                    }
                }

                WriteReport(elements.Count, errors);
                return CommandBase.SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return CommandBase.FAILURE;
            }
        }

        private static void WriteReport(int appended, List<BatchElementError> errors)
        {
            var rejected = new JArray(
                errors
                    .OrderBy(e => e.Index)
                    .Select(e =>
                    {
                        var entry = new JObject { ["index"] = e.Index };
                        if (!string.IsNullOrEmpty(e.ErrorCode)) { entry["errorCode"] = e.ErrorCode; }
                        entry["message"] = e.Message;
                        return entry;
                    })
                    .Cast<JToken>());

            var report = new JObject
            {
                ["appended"] = appended,
                ["rejected"] = rejected,
            };

            Console.Out.WriteLine(report.ToString(Formatting.Indented));
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}
