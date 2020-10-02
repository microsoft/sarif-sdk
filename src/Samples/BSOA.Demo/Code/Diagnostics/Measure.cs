// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;

namespace BSOA.Demo
{
    /// <summary>
    ///  Measure.Operation provides and easy way to measure runtime, throughput, and memory use.
    ///   MeasureSettings can configure the desired iterations, a runtime target, and whether to collect memory.
    /// </summary>
    public static class Measure
    {
        public static MeasureResult Operation(string inputFilePath, Action<string> operation, MeasureSettings settings = null)
        {
            settings = settings ?? MeasureSettings.Default;

            long ramBefore = (settings.MeasureMemory ? GC.GetTotalMemory(true) : 0);
            TimeSpan elapsedAfterFirst = TimeSpan.Zero;

            Stopwatch total = Stopwatch.StartNew();
            Stopwatch single = Stopwatch.StartNew();

            int iteration = 0;
            while (iteration < settings.MinIterations || (iteration < settings.MaxIterations && total.Elapsed < settings.WithinTime))
            {
                single.Restart();
                operation(inputFilePath);
                single.Stop();

                if (iteration > 0)
                {
                    elapsedAfterFirst += single.Elapsed;
                }

                iteration++;
            }

            MeasureResult result = new MeasureResult()
            {
                InputFilePath = inputFilePath,
                InputSizeBytes = new FileInfo(inputFilePath).Length,
                Iterations = iteration,
                AverageTime = (iteration == 1 ? single.Elapsed : elapsedAfterFirst / (iteration - 1)),
                AddedMemoryBytes = (settings.MeasureMemory ? GC.GetTotalMemory(true) - ramBefore : 0),
            };

            return result;
        }

        public static MeasureResult<T> Operation<T>(string inputFilePath, Func<string, T> operation, MeasureSettings settings = null)
        {
            T output = default(T);

            MeasureResult inner = Operation(inputFilePath, (Action<string>)((path) => output = operation(path)), settings);

            return new MeasureResult<T>()
            {
                InputFilePath = inner.InputFilePath,
                InputSizeBytes = inner.InputSizeBytes,
                Iterations = inner.Iterations,
                AverageTime = inner.AverageTime,
                AddedMemoryBytes = inner.AddedMemoryBytes,
                Output = output
            };
        }
    }

    public class MeasureSettings
    {
        public static MeasureSettings Default = new MeasureSettings(TimeSpan.FromSeconds(2), 1, 8, false);

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
        public string InputFilePath { get; set; }
        public long InputSizeBytes { get; set; }
        public TimeSpan AverageTime { get; set; }
        public int Iterations { get; set; }
        public long AddedMemoryBytes { get; set; }
    }

    public class MeasureResult<T> : MeasureResult
    {
        public T Output { get; set; }
    }
}
