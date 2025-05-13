// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SuppressCommand : CommandBase
    {
        public SuppressCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
        }

        public int Run(SuppressOptions options)
        {
            try
            {
                Console.WriteLine($"Suppress '{options.InputFilePath}' => '{options.OutputFilePath}'...");
                var w = Stopwatch.StartNew();

                bool valid = ValidateOptions(options);
                if (!valid)
                {
                    return FAILURE;
                }

                SarifLog currentSarifLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(FileSystem.FileReadAllText(options.InputFilePath),
                                                                                                     options.Formatting,
                                                                                                     out string _);

                SarifLog reformattedLog = new SuppressVisitor(options.Justification,
                                                              options.Alias,
                                                              options.Guids,
                                                              options.Timestamps,
                                                              options.ExpiryInDays,
                                                              options.Status).VisitSarifLog(currentSarifLog);

                string actualOutputPath = CommandUtilities.GetTransformedOutputFileName(FileSystem, options);
                if (options.SarifOutputVersion == SarifVersion.OneZeroZero)
                {
                    var visitor = new SarifCurrentToVersionOneVisitor();
                    visitor.VisitSarifLog(reformattedLog);

                    WriteSarifFile(FileSystem, visitor.SarifLogVersionOne, actualOutputPath, options.Formatting, SarifContractResolverVersionOne.Instance);
                }
                else
                {
                    WriteSarifFile(FileSystem, reformattedLog, actualOutputPath, options.Formatting);
                }

                w.Stop();
                Console.WriteLine($"Supress completed in {w.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private bool ValidateOptions(SuppressOptions options)
        {
            bool valid = true;

            valid &= options.Validate();
            valid &= options.ExpiryInDays >= 0;
            valid &= !string.IsNullOrWhiteSpace(options.Justification);
            valid &= (options.Status == SuppressionStatus.Accepted || options.Status == SuppressionStatus.UnderReview);
            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(options.OutputFilePath, options.ForceOverwrite, FileSystem);

            return valid;
        }
    }
}
