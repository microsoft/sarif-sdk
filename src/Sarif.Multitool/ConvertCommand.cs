// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class ConvertCommand
    {
        public static int Run(ConvertOptions convertOptions)
        {
            try
            {
                LoggingOptions loggingOptions = LoggingOptions.None;

                if (convertOptions.PrettyPrint)
                {
                    loggingOptions |= LoggingOptions.PrettyPrint;
                };

                if (convertOptions.Force)
                {
                    loggingOptions |= LoggingOptions.OverwriteExistingOutputFile;
                };

                if (convertOptions.PersistFileContents)
                {
                    loggingOptions |= LoggingOptions.PersistFileContents;
                }

                if (string.IsNullOrEmpty(convertOptions.OutputFilePath))
                {
                    convertOptions.OutputFilePath = convertOptions.InputFilePath + ".sarif";
                }

                new ToolFormatConverter().ConvertToStandardFormat(
                    convertOptions.ToolFormat,
                    convertOptions.InputFilePath,
                    convertOptions.OutputFilePath,
                    loggingOptions,
                    convertOptions.PluginAssemblyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }

            return 0;
        }
    }
}
