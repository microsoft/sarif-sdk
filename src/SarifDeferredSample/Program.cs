using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace SarifDeferredSample
{
    // NOTE: Not being submitted. This is a sample of usage of the Deferred Collections.

    internal class Program
    {
        private const float BytesPerMB = (float)(1024 * 1024);

        private static void Main(string[] args)
        {
            bool deferred = true;

            string filePath = args[0];
            SarifLog log = null;

            Console.WriteLine($"Loading {filePath}{(deferred ? " (deferred)" : "")}...");

            Measure(() =>
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.ContractResolver = (deferred ? SarifDeferredContractResolver.Instance : SarifContractResolver.Instance);

                using (JsonTextReader reader = (deferred ? new JsonPositionedTextReader(filePath) : new JsonTextReader(new StreamReader(filePath))))
                {
                    log = serializer.Deserialize<SarifLog>(reader);
                }

                return $"Loaded {filePath} ({new FileInfo(filePath).Length / BytesPerMB} MB)";
            });

            Run run = log.Runs[0];
            Measure(() =>
            {
                int messageLengthTotal = 0;

                foreach (Result result in run.Results)
                {
                    messageLengthTotal += result?.Message?.Text?.Length ?? 0;
                }

                return $"Enumerated {run.Results.Count:n0} Results message total {messageLengthTotal:n0}b";
            });

            Measure(() =>
            {
                int fileCount = 0;
                int uriLengthTotal = 0;

                if (run.Files != null)
                {
                    fileCount = run.Files.Count;

                    //foreach (var item in run.Files)
                    //{
                    //    FileData file = item.Value;
                    //    uriLengthTotal += file?.FileLocation?.Uri?.OriginalString?.Length ?? 0;
                    //}

                    foreach (var key in run.Files.Keys)
                    {
                        FileData file = run.Files[key];
                        uriLengthTotal += file?.FileLocation?.Uri?.OriginalString?.Length ?? 0;
                    }
                } 

                return $"Enumerated {fileCount:n0} Files, URI total {uriLengthTotal:n0}b.";
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

            Console.WriteLine($"{message} in {w.ElapsedMilliseconds:n0}ms using {(ramAfter - ramBefore) / (BytesPerMB):n3}MB RAM.");
        }

        private static string SeekAndRead(string filePath, long position)
        {
            char[] buffer = new char[500];
            using (StreamReader reader = new StreamReader(filePath))
            {
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                int length = reader.Read(buffer, 0, buffer.Length);
                return new string(buffer, 0, length);
            }
        }
    }
}
