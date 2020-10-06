// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace BSOA.Benchmarks
{
    /// <summary>
    ///  Measure.Operation provides and easy way to measure runtime, throughput, and memory use.
    ///   MeasureSettings can configure the desired iterations, a runtime target, and whether to collect memory.
    /// </summary>
    public static class Measure
    {
        public static MeasureResult Operation(Action operation, MeasureSettings settings = null)
        {
            settings = settings ?? MeasureSettings.Default;

            long ramBefore = (settings.MeasureMemory ? GC.GetTotalMemory(true) : 0);
            TimeSpan elapsedAfterFirst = TimeSpan.Zero;

            Stopwatch total = Stopwatch.StartNew();
            Stopwatch single = Stopwatch.StartNew();

            int pass = 0;
            int iterations = 0;
            int innerIterations = 1;

            while (pass < settings.MinIterations || (pass < settings.MaxIterations && total.Elapsed < settings.WithinTime))
            {
                // Measure the operation in a tight loop
                single.Restart();
                for (int i = 0; i < innerIterations; ++i)
                {
                    operation();
                }
                single.Stop();

                // Double inner loop iterations and don't count pass until we spend enough time to measure reliably in a pass
                if (single.ElapsedMilliseconds < 100 && innerIterations < 32 * 1024 * 1024)
                {
                    innerIterations *= 2;
                    pass--;
                }

                // Track time from all iterations but the first (which is often much slower)
                if (iterations > 0)
                {
                    elapsedAfterFirst += single.Elapsed;
                }

                iterations += innerIterations;
                pass++;
            }

            MeasureResult result = new MeasureResult()
            {
                Iterations = (iterations == 1 ? 1 : iterations - 1),
                Elapsed = (iterations == 1 ? single.Elapsed : elapsedAfterFirst),
                AddedMemoryBytes = (settings.MeasureMemory ? GC.GetTotalMemory(true) - ramBefore : 0),
            };

            return result;
        }

        public static MeasureResult<T> Operation<T>(Func<T> operation, MeasureSettings settings = null)
        {
            T output = default(T);

            MeasureResult inner = Operation(() => { output = operation(); }, settings);

            return new MeasureResult<T>()
            {
                Iterations = inner.Iterations,
                Elapsed = inner.Elapsed,
                AddedMemoryBytes = inner.AddedMemoryBytes,
                Output = output
            };
        }
    }

    /// <summary>
    ///  MeasureSettings configures how performance measurements are run: for how long to measure, bounds on the number
    ///  of iterations to run, and whether to measure memory use.
    ///  
    ///  These settings allow the same measurement code to be used for small and large operations.
    /// </summary>
    public class MeasureSettings
    {
        // Measure at least once, then up to 16 passes or 2 seconds, whichever comes first
        public static MeasureSettings Default = new MeasureSettings(TimeSpan.FromSeconds(2), 1, 10, false);

        // Load once, measuring RAM
        public static MeasureSettings LoadOnce = new MeasureSettings(TimeSpan.Zero, 1, 1, true);

        // Load a few times, measuring RAM
        public static MeasureSettings Load = new MeasureSettings(TimeSpan.FromSeconds(2), 2, 5, true);

        public TimeSpan WithinTime { get; set; }
        public int MinIterations { get; set; }
        public int MaxIterations { get; set; }
        public bool MeasureMemory { get; set; }

        public MeasureSettings(TimeSpan withinTime, int minIterations, int maxIterations, bool measureMemory)
        {
            this.WithinTime = withinTime;
            this.MinIterations = minIterations;
            this.MaxIterations = maxIterations;
            this.MeasureMemory = measureMemory;
        }
    }

    public class MeasureResult
    {
        public int Iterations { get; set; }
        public TimeSpan Elapsed { get; set; }
        public double SecondsPerIteration => Elapsed.TotalSeconds / Iterations;

        public long AddedMemoryBytes { get; set; }
    }

    public class MeasureResult<T> : MeasureResult
    {
        public T Output { get; set; }
    }
}
