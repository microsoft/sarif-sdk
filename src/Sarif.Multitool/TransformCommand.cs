// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class TransformCommand
    {
        public static int Run(TransformOptions transformOptions)
        {
            try
            {
                if (transformOptions.Version < 1 || transformOptions.Version > 2)
                {
                    Console.WriteLine(MultitoolResources.ErrorInvalidTransformTargetVersion);
                    return 1;
                }

                OptionallyEmittedData dataToInsert = transformOptions.DataToInsert.ToFlags();

                // NOTE: we don't actually utilize the dataToInsert command-line data yet...

                string fileName = CommandUtilities.GetTransformedOutputFileName(transformOptions);

                var formatting = transformOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                // Assume the input log is the "other" version
                if (transformOptions.Version == 2)
                {
                    SarifLogVersionOne actualLog = FileHelpers.ReadSarifFile<SarifLogVersionOne>(transformOptions.InputFilePath, SarifContractResolverVersionOne.Instance);
                    var visitor = new SarifVersionOneToCurrentVisitor();
                    visitor.VisitSarifLogVersionOne(actualLog);

                    FileHelpers.WriteSarifFile(visitor.SarifLog, fileName, formatting);
                }
                else
                {
                    SarifLog actualLog = FileHelpers.ReadSarifFile<SarifLog>(transformOptions.InputFilePath);
                    var visitor = new SarifCurrentToVersionOneVisitor();
                    visitor.VisitSarifLog(actualLog);

                    FileHelpers.WriteSarifFile(visitor.SarifLogVersionOne, fileName, formatting);
                }
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
