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
                Console.WriteLine($"Rewriting '{options.InputFilePath}' => '{options.OutputFilePath}'...");
                Stopwatch w = Stopwatch.StartNew();

                bool valid = ValidateOptions(options);
                if (!valid)
                {
                    return FAILURE;
                }

                string actualOutputPath = CommandUtilities.GetTransformedOutputFileName(options);

                SarifLog actualLog = null;

                string inputVersion = SniffVersion(options.InputFilePath);
                if (!inputVersion.Equals(SarifUtilities.StableSarifVersion))
                {
                    //  Transform file and write to output path.  Then continue rewriting as before.
                    //  TODO:  Consolidate this so only a single write is necessary.
                    TransformFileToVersionTwo(options, inputVersion, actualOutputPath);
                    //  We wrote to the output path, so read from it again.
                    actualLog = ReadSarifFile<SarifLog>(_fileSystem, actualOutputPath);
                }
                else
                {
                    //  Read from the original input path.
                    actualLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);
                }

                OptionallyEmittedData dataToInsert = options.DataToInsert.ToFlags();
                OptionallyEmittedData dataToRemove = options.DataToRemove.ToFlags();
                IDictionary<string, ArtifactLocation> originalUriBaseIds = options.ConstructUriBaseIdsDictionary();

                SarifLog reformattedLog = new RemoveOptionalDataVisitor(dataToRemove).VisitSarifLog(actualLog);
                reformattedLog = new InsertOptionalDataVisitor(dataToInsert, originalUriBaseIds).VisitSarifLog(reformattedLog);

                if (options.SarifOutputVersion == SarifVersion.OneZeroZero)
                {
                    var visitor = new SarifCurrentToVersionOneVisitor();
                    visitor.VisitSarifLog(reformattedLog);

                    WriteSarifFile(_fileSystem, visitor.SarifLogVersionOne, actualOutputPath, options.Minify, SarifContractResolverVersionOne.Instance);
                }
                else
                {
                    WriteSarifFile(_fileSystem, reformattedLog, actualOutputPath, options.Minify);
                }

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
            if (!rewriteOptions.Validate())
            {
                return false;
            }

            //  While this is returning true for inline cases, I think it's doing so for the wrong reasons.
            //  TODO: validate whether "actualOutputPath" can be created.
            if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(rewriteOptions.OutputFilePath, rewriteOptions.Force, _fileSystem))
            {
                return false;
            }

            return true;
        }

        //  TODO Move this into a separate class for better unit testing
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

        private void TransformFileToVersionTwo(SingleFileOptionsBase options, string inputVersion, string outputFilePath)
        {
            if (inputVersion == "1.0.0")
            {
                //  Converting version 1 to version 2
                SarifLogVersionOne actualLog = ReadSarifFile<SarifLogVersionOne>(_fileSystem, options.InputFilePath, SarifContractResolverVersionOne.Instance);
                var visitor = new SarifVersionOneToCurrentVisitor();
                visitor.VisitSarifLogVersionOne(actualLog);
                WriteSarifFile(_fileSystem, visitor.SarifLog, outputFilePath, options.Minify);
            }
            else
            {
                //  Converting prerelease version 2 to version 2
                PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                    _fileSystem.FileReadAllText(options.InputFilePath),
                    formatting: options.Formatting,
                    out string sarifText);

                _fileSystem.FileWriteAllText(outputFilePath, sarifText);
            }
        }
    }
}
