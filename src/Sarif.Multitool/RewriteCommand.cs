// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
                IDictionary<string, Uri> originalUriBaseIds = ConstructUriBaseIds(rewriteOptions.UriBaseIds);

                SarifLog reformattedLog = new InsertOptionalDataVisitor(dataToInsert, originalUriBaseIds).VisitSarifLog(actualLog);
                
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

        private static IDictionary<string, Uri> ConstructUriBaseIds(IEnumerable<string> uriBaseIds)
        {
            if (uriBaseIds == null) { return null; }

            IDictionary<string, Uri> result = new Dictionary<string, Uri>();

            foreach (string uriBaseId in uriBaseIds)
            {
                string[] tokens = uriBaseId.Split('=');
                string key = tokens[0];
                Uri value = new Uri(tokens[1], UriKind.Absolute);
                result[key] = value;
            }

            return result;
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
