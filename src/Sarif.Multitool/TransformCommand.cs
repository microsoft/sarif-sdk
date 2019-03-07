// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class TransformCommand : CommandBase
    {
        private readonly bool _testing;
        private readonly IFileSystem _fileSystem;

        public TransformCommand(IFileSystem fileSystem = null, bool testing = false)
        {
            _fileSystem = fileSystem ?? new FileSystem();
            _testing = testing;
        }

        public int Run(TransformOptions transformOptions)
        {
            try
            {
                if (transformOptions.SarifOutputVersion != SarifVersion.OneZeroZero && transformOptions.SarifOutputVersion != SarifVersion.Current)
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


                string inputFilePath = transformOptions.InputFilePath;
                string inputVersion = SniffVersion(inputFilePath);

                // If the user wants to transform to current v2, we check to see whether the input
                // file is v2 or pre-release v2. We upgrade both formats to current v2. 
                // 
                // Correspondingly, if the input file is v2 of any kind, we first ensure that it is 
                // current v2, then drop it down to v1.
                // 
                // We do not support transforming to any obsoleted pre-release v2 formats. 
                if (transformOptions.SarifOutputVersion == SarifVersion.Current)
                {
                    if (inputVersion == "1.0.0")
                    {
                        SarifLogVersionOne actualLog = ReadSarifFile<SarifLogVersionOne>(_fileSystem, transformOptions.InputFilePath, SarifContractResolverVersionOne.Instance);
                        var visitor = new SarifVersionOneToCurrentVisitor();
                        visitor.VisitSarifLogVersionOne(actualLog);
                        WriteSarifFile(_fileSystem, visitor.SarifLog, fileName, formatting);
                    }
                    else
                    {
                        // We have a pre-release v2 file that we should upgrade to current. 
                        PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                            _fileSystem.ReadAllText(inputFilePath),
                            formatting: formatting,
                            out string sarifText);

                        _fileSystem.WriteAllText(fileName, sarifText);
                    }
                }
                else 
                {
                    if (inputVersion == "1.0.0")
                    {
                        _fileSystem.WriteAllText(fileName, _fileSystem.ReadAllText(inputFilePath));
                    }
                    else
                    {
                        string currentSarifVersion = SarifUtilities.SemanticVersion;

                        string sarifText = _fileSystem.ReadAllText(inputFilePath);
                        SarifLog actualLog = null;

                        if (inputVersion != currentSarifVersion)
                        {
                            // Note that we don't provide formatting here. It is not required to indent the v2 SARIF - it 
                            // will be transformed to v1 later, where we should apply the indentation settings.
                            actualLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                                sarifText,
                                formatting: Formatting.None,
                                out sarifText);
                        }
                        else
                        {
                            actualLog = JsonConvert.DeserializeObject<SarifLog>(sarifText);
                        }

                        var visitor = new SarifCurrentToVersionOneVisitor();
                        visitor.VisitSarifLog(actualLog);

                        WriteSarifFile(_fileSystem, visitor.SarifLogVersionOne, fileName, formatting, SarifContractResolverVersionOne.Instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        private string SniffVersion(string sarifPath)
        {
            // When we're unit-testing, we'll retrieve a string representation of the file contents and pass this
            // to a stream reader instance. We take this approach to prevent the need to load a potentially large file
            // all at once in the production code.
            TextReader textReader = _testing 
                ? new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(_fileSystem.ReadAllText(sarifPath)))) 
                : new StreamReader(sarifPath);

            using (JsonTextReader reader = new JsonTextReader(textReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName && ((string)reader.Value).Equals("version"))
                    {
                        reader.Read();
                        return (string)reader.Value;
                    }
                }
            }
            return null;
        }
    }
}