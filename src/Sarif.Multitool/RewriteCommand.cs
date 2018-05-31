// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class RewriteCommand
    {
        public static int Run(RewriteOptions rewriteOptions)
        {
            try
            {
                rewriteOptions = ValidateOptions(rewriteOptions);
                
                SarifLog actualLog = MultitoolFileHelpers.ReadSarifFile<SarifLog>(rewriteOptions.InputFilePath);

                LoggingOptions loggingOptions = rewriteOptions.ConvertToLoggingOptions();

                SarifLog reformattedLog = new ReformattingVisitor(loggingOptions).VisitSarifLog(actualLog);
                
                string fileName = CommandUtilities.GetTransformedOutputFileName(rewriteOptions);

                var formatting = rewriteOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                MultitoolFileHelpers.WriteSarifFile(reformattedLog, fileName, formatting);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }

            return 0;
        }

        private static RewriteOptions ValidateOptions(RewriteOptions rewriteOptions)
        {
            if (rewriteOptions.Inline)
            {
                rewriteOptions.Force = true;
            }

            return rewriteOptions;
        }
    }
}
