// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// CLI shell for the polymorphic <c>add-*</c> emit verbs. Resolves the staged event log path,
    /// reads the caller-supplied JSON (file or stdin), drives a <see cref="RunEmitContext"/> over a
    /// <see cref="FileEmitSink"/>, and renders the resulting <see cref="EmitReport"/>. The verb's
    /// value — element validation and all-or-none append semantics — lives in
    /// <see cref="RunEmitContext"/>; this shell owns only process I/O and exit-code mapping.
    /// </summary>
    /// <remarks>
    /// A whole-payload rejection (<see cref="EmitReport.PayloadError"/>) is written to stderr; per-
    /// element rejections are rendered as a structured JSON document on stdout
    /// (<c>{ "appended": N, "rejected": [ { "index": i, "errorCode": "...", "message": "..." } ] }</c>),
    /// so an AI orchestrator can correct the offending elements and retry idempotently.
    /// </remarks>
    internal static class EmitBatchProcessor
    {
        internal static int Run(
            string outputFilePath,
            string inputFilePath,
            string payloadKind,
            IFileSystem fileSystem,
            Func<RunEmitContext, JToken, EmitReport> apply)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                int code = EmitEventLogHelpers.TryResolveWipPath(outputFilePath, fileSystem, out string wipPath);
                if (code != CommandBase.SUCCESS) { return code; }

                code = EmitEventLogHelpers.TryReadJsonToken(inputFilePath, payloadKind, fileSystem, out JToken token);
                if (code != CommandBase.SUCCESS) { return code; }

                EmitReport report;
                using (var sink = new FileEmitSink(wipPath))
                {
                    report = apply(new RunEmitContext(sink), token);
                }

                if (report.PayloadError != null)
                {
                    Console.Error.WriteLine(report.PayloadError);
                    return CommandBase.FAILURE;
                }

                WriteReport(report);
                return report.Rejected.Count > 0 ? CommandBase.FAILURE : CommandBase.SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return CommandBase.FAILURE;
            }
        }

        private static void WriteReport(EmitReport report)
        {
            var rejected = new JArray(
                report.Rejected
                    .Select(e =>
                    {
                        var entry = new JObject { ["index"] = e.Index };
                        if (!string.IsNullOrEmpty(e.ErrorCode)) { entry["errorCode"] = e.ErrorCode; }
                        entry["message"] = e.Message;
                        return entry;
                    })
                    .Cast<JToken>());

            var document = new JObject
            {
                ["appended"] = report.Appended,
                ["rejected"] = rejected,
            };

            Console.Out.WriteLine(document.ToString(Formatting.Indented));
        }
    }
}
