// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BSOA.Demo
{
    public static class Measure
    {
        public static TimeSpan Time(int iterations, Action method, bool logTimes = true)
        {
            TextWriter log = (logTimes ? Console.Out : null);
            log?.Write("  ");

            Stopwatch w = Stopwatch.StartNew();
            TimeSpan elapsedAfterFirst = TimeSpan.Zero;

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                GC.Collect();

                w.Restart();
                method();
                w.Stop();

                if (iteration > 0) 
                {
                    elapsedAfterFirst += w.Elapsed;
                    log?.Write(" | "); 
                }

                log?.Write(Friendly.Time(w.Elapsed));
            }

            log?.WriteLine();
            return (iterations == 1 ? w.Elapsed : elapsedAfterFirst / (iterations - 1));
        }

        public static TimeSpan Time(TimeSpan measureFor, Action method, bool logTimes = true)
        {
            TextWriter log = (logTimes ? Console.Out : null);
            log?.Write("  ");

            Stopwatch w = Stopwatch.StartNew();
            TimeSpan elapsedAfterFirst = TimeSpan.Zero;

            int iteration = 0;
            while(w.Elapsed < measureFor || iteration == 0)
            {
                GC.Collect();

                w.Restart();
                method();
                w.Stop();

                if (iteration > 0)
                {
                    elapsedAfterFirst += w.Elapsed;
                    log?.Write(" | ");
                }

                log?.Write(Friendly.Time(w.Elapsed));
                iteration++;
            }

            log?.WriteLine();
            return (iteration == 1 ? w.Elapsed : elapsedAfterFirst / (iteration - 1));
        }

        public static T LoadPerformance<T>(string path, TimeSpan measureFor, Func<string, T> loader)
        {
            T result = default(T);
            long fileSizeBytes = new FileInfo(path).Length;
            long ramBeforeBytes = GC.GetTotalMemory(true);

            Friendly.HighlightLine($"Loading {Path.GetFileName(path)} [", Friendly.Size(fileSizeBytes), "] with ", AssemblyDescription<T>(), "...");
            TimeSpan averageRuntime = Time(measureFor, () => result = loader(path));

            long ramAfterBytes = GC.GetTotalMemory(true);

            Friendly.HighlightLine($"  -> Loaded ", Friendly.Size(fileSizeBytes), " at ", Friendly.Rate(fileSizeBytes, averageRuntime), " into ", $"{Friendly.Size(ramAfterBytes - ramBeforeBytes)} RAM");
            return result;
        }

        public static void SavePerformance(string path, TimeSpan measureFor, Action<string> saver)
        {
            Console.WriteLine($"Saving as {Path.GetFileName(path)}...");
            TimeSpan averageRuntime = Time(measureFor, () => saver(path));
            
            long fileSizeBytes = new FileInfo(path).Length;
            Friendly.HighlightLine($"  -> Saved at ", Friendly.Rate(fileSizeBytes, averageRuntime), " to ", Friendly.Size(fileSizeBytes), " file");
        }

        public static string AssemblyDescription<T>()
        {
            AssemblyName name = typeof(T).Assembly.GetName();
            string suffix = typeof(T).Assembly.GetType("Microsoft.CodeAnalysis.Sarif.SarifLogDatabase") != null ? " BSOA" : "";
            return $"{name.Name}{suffix} v{name.Version}";
        }
    }
}
