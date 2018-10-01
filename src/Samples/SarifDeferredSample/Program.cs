using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            WriteTest(bool.Parse(args[0]), args[1], args[2]);
        }

        private static void WriteTest(bool newWriter, string inputFilePath, string outputFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = SarifContractResolver.Instance;
            serializer.Formatting = Formatting.Indented;

            // Read (a few) sample results from the input file
            List<Result> results = null;
            Measure(() =>
            {
                using (JsonTextReader reader = new JsonTextReader(new StreamReader(inputFilePath)))
                {
                    results = serializer.Deserialize<SarifLog>(reader).Runs[0].Results.Take(1000).ToList();
                }

                return $"Read {results.Count:n0} results from {inputFilePath}.";
            });

            int targetResultCount = 500 * 1000;

            SarifLog log = new SarifLog() { Version = SarifVersion.TwoZeroZero };
            Run run = new Run() { Tool = new Tool() { Name = "FilesList", Version = "1.0" } };
            
            if(newWriter)
            {
                Measure(() =>
                {
                    // Write base log (must not have Runs)
                    using (SarifWriter writer = new SarifWriter(serializer, outputFilePath, log))
                    {
                        // Write Run (must not have Files or Results)
                        writer.Write(run);

                        // Write Results/Files (as they are generated; DON'T keep on a List or Array)
                        for (int i = 0; i < targetResultCount; ++i)
                        {
                            writer.Write(results[i % results.Count]);
                        }
                    }

                    return $"Wrote {targetResultCount:n0} results to {outputFilePath} (SarifWriter)";
                });
            }
            else
            {
                Measure(() =>
                {
                    log.Runs = new List<Run>();
                    log.Runs.Add(run);
                    run.Results = new List<Result>();

                    for (int i = 0; i < targetResultCount; ++i)
                    {
                        run.Results.Add(new Result(results[i % results.Count]));
                    }

                    using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(outputFilePath)))
                    {
                        serializer.Serialize(writer, log);
                    }

                    return $"Wrote {targetResultCount:n0} results to {outputFilePath} (Serialize)";
                });
            }

            GC.KeepAlive(log);
        }

        private static void ReadTest(bool newReader, string filePath)
        {
            SarifLog log = null;

            Console.WriteLine($"Loading {filePath}{(newReader ? " (deferred)" : "")}...");

            Measure(() =>
            {
                // Use 'SarifDeferredContractResolver' and 'JsonPositionedTextReader' to load a geferred version of the same object graph.
                JsonSerializer serializer = new JsonSerializer();
                serializer.ContractResolver = (newReader ? SarifDeferredContractResolver.Instance : SarifContractResolver.Instance);

                using (JsonTextReader reader = (newReader ? new JsonPositionedTextReader(filePath) : new JsonTextReader(new StreamReader(filePath))))
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

                if (run.Files != null)
                {
                    // Fastest: Enumerate
                    foreach (var item in run.Files)
                    {
                        FileData file = item.Value;
                        uriLengthTotal += file?.FileLocation?.Uri?.OriginalString?.Length ?? 0;
                        fileCount++;
                    }

                    // Slower: Keys and indexer
                    //foreach (var key in run.Files.Keys)
                    //{
                    //    FileData file = run.Files[key];
                    //    uriLengthTotal += file?.FileLocation?.Uri?.OriginalString?.Length ?? 0;
                    //    fileCount++;
                    //}
                }

                return $"Enumerated {fileCount:n0} Files, URI total {uriLengthTotal / BytesPerMB:n0}MB.";
            });

            GC.KeepAlive(log);
        }

        private static int TruncateToInt(long value)
        {
            if (value > int.MaxValue) return int.MaxValue;
            if (value < int.MinValue) return int.MinValue;
            return (int)value;
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
