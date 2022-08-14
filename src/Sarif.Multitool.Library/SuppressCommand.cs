// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Kusto.Cloud.Platform.Utils;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Query;
using Microsoft.CodeAnalysis.Sarif.Query.Evaluators;
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

                if (!string.IsNullOrWhiteSpace(options.Expression))
                {
                    var expressionGuids = ReturnQueryExpressionGuids(options);
                    if (options.ResultsGuids != null && options.ResultsGuids.Any())
                    {
                        options.ResultsGuids.Union(expressionGuids);
                    }
                    else
                    {
                        options.ResultsGuids = expressionGuids;
                    }
                }
                Console.WriteLine($"Suppressing {options.ResultsGuids.Count()} of {currentSarifLog.Runs.Sum(i => i.Results.Count)} results.");

                SarifLog reformattedLog = new SuppressVisitor(options.Justification,
                                                              options.Alias,
                                                              options.Guids,
                                                              options.Timestamps,
                                                              options.ExpiryInDays,
                                                              options.Status,
                                                              options.ResultsGuids).VisitSarifLog(currentSarifLog);

                string actualOutputPath = CommandUtilities.GetTransformedOutputFileName(options);
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
                Console.WriteLine($"Suppress completed in {w.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private IEnumerable<string> ReturnQueryExpressionGuids(SuppressOptions options)
        {
            int originalTotal = 0;
            int matchCount = 0;
            // Parse the Query and create a Result evaluator for it
            IExpression expression = ExpressionParser.ParseExpression(options.Expression);
            IExpressionEvaluator<Result> evaluator = expression.ToEvaluator<Result>(SarifEvaluators.ResultEvaluator);

            // Read the log
            SarifLog log = ReadSarifFile<SarifLog>(this.FileSystem, options.InputFilePath);

            foreach (Run run in log.Runs)
            {
                if (run.Results == null) { continue; }
                run.SetRunOnResults();

                originalTotal += run.Results.Count;

                // Find matches for Results in the Run
                BitArray matches = new BitArray(run.Results.Count);
                evaluator.Evaluate(run.Results, matches);

                // Count the new matches
                matchCount += matches.TrueCount();

                // Filter the Run.Results to the matches
                run.Results = matches.MatchingSubset<Result>(run.Results);
            }

            // Remove any Runs with no remaining matches
            log.Runs = log.Runs.Where(r => (r?.Results?.Count ?? 0) > 0).ToList();
            var guids = log.Runs.SelectMany(x => x.Results.Select(y => y.Guid)).ToList();

            return guids;
        }

        private bool ValidateOptions(SuppressOptions options)
        {
            bool valid = true;

            valid &= options.Validate();
            valid &= options.ExpiryInDays >= 0;
            valid &= !string.IsNullOrWhiteSpace(options.Justification);
            valid &= (options.Status == SuppressionStatus.Accepted || options.Status == SuppressionStatus.UnderReview);
            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(options.OutputFilePath, options.Force, FileSystem);

            return valid;
        }
    }
}
