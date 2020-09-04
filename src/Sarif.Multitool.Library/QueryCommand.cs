// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Query;
using Microsoft.CodeAnalysis.Sarif.Query.Evaluators;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class QueryCommand : CommandBase
    {
        private const int TOO_MANY_RESULTS = 2;

        private readonly IFileSystem _fileSystem;

        public QueryCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(QueryOptions options)
        {
            try
            {
                return RunWithoutCatch(options);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }
        }

        public int RunWithoutCatch(QueryOptions options)
        {
            bool valid = DriverUtilities.ReportWhetherOutputFileCanBeCreated(options.OutputFilePath, options.Force, _fileSystem);
            if (!valid) { return FAILURE; }

            Stopwatch w = Stopwatch.StartNew();
            int originalTotal = 0;
            int matchCount = 0;

            // Parse the Query and create a Result evaluator for it
            IExpression expression = ExpressionParser.ParseExpression(options.Expression);
            IExpressionEvaluator<Result> evaluator = expression.ToEvaluator<Result>(SarifEvaluators.ResultEvaluator);

            // Read the log
            SarifLog log = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

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

                // Write to console, if caller requested
                if (options.WriteToConsole)
                {
                    foreach (Result result in run.Results)
                    {
                        Console.WriteLine(result.FormatForVisualStudio());
                    }
                }
            }

            // Remove any Runs with no remaining matches
            log.Runs = log.Runs.Where(r => (r?.Results?.Count ?? 0) > 0).ToList();

            w.Stop();
            Console.WriteLine($"Found {matchCount:n0} of {originalTotal:n0} results matched in {w.Elapsed.TotalSeconds:n1}s.");

            // Write to Output file, if caller requested
            if (!string.IsNullOrEmpty(options.OutputFilePath) && (options.Force || !_fileSystem.FileExists(options.OutputFilePath)))
            {
                Console.WriteLine($"Writing matches to {options.OutputFilePath}.");
                WriteSarifFile<SarifLog>(_fileSystem, log, options.OutputFilePath, (options.PrettyPrint ? Formatting.Indented : Formatting.None));
            }

            // Return exit code based on configuration
            if (options.ReturnCount)
            {
                return matchCount;
            }
            else if (options.NonZeroExitCodeIfCountOver >= 0 && matchCount > options.NonZeroExitCodeIfCountOver)
            {
                return TOO_MANY_RESULTS;
            }
            else
            {
                return SUCCESS;
            }
        }
    }
}
