// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
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
                
                SarifLog actualLog = MultitoolFileHelpers.ReadSarifFile(rewriteOptions.InputFilePath);

                LoggingOptions loggingOptions = rewriteOptions.ConvertToLoggingOptions();

                SarifLog reformattedLog = new ReformattingVisitor(loggingOptions).VisitSarifLog(actualLog);
                
                string fileName = GetOutputFileName(rewriteOptions);

                var formatting = rewriteOptions.PrettyPrint
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None;

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

        internal static string GetOutputFileName(RewriteOptions rewriteOptions)
        {
            string filePath = Path.GetFullPath(rewriteOptions.InputFilePath);

            if (rewriteOptions.Inline)
            {
                return filePath;
            }

            if (!String.IsNullOrEmpty(rewriteOptions.OutputFilePath))
            {
                return rewriteOptions.OutputFilePath;
            }

            const string TransformedExtension = "transformed.sarif";
            string extension = Path.GetExtension(filePath);

            // For an input file named MyFile.sarif, returns MyFile.transformed.sarif.
            if (extension.Equals(".sarif", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileNameWithoutExtension(filePath) + TransformedExtension;
            }

            // For an input file named MyFile.json, return MyFile.json.transformed.sarif.
            return Path.GetFullPath(rewriteOptions.InputFilePath + TransformedExtension);
        }
    }
}
