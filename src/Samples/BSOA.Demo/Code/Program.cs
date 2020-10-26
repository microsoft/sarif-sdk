// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace BSOA.Demo.Comparison
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string mode = (args.Length > 0 ? args[0].ToLowerInvariant() : "load");
            string inputPath = (args.Length > 1 ? args[1] : @"C:\Download\Demo\V2\Inputs\CodeAsData.sarif");
            string outputPath = (args.Length > 2 ? args[2] : Path.Combine(Path.GetDirectoryName(inputPath), "..\\Out", Path.GetFileName(inputPath)));

            switch (mode)
            {
                case "convert":
                    Modes.Convert(inputPath);
                    break;

                case "benchmarks":
                    Modes.Benchmarks(inputPath);
                    break;

                case "new":
                    Modes.New();
                    break;

                case "diagnostics":
                    Modes.Diagnostics(inputPath, (args.Length > 2 ? int.Parse(args[2]) : 3));
                    break;

                case "leaktest":
                    Modes.LeakTest(inputPath, (args.Length > 2 ? int.Parse(args[2]) : 100000));
                    break;

                default:
                    Console.WriteLine($"Unknown mode '{mode}'. Usage: BSOA.Demo <convert|benchmarks|diagnostics> <fileOrFolderPath>");
                    break;
            }

            Console.WriteLine();
        }
    }
}
