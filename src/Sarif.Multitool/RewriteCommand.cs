﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Visitors;
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

                OptionallyEmittedData dataToInsert = rewriteOptions.DataToInsert.ToFlags();

                SarifLog reformattedLog = new ReformattingVisitor(dataToInsert).VisitSarifLog(actualLog);
                
                string fileName = CommandUtilities.GetTransformedOutputFileName(rewriteOptions);

                var formatting = rewriteOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                MultitoolFileHelpers.WriteSarifFile(reformattedLog, fileName, formatting);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
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
