// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    class BaselineCommand
    {
        public static int Run(BaselineOptions baselineOptions)
        {
            try
            {
                SarifLog baselineFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(baselineOptions.BaselineFilePath);
                SarifLog currentFile = MultitoolFileHelpers.ReadSarifFile<SarifLog>(baselineOptions.CurrentFilePath);
                if (baselineFile.Runs.Count != 1 || currentFile.Runs.Count != 1)
                {
                    throw new ArgumentException("Invalid sarif logs, we can only baseline logs with a single run in them.");
                }

                ISarifLogBaseliner baseliner = SarifLogBaselinerFactory.CreateSarifLogBaseliner(baselineOptions.BaselineType);

                Run diffedRun = baseliner.CreateBaselinedRun(baselineFile.Runs.First(), currentFile.Runs.First());
                
                SarifLog output = currentFile.DeepClone();
                output.Runs = new List<Run>();
                output.Runs.Add(diffedRun);

                var formatting = baselineOptions.PrettyPrint
                        ? Formatting.Indented
                        : Formatting.None;
                
                MultitoolFileHelpers.WriteSarifFile(output, baselineOptions.OutputFilePath, formatting);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }
    }
}
