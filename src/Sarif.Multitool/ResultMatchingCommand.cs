// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class ResultMatchingCommand
    {
        public static int Run(ResultMatchingOptions matchingOptions)
        {
            try
            {
                SarifLog baselineFile = null;
                if (!string.IsNullOrEmpty(matchingOptions.PreviousFilePath))
                {
                    baselineFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(matchingOptions.PreviousFilePath);
                }

                string outputFilePath = matchingOptions.OutputFilePath;
                if (string.IsNullOrEmpty(outputFilePath))
                {
                    outputFilePath = Path.GetFileNameWithoutExtension(matchingOptions.PreviousFilePath) + "-annotated.sarif";
                }

                SarifLog currentFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(matchingOptions.CurrentFilePath);
                
                ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();

                SarifLog output = matcher.Match(new SarifLog[] { baselineFile }, new SarifLog[] { currentFile }).First();
                
                var formatting = matchingOptions.PrettyPrint
                        ? Newtonsoft.Json.Formatting.Indented
                        : Newtonsoft.Json.Formatting.None;
                
                MultitoolFileHelpers.WriteSarifFile(output, outputFilePath, formatting);
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
