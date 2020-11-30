// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class Program
    {
        /// <summary>The entry point for the SARIF multi utility.</summary>
        /// <param name="args">Arguments passed in from the tool's command line.</param>
        /// <returns>0 on success; nonzero on failure.</returns>
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<
                // Keep this in alphabetical order
                AbsoluteUriOptions,
#if DEBUG
                AnalyzeTestOptions,
#endif
                ApplyPolicyOptions,
                ConvertOptions,
                ExportValidationConfigurationOptions,
                ExportValidationRulesMetadataOptions,
                FileWorkItemsOptions,
                ResultMatchingOptions,
                MergeOptions,
                PageOptions,
                QueryOptions,
                RebaseUriOptions,
                RewriteOptions,
                TransformOptions,
                ValidateOptions>(args)
                .MapResult(
                (AbsoluteUriOptions absoluteUriOptions) => new AbsoluteUriCommand().Run(absoluteUriOptions),
#if DEBUG
                (AnalyzeTestOptions fileWorkItemsOptions) => new AnalyzeTestCommand().Run(fileWorkItemsOptions),
#endif
                (ApplyPolicyOptions options) => new ApplyPolicyCommand().Run(options),
                (ConvertOptions convertOptions) => new ConvertCommand().Run(convertOptions),
                (ExportValidationConfigurationOptions options) => new ExportValidationConfigurationCommand().Run(options),
                (ExportValidationRulesMetadataOptions options) => new ExportValidationRulesMetadataCommand().Run(options),
                (FileWorkItemsOptions fileWorkItemsOptions) => new FileWorkItemsCommand().Run(fileWorkItemsOptions),
                (ResultMatchingOptions baselineOptions) => new ResultMatchingCommand().Run(baselineOptions),
                (MergeOptions mergeOptions) => new MergeCommand().Run(mergeOptions),
                (PageOptions pageOptions) => new PageCommand().Run(pageOptions),
                (QueryOptions queryOptions) => new QueryCommand().Run(queryOptions),
                (RebaseUriOptions rebaseOptions) => new RebaseUriCommand().Run(rebaseOptions),
                (RewriteOptions rewriteOptions) => new RewriteCommand().Run(rewriteOptions),
                (TransformOptions transformOptions) => new TransformCommand().Run(transformOptions),
                (ValidateOptions validateOptions) => new ValidateCommand().Run(validateOptions),
                 _ => HandleParseError(args));
        }

        private static int HandleParseError(string[] args)
        {
            string[] validArgs = new[] { "help", "version", "--version", "--help" };
            return args.Any(arg => validArgs.Contains(arg, StringComparer.OrdinalIgnoreCase))
                ? CommandBase.SUCCESS
                : CommandBase.FAILURE;
        }
    }
}
