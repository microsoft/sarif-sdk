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
        public static TimeSpan Time(string description, int iterations, Action method)
        {
            Console.WriteLine();
            Console.WriteLine(description);
            Console.Write("  ");

            Stopwatch w = Stopwatch.StartNew();
            TimeSpan elapsedAfterFirst = TimeSpan.Zero;

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                GC.Collect();

                w.Restart();
                method();
                w.Stop();

                if(iteration > 0) { Console.Write(" | "); }
                Console.Write(Friendly.Time(w.Elapsed));
                if (iteration > 0) { elapsedAfterFirst += w.Elapsed; }
            }

            Console.WriteLine();
            return (iterations == 1 ? w.Elapsed : TimeSpan.FromTicks(elapsedAfterFirst.Ticks / (iterations - 1)));
        }

        public static T LoadPerformance<T>(string path, int iterations, Func<string, T> loader)
        {
            T result = default(T);
            long fileSizeBytes = new FileInfo(path).Length;
            long ramBeforeBytes = GC.GetTotalMemory(true);

            string description = $"Loading {Path.GetFileName(path)} [{Friendly.Size(fileSizeBytes)}] with {AssemblyDescription<T>()}...";

            // Run and time the method
            TimeSpan averageRuntime = Time(description, iterations, () => result = loader(path));

            long ramAfterBytes = GC.GetTotalMemory(true);

            Friendly.HighlightLine($"  -> Loaded ", Friendly.Size(fileSizeBytes), " at ", $"{Friendly.Size((long)(fileSizeBytes / averageRuntime.TotalSeconds))}/s", " into ", $"{Friendly.Size(ramAfterBytes - ramBeforeBytes)} RAM");
            return result;
        }

        public static void SavePerformance(string path, int iterations, Action<string> saver)
        {
            string description = $"Saving as {Path.GetFileName(path)}...";

            // Run and time the method
            TimeSpan averageRuntime = Time(description, iterations, () => saver(path));
            long fileSizeBytes = new FileInfo(path).Length;

            Friendly.HighlightLine($"  -> Saved at ", $"{Friendly.Size((long)(fileSizeBytes / averageRuntime.TotalSeconds))}/s", " to ", Friendly.Size(fileSizeBytes), " file");
        }

        public static string AssemblyDescription<T>()
        {
            AssemblyName name = typeof(T).Assembly.GetName();
            string suffix = typeof(T).Assembly.GetType("Microsoft.CodeAnalysis.Sarif.SarifLogDatabase") != null ? " BSOA" : "";
            return $"{name.Name}{suffix} v{name.Version}";
        }
    }
}
