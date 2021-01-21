// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RewriteCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public RewriteCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(RewriteOptions options)
        {
            try
            {
                // Only set --output-file if --inline isn't specified. ValidateOptions will check
                // to make sure that exactly one of those two options is set.
                if (!options.Inline)
                {
                    //  TODO:  Find a more consistent way of correcting options
                    options.OutputFilePath = CommandUtilities.GetTransformedOutputFileName(options);
                }

                Console.WriteLine($"Rewriting '{options.InputFilePath}' => '{options.OutputFilePath}'...");
                Stopwatch w = Stopwatch.StartNew();

                bool valid = ValidateOptions(options);
                if (!valid) { return FAILURE; }

                SarifLog actualLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

                string inputVersion = SniffVersion(options.InputFilePath);
                if (TransformationNecessary(options.SarifOutputVersion, inputVersion))
                {
                    TransformFile(options, options.InputFilePath, inputVersion);
                }

                OptionallyEmittedData dataToInsert = options.DataToInsert.ToFlags();
                OptionallyEmittedData dataToRemove = options.DataToRemove.ToFlags();
                IDictionary<string, ArtifactLocation> originalUriBaseIds = options.ConstructUriBaseIdsDictionary();

                SarifLog reformattedLog = new RemoveOptionalDataVisitor(dataToRemove).VisitSarifLog(actualLog);
                reformattedLog = new InsertOptionalDataVisitor(dataToInsert, originalUriBaseIds).VisitSarifLog(reformattedLog);

                string fileName = CommandUtilities.GetTransformedOutputFileName(options);

                WriteSarifFile(_fileSystem, reformattedLog, fileName, options.Minify);

                w.Stop();
                Console.WriteLine($"Rewrite completed in {w.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private bool ValidateOptions(RewriteOptions rewriteOptions)
        {
            bool valid = true;

            valid &= rewriteOptions.Validate();

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(rewriteOptions.OutputFilePath, rewriteOptions.Force, _fileSystem);

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

        private void TransformFile(SingleFileOptionsBase options, string inputFilePath, string inputVersion)
        {
            // If the user wants to transform to current v2, we check to see whether the input
            // file is v2 or pre-release v2. We upgrade both formats to current v2. 
            // 
            // Correspondingly, if the input file is v2 of any kind, we first ensure that it is 
            // current v2, then drop it down to v1.
            // 
            // We do not support transforming to any obsoleted pre-release v2 formats. 
            // SarifVersion can only be "OneZeroZero" or "Current" (2).  
            if (options.SarifOutputVersion == SarifVersion.Current)
            {
                if (inputVersion == "1.0.0")
                {
                    //  Converting version 1 to version 2
                    SarifLogVersionOne actualLog = ReadSarifFile<SarifLogVersionOne>(_fileSystem, options.InputFilePath, SarifContractResolverVersionOne.Instance);
                    var visitor = new SarifVersionOneToCurrentVisitor();
                    visitor.VisitSarifLogVersionOne(actualLog);
                    WriteSarifFile(_fileSystem, visitor.SarifLog, options.OutputFilePath, options.Minify);
                }
                else
                {
                    //  Converting prerelease version 2 to version 2
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
                    //  Converting version 1 to version 1?
                    return;
                    //  TODO: Give some output to the user, they asked for something that doesn't make sense.
                }
                else
                {
                    //  Converting version 2 or prerelease version 2 to version 1
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

        private bool TransformationNecessary(SarifVersion requestedOutputVersion, string incomingVersion)
        {
            switch (requestedOutputVersion)
            {
                case SarifVersion.OneZeroZero:
                    if (incomingVersion.Equals(SarifUtilities.V1_0_0))
                    {
                        return false;
                    }

                    return true;
                case SarifVersion.Current:
                    if (incomingVersion.Equals(SarifUtilities.StableSarifVersion))
                    {
                        return false;
                    }

                    return true;
                default:
                    throw new ArgumentException("Unable to determine whether file transformation is necessary.  Was a new sarif version added?");
            }
        }
    }
}
