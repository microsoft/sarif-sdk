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
                var w = Stopwatch.StartNew();

                bool valid = ValidateOptions(options);
                if (!valid)
                {
                    return FAILURE;
                }

                string actualOutputPath = CommandUtilities.GetTransformedOutputFileName(FileSystem, options);

                SarifLog actualLog = null;

                string inputVersion = SniffVersion(options.InputFilePath);
                if (!inputVersion.Equals(SarifUtilities.StableSarifVersion))
                {
                    actualLog = TransformFileToVersionTwo(options.InputFilePath, inputVersion);
                }
                else
                {
                    actualLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);
                }

                OptionallyEmittedData dataToInsert = options.DataToInsert.ToFlags();
                OptionallyEmittedData dataToRemove = options.DataToRemove.ToFlags();
                IDictionary<string, ArtifactLocation> originalUriBaseIds = options.ConstructUriBaseIdsDictionary();

                SarifLog reformattedLog = new RemoveOptionalDataVisitor(dataToRemove).VisitSarifLog(actualLog);

                reformattedLog = new InsertOptionalDataVisitor(dataToInsert,
                                                               new FileRegionsCache(),
                                                               originalUriBaseIds,
                                                               insertProperties: options.InsertProperties).VisitSarifLog(reformattedLog);

                if (options.SortResults)
                {
                    reformattedLog = new SortingVisitor().VisitSarifLog(reformattedLog);
                }

                if (options.NormalizeForGhas)
                {
                    if (options.BasePath != null && options.BasePathToken != null)
                    {
                        var visitor = new RebaseUriVisitor(options.BasePathToken, new Uri(options.BasePath), options.RebaseRelativeUris);
                        reformattedLog = visitor.VisitSarifLog(reformattedLog);
                    }

                    reformattedLog = new GitHubIngestionVisitor().VisitSarifLog(reformattedLog);
                }

                if (options.SarifOutputVersion == SarifVersion.OneZeroZero)
                {
                    var visitor = new SarifCurrentToVersionOneVisitor();
                    visitor.VisitSarifLog(reformattedLog);

                    bool minify = options.OutputFileOptions.ToFlags().HasFlag(FilePersistenceOptions.Minify);
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
            //  #2270 https://github.com/microsoft/sarif-sdk/issues/2270
            if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(rewriteOptions.OutputFilePath, rewriteOptions.ForceOverwrite, _fileSystem))
            {
                return false;
            }

            return true;
        }

        //  TODO Move this into a separate class for better unit testing
        //  #2271 https://github.com/microsoft/sarif-sdk/issues/2271
        private string SniffVersion(string sarifPath)
        {
            using (var reader = new JsonTextReader(new StreamReader(_fileSystem.FileOpenRead(sarifPath))))
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

        private SarifLog TransformFileToVersionTwo(string inputFilePath, string inputVersion)
        {
            if (inputVersion == "1.0.0")
            {
                //  Converting version 1 to version 2
                SarifLogVersionOne actualLog = ReadSarifFile<SarifLogVersionOne>(_fileSystem, inputFilePath, SarifContractResolverVersionOne.Instance);
                var visitor = new SarifVersionOneToCurrentVisitor();
                visitor.VisitSarifLogVersionOne(actualLog);
                return visitor.SarifLog;
            }
            else
            {
                //  Converting prerelease version 2 to version 2
                return PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                    _fileSystem.FileReadAllText(inputFilePath),
                    formatting: Formatting.None,
                    out string _);
            }
        }
    }
}
