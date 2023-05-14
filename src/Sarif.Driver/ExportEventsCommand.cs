// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Tracing;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ExportEventsCommand
    {
        public int Run(ExportEventsOptions options)
        {
            string path = options.OutputFilePath;

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Manifest output path was not provided.");
                return 1;
            }

            if (!options.ForceOverwrite && File.Exists(path))
            {
                Console.WriteLine("Output file exists. Delete the file or pass `--log ForceOverwrite` on the command-line.");
                return 1;
            }

            Type type = typeof(DriverEventSource);
            string assemblyPath = type.Assembly.Location;

            File.WriteAllText(
                path,
                EventSource.GenerateManifest(typeof(DriverEventSource), assemblyPath));

            Console.WriteLine($"Events manifest written to: {path}");
            return 0;
        }
    }
}
