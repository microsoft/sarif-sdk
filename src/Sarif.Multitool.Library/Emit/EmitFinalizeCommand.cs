// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;
using Microsoft.CodeAnalysis.Sarif.Taxonomies;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>multitool emit-finalize</c>: replays <c>&lt;output&gt;.wip.jsonl</c>,
    /// optionally enriches CWE-as-rule-id descriptors, and atomically writes the destination
    /// SARIF file.
    /// </summary>
    public class EmitFinalizeCommand : CommandBase
    {
        public int Run(EmitFinalizeOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (string.IsNullOrWhiteSpace(options?.OutputFilePath))
                {
                    Console.Error.WriteLine("Output SARIF path is required.");
                    return FAILURE;
                }

                string outputPath = Path.GetFullPath(options.OutputFilePath);
                string wipPath = outputPath + ".wip.jsonl";

                if (!fileSystem.FileExists(wipPath))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Event log '{0}' does not exist; run 'emit-init-run' first.",
                            wipPath));
                    return FAILURE;
                }

                SarifLog log = SarifEventReplayer.Replay(wipPath);

                if (!options.NoCweEnrichment && log?.Runs != null)
                {
                    foreach (Run run in log.Runs)
                    {
                        CweTaxonomyEnricher.Enrich(run);
                    }
                }

                Formatting formatting = options.Minify ? Formatting.None : Formatting.Indented;
                AtomicSarifWriter.Write(outputPath, stream =>
                {
                    using var sw = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                    using var jw = new JsonTextWriter(sw) { Formatting = formatting };
                    var serializer = JsonSerializer.Create(new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = formatting,
                    });
                    serializer.Serialize(jw, log);
                });

                int resultCount = 0;
                int ruleCount = 0;
                if (log?.Runs != null)
                {
                    foreach (Run r in log.Runs)
                    {
                        if (r.Results != null) { resultCount += r.Results.Count; }
                        if (r.Tool?.Driver?.Rules != null) { ruleCount += r.Tool.Driver.Rules.Count; }
                    }
                }

                Console.Out.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Wrote '{0}' ({1} result(s), {2} rule(s)).",
                        outputPath,
                        resultCount,
                        ruleCount));

                if (!options.KeepWip)
                {
                    try
                    {
                        fileSystem.FileDelete(wipPath);
                    }
                    catch (Exception delEx)
                    {
                        // Non-fatal: SARIF was successfully written; failing to remove the wip is
                        // a janitorial issue worth reporting but not worth aborting.
                        Console.Error.WriteLine(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Warning — could not delete '{0}': {1}",
                                wipPath,
                                delEx.Message));
                    }
                }

                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }
    }
}
