// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CommandLine;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.ConvertToSarif
{
    internal static class Program
    {
        /// <summary>The entry point for the SARIF file format conversion utility.</summary>
        /// <param name="args">Arguments passed in from the tool's command line.</param>
        /// <returns>0 on success; nonzero on failure.</returns>
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<
                ConvertOptions>(args)
              .MapResult(
                (ConvertOptions convertOptions) => RunConvertFile(convertOptions),
                errs => 1);
        }
        public static int RunConvertFile(ConvertOptions convertOptions)
        {
            try
            {
                ToolFormatConversionOptions toolFormatConversionOptions = 0;

                if (convertOptions.PrettyPrint)
                {
                    toolFormatConversionOptions |= ToolFormatConversionOptions.PrettyPrint;
                };

                if (convertOptions.Force)
                {
                    toolFormatConversionOptions |= ToolFormatConversionOptions.OverwriteExistingOutputFile;
                };

                new ToolFormatConverter().ConvertToStandardFormat(
                    convertOptions.ToolFormat,
                    convertOptions.InputFilePath,
                    convertOptions.OutputFilePath,
                    toolFormatConversionOptions);
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
