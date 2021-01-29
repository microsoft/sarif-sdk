// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class Program
    {
        /// <summary>The entry point for the SARIF multi utility.</summary>
        /// <param name="args">Arguments passed in from the tool's command line.</param>
        /// <returns>0 on success; nonzero on failure.</returns>
        public static int Main(string[] args)
        {
            OptionsInterpretter optionsInterpretter = new OptionsInterpretter();

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
                ValidateOptions>(args)
                .WithParsed<AbsoluteUriOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
#if DEBUG
                .WithParsed<AnalyzeTestOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
#endif
                .WithParsed<ApplyPolicyOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ConvertOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ExportValidationConfigurationOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ExportValidationRulesMetadataOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<FileWorkItemsOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ResultMatchingOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<MergeOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<PageOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<QueryOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<RebaseUriOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<RewriteOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ValidateOptions>(x => { optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(x); })
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
