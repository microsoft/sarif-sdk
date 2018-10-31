// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class ResultMatchingCommand
    {
        private readonly IFileSystem _fileSystem;

        public ResultMatchingCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(ResultMatchingOptions matchingOptions)
        {
            try
            {
                SarifLog baselineFile = null;
                if (!string.IsNullOrEmpty(matchingOptions.PreviousFilePath))
                {
                    baselineFile = FileHelpers.ReadSarifFile<SarifLog>(_fileSystem, matchingOptions.PreviousFilePath);
                }

                string outputFilePath = matchingOptions.OutputFilePath;
                if (string.IsNullOrEmpty(outputFilePath))
                {
                    outputFilePath = Path.GetFileNameWithoutExtension(matchingOptions.PreviousFilePath) + "-annotated.sarif";
                }

                SarifLog currentFile = FileHelpers.ReadSarifFile<SarifLog>(_fileSystem, matchingOptions.CurrentFilePath);
                
                ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();

                SarifLog output = matcher.Match(new SarifLog[] { baselineFile }, new SarifLog[] { currentFile }).First();
                
                var formatting = matchingOptions.PrettyPrint
                        ? Newtonsoft.Json.Formatting.Indented
                        : Newtonsoft.Json.Formatting.None;
                
                FileHelpers.WriteSarifFile(_fileSystem, output, outputFilePath, formatting);
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
