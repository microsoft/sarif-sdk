using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BSOA.Benchmarks
{
    /// <summary>
    ///  [Benchmark] attribute for methods, in case Benchmark.net isn't referenced.
    /// </summary>
    public class BenchmarkAttribute : Attribute
    { }

    public static class QuickBenchmarker
    {
        public static void RunFiles<OperationsClass, ArgumentClass>(string inputPath, Func<string, ArgumentClass> loader) where OperationsClass : new()
        {
            // Find all [Benchmark] methods which take an ArgumentClass.
            Dictionary<string, Action<ArgumentClass>> benchmarkMethods = BenchmarkObjectMethods<OperationsClass, ArgumentClass>();

            List<ConsoleColumn> columns = new List<ConsoleColumn>()
            {
                new ConsoleColumn("File"),
                new ConsoleColumn("Size", Align.Right),
                new ConsoleColumn("Load", Align.Right),
                new ConsoleColumn("RAM", Align.Right, Highlight.On)
            };

            foreach (string key in benchmarkMethods.Keys)
            {
                columns.Add(new ConsoleColumn(key, Align.Right, Highlight.On));
            }

            ConsoleTable table = new ConsoleTable(columns.ToArray());

            foreach (string filePath in FilesForPath(inputPath))
            {
                long fileLengthBytes = new FileInfo(filePath).Length;

                ArgumentClass instance = default(ArgumentClass);
                List<string> row = new List<string>();

                // Use the loader to load the file; log name, size, load rate.
                MeasureResult load = Measure.Operation(() => instance = loader(filePath), MeasureSettings.Load);
                row.Add(Path.GetFileName(filePath));
                row.Add(Friendly.Size(fileLengthBytes));
                row.Add(Friendly.Rate(fileLengthBytes, load.Elapsed / load.Iterations));
                row.Add(Friendly.Size(load.AddedMemoryBytes));

                // Log action time per operation.
                foreach(string key in benchmarkMethods.Keys)
                {
                    Action<ArgumentClass> operation = benchmarkMethods[key];
                    MeasureResult opResult = Measure.Operation(() => operation(instance));
                    row.Add(Friendly.Time(opResult.SecondsPerIteration));
                }

                table.AppendRow(row);
            }
        }

        /// <summary>
        ///  Similar to a fast, simple Benchmark.net Benchmarker.Run.
        ///  Benchmarks each method on the given class with the [Benchmark]
        ///  attribute.
        /// </summary>
        /// <typeparam name="T">Type containing methods to benchmark</typeparam>
        /// <param name="settings">Measurement settings, or null for defaults</param>
        public static void Run<T>(MeasureSettings settings = null) where T : new()
        {
            Dictionary<string, Action> benchmarkMethods = BenchmarkEmptyMethods<T>();

            ConsoleTable table = new ConsoleTable(new ConsoleColumn("Name"), new ConsoleColumn("Mean", Align.Right, Highlight.On));
            foreach (string methodName in benchmarkMethods.Keys)
            {
                MeasureResult result = Measure.Operation(benchmarkMethods[methodName], settings);
                table.AppendRow(methodName, Friendly.Time(result.SecondsPerIteration));
            }
        }

        internal static Dictionary<string, Action> BenchmarkEmptyMethods<T>() where T : new()
        {
            Dictionary<string, Action> methods = new Dictionary<string, Action>();

            // Create an instance of the desired class (triggering any initialization)
            T instance = new T();

            // Find all public methods with no arguments and a 'Benchmark' attribute
            Type tType = typeof(T);
            foreach (MethodInfo method in tType.GetMethods())
            {
                if (method.IsPublic && method.GetParameters().Length == 0 && method.GetCustomAttributes().Where((a) => a.GetType().Name == "BenchmarkAttribute").Any())
                {
                    Action operation = (Action)method.CreateDelegate(typeof(Action), instance);
                    methods[method.Name] = operation;
                }
            }

            return methods;
        }

        internal static Dictionary<string, Action<ArgumentClass>> BenchmarkObjectMethods<OperationsClass, ArgumentClass>() where OperationsClass : new()
        {
            Dictionary<string, Action<ArgumentClass>> methods = new Dictionary<string, Action<ArgumentClass>>();

            // Create an instance of the desired class (triggering any initialization)
            OperationsClass instance = new OperationsClass();

            // Find all public methods with no arguments and a 'Benchmark' attribute
            Type tType = typeof(OperationsClass);
            foreach (MethodInfo method in tType.GetMethods())
            {
                if (method.IsPublic && method.GetCustomAttributes().Where((a) => a.GetType().Name == "BenchmarkAttribute").Any())
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ArgumentClass))
                    {
                        Action<ArgumentClass> operation = (Action<ArgumentClass>)method.CreateDelegate(typeof(Action<ArgumentClass>), instance);
                        methods[method.Name] = operation;
                    }
                }
            }

            return methods;
        }

        public static IEnumerable<string> FilesForPath(string inputPath)
        {
            if (Directory.Exists(inputPath))
            {
                return Directory.EnumerateFiles(inputPath).ToList();
            }
            else
            {
                return new string[] { inputPath };
            }
        }
    }
}
