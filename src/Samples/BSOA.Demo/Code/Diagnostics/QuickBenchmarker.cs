using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace BSOA.Benchmarks.Attributes
{
    /// <summary>
    ///  [Benchmark] attribute for methods, in case Benchmark.net isn't referenced.
    /// </summary>
    public class BenchmarkAttribute : Attribute
    { }
}

namespace BSOA.Benchmarks
{
    public static class QuickBenchmarker
    {
        /// <summary>
        ///  For each file in inputPath, load into an ArgumentClass with loader(),
        ///  then run each [Benchmark] method on operationsClass and report the results.
        /// </summary>
        /// <typeparam name="ArgumentClass">Object Model type each file turns into.</typeparam>
        /// <param name="operationsClass">Class containing [Benchmark] methods which take the ArgumentClass type</param>
        /// <param name="inputPath">Folder Path or single File Path which can be loaded into ArgumentClass</param>
        /// <param name="loader">Method which takes a file path and loads into an ArgumentClass intance</param>
        public static void RunFiles<ArgumentClass>(Type operationsClass, string inputPath, Func<string, ArgumentClass> loader)
        {
            // Find all [Benchmark] methods which take an ArgumentClass.
            Dictionary<string, Action<ArgumentClass>> benchmarkMethods = GetBenchmarkMethods<Action<ArgumentClass>>(operationsClass);

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
                foreach (string key in benchmarkMethods.Keys)
                {
                    Action<ArgumentClass> operation = benchmarkMethods[key];
                    MeasureResult opResult = Measure.Operation(() => operation(instance));
                    row.Add(Friendly.Time(opResult.SecondsPerIteration));
                }

                table.AppendRow(row);
            }
        }

        public static void RunFiles<OperationsClass, ArgumentClass>(string inputPath, Func<string, ArgumentClass> loader)
        {
            RunFiles<ArgumentClass>(typeof(OperationsClass), inputPath, loader);
        }

        /// <summary>
        ///  Similar to a fast, simple Benchmark.net Benchmarker.Run.
        ///  Benchmarks each method on the given class with the [Benchmark]
        ///  attribute.
        /// </summary>
        /// <param name="typeWithBenchmarkMethods">Type containing methods to benchmark</typeparam>
        /// <param name="settings">Measurement settings, or null for defaults</param>
        public static void Run(Type typeWithBenchmarkMethods, MeasureSettings settings = null)
        {
            Dictionary<string, Action> benchmarkMethods = GetBenchmarkMethods<Action>(typeWithBenchmarkMethods);

            ConsoleTable table = new ConsoleTable(new ConsoleColumn("Name"), new ConsoleColumn("Mean", Align.Right, Highlight.On));
            foreach (string methodName in benchmarkMethods.Keys)
            {
                MeasureResult result = Measure.Operation(benchmarkMethods[methodName], settings);
                table.AppendRow(methodName, Friendly.Time(result.SecondsPerIteration));
            }
        }

        /// <summary>
        ///  Similar to a fast, simple Benchmark.net Benchmarker.Run.
        ///  Benchmarks each method on the given class with the [Benchmark]
        ///  attribute.
        /// </summary>
        /// <typeparam name="T">Type containing methods to benchmark</typeparam>
        /// <param name="settings">Measurement settings, or null for defaults</param>
        public static void Run<T>(MeasureSettings settings = null)
        {
            Run(typeof(T), settings);
        }

        /// <summary>
        ///  Reflection magic to get public methods with the [Benchmark] attribute
        ///  matching the desired signature.
        /// </summary>
        /// <remarks>
        ///  See https://github.com/microsoft/elfie-arriba/blob/master/XForm/XForm/Core/NativeAccelerator.cs for the
        ///  closest related craziness.
        /// </remarks>
        /// <typeparam name="WithSignature">Action or Func with desired parameters and return type</typeparam>
        /// <param name="fromType">Type to search for matching methods</param>
        /// <returns>Dictionary with method names and Action or Func to invoke for each matching method</returns>
        internal static Dictionary<string, WithSignature> GetBenchmarkMethods<WithSignature>(Type fromType)
        {
            Dictionary<string, WithSignature> methods = new Dictionary<string, WithSignature>();

            // Identify the return type and argument types on the desired method signature
            Type delegateOrFuncType = typeof(WithSignature);
            MethodInfo withSignatureInfo = delegateOrFuncType.GetMethod("Invoke");
            Type returnType = withSignatureInfo.ReturnType;
            List<Type> arguments = new List<Type>(withSignatureInfo.GetParameters().Select((pi) => pi.ParameterType));

            // Create an instance of the desired class (triggering any initialization)
            object instance = null;

            // Find all public methods with 'Benchmark' attribute and correct signature
            foreach (MethodInfo method in fromType.GetMethods())
            {
                if (!method.IsPublic) { continue; }
                if (!method.GetCustomAttributes().Where((a) => a.GetType().Name == "BenchmarkAttribute").Any()) { continue; }
                if (!method.ReturnType.Equals(returnType)) { continue; }
                if (!arguments.SequenceEqual(method.GetParameters().Select((pi) => pi.ParameterType))) { continue; }

                if (!method.IsStatic)
                {
                    instance ??= fromType.GetConstructor(new Type[0]).Invoke(null);
                }

                methods[method.Name] = (WithSignature)(object)method.CreateDelegate(delegateOrFuncType, instance);
            }

            return methods;
        }

        /// <summary>
        ///  Return the list of files for a given path.
        ///  If it's a folder, list the files directly in the folder.
        ///  If it's a file, return just that file.
        /// </summary>
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
