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
        private const ConsoleColor HighlightColor = ConsoleColor.Green;
        private const double Megabyte = 1024 * 1024;

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

                Console.Write($"{(iteration > 0 ? " | " : "")}{w.Elapsed.TotalSeconds:n2}s");
                if (iteration > 0) { elapsedAfterFirst += w.Elapsed; }
            }

            Console.WriteLine();
            return (iterations == 1 ? w.Elapsed : TimeSpan.FromTicks(elapsedAfterFirst.Ticks / (iterations - 1)));
        }

        public static T LoadPerformance<T>(string path, int iterations, Func<string, T> loader)
        {
            T result = default(T);
            double fileSizeMB = new FileInfo(path).Length / Megabyte;
            double ramBeforeMB = GC.GetTotalMemory(true) / Megabyte;

            string description = $"Loading {Path.GetFileName(path)} [{fileSizeMB:n1} MB] with {AssemblyDescription<T>()}...";

            // Run and time the method
            TimeSpan averageRuntime = Time(description, iterations, () => result = loader(path));

            double ramAfterMB = GC.GetTotalMemory(true) / Megabyte;
            double loadMegabytesPerSecond = fileSizeMB / averageRuntime.TotalSeconds;

            ConsoleColor previous = Console.ForegroundColor;
            Console.Write($"  -> Loaded ");
            Console.ForegroundColor = HighlightColor;
            Console.Write($"{fileSizeMB:n1} MB");
            Console.ForegroundColor = previous;
            Console.Write($" at ");
            Console.ForegroundColor = HighlightColor;
            Console.Write($"{loadMegabytesPerSecond:n1} MB/s");
            Console.ForegroundColor = previous;
            Console.Write($" into ");
            Console.ForegroundColor = HighlightColor;
            Console.WriteLine($"{(ramAfterMB - ramBeforeMB):n1} MB RAM");
            Console.ForegroundColor = previous;

            return result;
        }

        public static void SavePerformance(string path, int iterations, Action<string> saver)
        {
            string description = $"Saving as {Path.GetFileName(path)}...";

            // Run and time the method
            TimeSpan averageRuntime = Time(description, iterations, () => saver(path));

            double fileSizeMB = new FileInfo(path).Length / Megabyte;
            double saveMegabytesPerSecond = fileSizeMB / averageRuntime.TotalSeconds;

            ConsoleColor previous = Console.ForegroundColor;
            Console.Write($"  -> Saved at ");
            Console.ForegroundColor = HighlightColor;
            Console.Write($"{saveMegabytesPerSecond:n1} MB/s");
            Console.ForegroundColor = previous;
            Console.Write($" to ");
            Console.ForegroundColor = HighlightColor;
            Console.Write($"{fileSizeMB:n1} MB");
            Console.ForegroundColor = previous;
            Console.WriteLine($" file");
        }

        public static string AssemblyDescription<T>()
        {
            AssemblyName name = typeof(T).Assembly.GetName();
            string suffix = typeof(T).Assembly.GetType("Microsoft.CodeAnalysis.Sarif.SarifLogDatabase") != null ? " BSOA" : "";
            return $"{name.Name}{suffix} v{name.Version}";
        }
    }
}
