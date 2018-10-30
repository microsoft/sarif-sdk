// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class TransformCommand
    {
        private bool _testing;
        private IFileSystem _fileSystem;

        public TransformCommand(IFileSystem fileSystem = null, bool testing = false)
        {
            _fileSystem = fileSystem ?? new FileSystem();
            _testing = testing;
        }

        public int Run(TransformOptions transformOptions)
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
                        SarifLogVersionOne actualLog = FileHelpers.ReadSarifFile<SarifLogVersionOne>(_fileSystem, transformOptions.InputFilePath, SarifContractResolverVersionOne.Instance);
                        var visitor = new SarifVersionOneToCurrentVisitor();
                        visitor.VisitSarifLogVersionOne(actualLog);
                        FileHelpers.WriteSarifFile(_fileSystem, visitor.SarifLog, fileName, formatting);
                    }
                    else
                    {
                        // We have a pre-release v2 file that we should upgrade to current. 
                        string sarifText = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(_fileSystem.ReadAllText(inputFilePath), formatting: formatting);
                        _fileSystem.WriteAllText(fileName, sarifText);
                    }
                }
                else
                {
                    SarifLog actualLog = FileHelpers.ReadSarifFile<SarifLog>(_fileSystem, transformOptions.InputFilePath);
                    var visitor = new SarifCurrentToVersionOneVisitor();
                    visitor.VisitSarifLog(actualLog);

                    FileHelpers.WriteSarifFile(_fileSystem, visitor.SarifLogVersionOne, fileName, formatting);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        private string SniffVersion(string sarifPath, int tokenCountLimit = int.MaxValue)
        {
            int tokenCount = 0;

            // When we're unit-testing, we'll retrieve a string representation of the file contents and pass this to a stream reader instance.
            TextReader textReader = _testing ? new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(_fileSystem.ReadAllText(sarifPath)))) : new StreamReader(sarifPath);

            using (JsonTextReader reader = new JsonTextReader(textReader))
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