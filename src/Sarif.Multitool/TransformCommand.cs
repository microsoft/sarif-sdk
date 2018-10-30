// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class TransformCommand
    {
        public static int Run(TransformOptions transformOptions)
        {
            try
            {
                if (transformOptions.Version != SarifVersion.OneZeroZero && transformOptions.Version != SarifVersion.Current)
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
                if (transformOptions.Version == SarifVersion.Current)
                {
                    string inputFilePath = transformOptions.InputFilePath;
                    string inputVersion = SniffVersion(inputFilePath);

                    if (inputFilePath == null || inputFilePath == "1.0.0")
                    {
                        SarifLogVersionOne actualLog = FileHelpers.ReadSarifFile<SarifLogVersionOne>(transformOptions.InputFilePath, SarifContractResolverVersionOne.Instance);
                        var visitor = new SarifVersionOneToCurrentVisitor();
                        visitor.VisitSarifLogVersionOne(actualLog);
                        FileHelpers.WriteSarifFile(visitor.SarifLog, fileName, formatting);
                    }
                    else
                    {
                        // We have a pre-release v2 file that we should upgrade to current. 
                        string sarifText = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(File.ReadAllText(inputFilePath), formatting: formatting);
                        File.WriteAllText(fileName, sarifText);
                    }
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

        private static string SniffVersion(string sarifPath, int tokenCountLimit = 100)
        {
            int tokenCount = 0;

            using (JsonTextReader reader = new JsonTextReader(new StreamReader(sarifPath)))
            {
                while (reader.Read() && tokenCount < tokenCountLimit)
                {
                    if (reader.TokenType == JsonToken.PropertyName && ((string)reader.Value).Equals("version"))
                    {
                        reader.Read();
                        return (string)reader.Value;
                    }

                    tokenCount++;
                }
            }
            return null;
        }
    }
}
