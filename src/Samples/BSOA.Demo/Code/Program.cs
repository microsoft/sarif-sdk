// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Demo.Comparison
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string mode = (args.Length > 0 ? args[0].ToLowerInvariant() : "load");
            string filePath = (args.Length > 1 ? args[1] : @"C:\Download\Demo\V2\Inputs\CodeAsData.sarif");
            string outputPath = (args.Length > 2 ? args[2] : Path.Combine(Path.GetDirectoryName(filePath), "..\\Out", Path.GetFileName(filePath)));
            int iterations = (args.Length > 3 ? int.Parse(args[3]) : 4);

            switch (mode)
            {
                case "load":
                    Modes.Load(filePath, iterations);
                    break;

                case "loadandsave":
                    Modes.LoadAndSave(filePath, outputPath, iterations);
                    break;

                case "objectmodeloverhead":
                    Modes.ObjectModelOverhead(filePath);
                    break;

                case "diagnostics":
                    Modes.Diagnostics(filePath, (args.Length > 2 ? int.Parse(args[2]) : 3));
                    break;

                case "convertfolder":
                    Modes.ConvertFolder(filePath);
                    break;

                case "indentfolder":
                    Modes.IndentFolder(filePath);
                    break;

                case "roundtripfolder":
                    Modes.LoadAndSaveFolder(filePath);
                    break;

                case "leaktest":
                    Modes.LeakTest(filePath, (args.Length > 2 ? int.Parse(args[2]) : 100000));
                    break;

                default:
                    Console.WriteLine($"Unknown mode '{mode}'. Usage: BSOA.Demo <load/build> <filePath>");
                    break;
            }
        }
    }
}
