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
    public class TransformCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public TransformCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(TransformOptions options)
        {
            try
            {
                // Only set --output-file if --inline isn't specified. ValidateOptions will check
                // to make sure that exactly one of those two options is set.
                if (!options.Inline)
                {
                    options.OutputFilePath = CommandUtilities.GetTransformedOutputFileName(options);
                }

                bool valid = ValidateOptions(options);
                if (!valid) { return FAILURE; }

                // NOTE: we don't actually utilize the dataToInsert command-line data yet...
                OptionallyEmittedData dataToInsert = options.DataToInsert.ToFlags();

                string inputFilePath = options.InputFilePath;
                string inputVersion = SniffVersion(inputFilePath);
                TransformFile(options, inputFilePath, inputVersion);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private void TransformFile(TransformOptions options, string inputFilePath, string inputVersion)
        {
            // If the user wants to transform to current v2, we check to see whether the input
            // file is v2 or pre-release v2. We upgrade both formats to current v2. 
            // 
            // Correspondingly, if the input file is v2 of any kind, we first ensure that it is 
            // current v2, then drop it down to v1.
            // 
            // We do not support transforming to any obsoleted pre-release v2 formats. 
            if (options.SarifOutputVersion == SarifVersion.Current)
            {
                if (inputVersion == "1.0.0")
                {
                    SarifLogVersionOne actualLog = ReadSarifFile<SarifLogVersionOne>(_fileSystem, options.InputFilePath, SarifContractResolverVersionOne.Instance);
                    var visitor = new SarifVersionOneToCurrentVisitor();
                    visitor.VisitSarifLogVersionOne(actualLog);
                    WriteSarifFile(_fileSystem, visitor.SarifLog, options.OutputFilePath, options.Minify);
                }
                else
                {
                    // We have a pre-release v2 file that we should upgrade to current. 
                    PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                        _fileSystem.FileReadAllText(inputFilePath),
                        formatting: options.Formatting,
                        out string sarifText);

                    _fileSystem.FileWriteAllText(options.OutputFilePath, sarifText);
                }
            }
            else
            {
                if (inputVersion == "1.0.0")
                {
                    SarifLogVersionOne logV1 = ReadSarifFile<SarifLogVersionOne>(_fileSystem, options.InputFilePath, SarifContractResolverVersionOne.Instance);
                    logV1.SchemaUri = SarifVersion.OneZeroZero.ConvertToSchemaUri();
                    WriteSarifFile(_fileSystem, logV1, options.OutputFilePath, options.Minify, SarifContractResolverVersionOne.Instance);
                    //  return;  Give some output to the user, they asked for something that doesn't make sense.
                }
                else
                {
                    //  Version 2 to version 1
                    string currentSarifVersion = SarifUtilities.StableSarifVersion;

                    string sarifText = _fileSystem.FileReadAllText(inputFilePath);
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

                    WriteSarifFile(_fileSystem, visitor.SarifLogVersionOne, options.OutputFilePath, options.Minify, SarifContractResolverVersionOne.Instance);
                }
            }
        }

        private bool ValidateOptions(TransformOptions transformOptions)
        {
            bool valid = true;

            valid &= transformOptions.Validate();

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(transformOptions.OutputFilePath, transformOptions.Force, _fileSystem);

            return valid;
        }

        private string SniffVersion(string sarifPath)
        {
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(_fileSystem.FileOpenRead(sarifPath))))
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
