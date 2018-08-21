// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    class ResultMatchingCommand
    {
        public static int Run(ResultMatchingOptions matchingOptions)
        {
            try
            {
                SarifLog baselineFile = null;
                if (!string.IsNullOrEmpty(matchingOptions.BaselineFilePath))
                {
                    baselineFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(matchingOptions.BaselineFilePath);
                }

                SarifLog currentFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(matchingOptions.CurrentFilePath);
                
                IResultMatchingBaseliner matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();

                SarifLog output = matcher.BaselineSarifLogs(new SarifLog[] { baselineFile }, new SarifLog[] { currentFile });
                
                var formatting = matchingOptions.PrettyPrint
                        ? Newtonsoft.Json.Formatting.Indented
                        : Newtonsoft.Json.Formatting.None;
                
                MultitoolFileHelpers.WriteSarifFile(output, matchingOptions.OutputFilePath, formatting);
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
