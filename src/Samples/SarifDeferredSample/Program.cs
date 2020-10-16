using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace SarifDeferredSample
{
    /// <summary>
    ///  Sample of using SarifDeferredContractResolver to load very large Sarif files with low memory usage.
    /// </summary>
    internal class Program
    {
        private const float BytesPerMB = (float)(1024 * 1024);

        private static void Main(string[] args)
        {
            bool deferred = bool.Parse(args[0]);
            string filePath = args[1];
            
            SarifLog log = null;

            Console.WriteLine($"Loading {filePath}{(deferred ? " (deferred)" : "")}...");

            Measure(() =>
            {
                // Use 'SarifDeferredContractResolver' and 'JsonPositionedTextReader' to load a deferred version of the same object graph.
                JsonSerializer serializer = new JsonSerializer();

                if (deferred)
                {
                    serializer.ContractResolver = SarifDeferredContractResolver.Instance;
                }

                using (JsonTextReader reader = (deferred ? new JsonPositionedTextReader(filePath) : new JsonTextReader(new StreamReader(filePath))))
                {
                    log = serializer.Deserialize<SarifLog>(reader);
                }

                return $"Loaded {filePath} ({(new FileInfo(filePath).Length / BytesPerMB):n1} MB)";
            });

            // Enumerate collections as normal. Enumeration is efficient. Indexing to items works, but is slower, as a file seek is required per item read.
            Run run = log.Runs[0];
            Measure(() =>
            {
                int messageLengthTotal = 0;

                // Fastest: Enumerate
                foreach (Result result in run.Results)
                {
                    messageLengthTotal += result?.Message?.Text?.Length ?? 0;
                }

                // Slower: Count and indexer
                //int messageCount = run.Results.Count;
                //for (int i = 0; i < messageCount; ++i)
                //{
                //    Result result = run.Results[i];
                //    messageLengthTotal += result?.Message?.Text?.Length ?? 0;
                //}

                return $"Enumerated {run.Results.Count:n0} Results message total {messageLengthTotal / BytesPerMB:n0}MB";
            });

            Measure(() =>
            {
                int fileCount = 0;
                int uriLengthTotal = 0;

                if (run.Artifacts != null)
                {
                    // Fastest: Enumerate
                    foreach (Artifact artifact in run.Artifacts)
                    {
                        uriLengthTotal += artifact?.Location?.Uri?.OriginalString?.Length ?? 0;
                        fileCount++;
                    }

                    // Slower: Keys and indexer
                    //foreach (var key in run.Files.Keys)
                    //{
                    //    FileData file = run.Files[key];
                    //    uriLengthTotal += file?.ArtifactLocation?.Uri?.OriginalString?.Length ?? 0;
                    //    fileCount++;
                    //}
                } 

                return $"Enumerated {fileCount:n0} Files, URI total {uriLengthTotal / BytesPerMB:n0}MB.";
            });

            GC.KeepAlive(log);
        }

        private static void Measure(Func<string> action)
        {
            long ramBefore = GC.GetTotalMemory(true);
            Stopwatch w = Stopwatch.StartNew();

            string message = action();

            w.Stop();
            long ramAfter = GC.GetTotalMemory(true);

            Console.WriteLine($"{message} in {w.ElapsedMilliseconds:n0}ms and {(ramAfter - ramBefore) / (BytesPerMB):n1}MB RAM.");
        }
    }
}
