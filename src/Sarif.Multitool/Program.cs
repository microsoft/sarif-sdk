// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
            var optionsInterpreter = new OptionsInterpreter();

            // Use a custom Parser so enum values bind case-insensitively
            // (e.g. --rule-kind ghazdo, GHAzDO, GHAZDO all map to RuleKind.GHAzDO).
            // Use the non-generic ParseArguments overload to side-step the 16-type-parameter
            // ceiling on the strongly-typed overloads — the verb roster has outgrown it.
            // Keep the verb list in alphabetical order.
            using var parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
                with.HelpWriter = Console.Error;
            });

            var verbTypes = new List<Type>
            {
                typeof(AbsoluteUriOptions),
                typeof(AddInvocationOptions),
                typeof(AddReportingDescriptorOptions),
                typeof(AddResultOptions),
#if DEBUG
                typeof(AnalyzeTestOptions),
#endif
                typeof(ApplyPolicyOptions),
                typeof(ConvertOptions),
                typeof(EmitFinalizeOptions),
                typeof(EmitInitRunOptions),
                typeof(ExportValidationConfigurationOptions),
                typeof(ExportValidationRulesMetadataOptions),
                typeof(FileWorkItemsOptions),
                typeof(ResultMatchingOptions),
                typeof(MergeOptions),
                typeof(PageOptions),
                typeof(PartitionOptions),
                typeof(QueryOptions),
                typeof(RebaseUriOptions),
                typeof(RewriteOptions),
                typeof(SuppressOptions),
                typeof(ValidateOptions),
            };

            return parser.ParseArguments(args, verbTypes.ToArray())
                .WithParsed<AbsoluteUriOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
#if DEBUG
                .WithParsed<AnalyzeTestOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
#endif
                .WithParsed<ApplyPolicyOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ConvertOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                // emit-* verbs carry no environment-variable plumbing.
                .WithParsed<ExportValidationConfigurationOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ExportValidationRulesMetadataOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<FileWorkItemsOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ResultMatchingOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<MergeOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<PageOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<PartitionOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<QueryOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<RebaseUriOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<RewriteOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<SuppressOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .WithParsed<ValidateOptions>(x => { optionsInterpreter.ConsumeEnvVarsAndInterpretOptions(x); })
                .MapResult(
                    Dispatch,
                    errors => HandleParseError(args));
        }

        private static int Dispatch(object options)
        {
            return options switch
            {
                AbsoluteUriOptions o => new AbsoluteUriCommand().Run(o),
                AddInvocationOptions o => new AddInvocationCommand().Run(o),
                AddReportingDescriptorOptions o => new AddReportingDescriptorCommand().Run(o),
                AddResultOptions o => new AddResultCommand().Run(o),
#if DEBUG
                AnalyzeTestOptions o => new AnalyzeTestCommand().Run(o),
#endif
                ApplyPolicyOptions o => new ApplyPolicyCommand().Run(o),
                ConvertOptions o => new ConvertCommand().Run(o),
                EmitFinalizeOptions o => new EmitFinalizeCommand().Run(o),
                EmitInitRunOptions o => new EmitInitRunCommand().Run(o),
                ExportValidationConfigurationOptions o => new ExportValidationConfigurationCommand().Run(o),
                ExportValidationRulesMetadataOptions o => new ExportValidationRulesMetadataCommand().Run(o),
                FileWorkItemsOptions o => new FileWorkItemsCommand().Run(o),
                ResultMatchingOptions o => new ResultMatchingCommand().Run(o),
                MergeOptions o => new MergeCommand().Run(o),
                PageOptions o => new PageCommand().Run(o),
                PartitionOptions o => new PartitionCommand().Run(o),
                QueryOptions o => new QueryCommand().Run(o),
                RebaseUriOptions o => new RebaseUriCommand().Run(o),
                RewriteOptions o => new RewriteCommand().Run(o),
                SuppressOptions o => new SuppressCommand().Run(o),
                ValidateOptions o => new ValidateCommand().Run(o),
                _ => CommandBase.FAILURE,
            };
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
