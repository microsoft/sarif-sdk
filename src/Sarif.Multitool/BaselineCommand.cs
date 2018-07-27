// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    class BaselineCommand
    {
        public static int Run(BaselineOptions baselineOptions)
        {
            try
            {
                SarifLog baselineFile = null;
                if (!string.IsNullOrEmpty(baselineOptions.BaselineFilePath))
                {
                    baselineFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(baselineOptions.BaselineFilePath);
                }

                SarifLog currentFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(baselineOptions.CurrentFilePath);
                
                IResultMatchingBaseliner matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();

                SarifLog output = matcher.BaselineSarifLogs(new SarifLog[] { baselineFile }, new SarifLog[] { currentFile });
                
                var formatting = baselineOptions.PrettyPrint
                        ? Newtonsoft.Json.Formatting.Indented
                        : Newtonsoft.Json.Formatting.None;
                
                MultitoolFileHelpers.WriteSarifFile(output, baselineOptions.OutputFilePath, formatting);
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
