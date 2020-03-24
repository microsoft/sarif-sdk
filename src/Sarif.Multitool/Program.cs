// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
                ValidateOptions,
                ConvertOptions,
                RewriteOptions,
                TransformOptions,
                MergeOptions,
                RebaseUriOptions,
                AbsoluteUriOptions,
                PageOptions,
                QueryOptions,
                ResultMatchingOptions,
                ResultMatchSetOptions,
                FileWorkItemsOptions>(args)
                .MapResult(
                (ValidateOptions validateOptions) => new ValidateCommand().Run(validateOptions),
                (ConvertOptions convertOptions) => new ConvertCommand().Run(convertOptions),
                (RewriteOptions rewriteOptions) => new RewriteCommand().Run(rewriteOptions),
                (TransformOptions transformOptions) => new TransformCommand().Run(transformOptions),
                (MergeOptions mergeOptions) => new MergeCommand().Run(mergeOptions),
                (RebaseUriOptions rebaseOptions) => new RebaseUriCommand().Run(rebaseOptions),
                (AbsoluteUriOptions absoluteUriOptions) => new AbsoluteUriCommand().Run(absoluteUriOptions),
                (PageOptions pageOptions) => new PageCommand().Run(pageOptions),
                (QueryOptions queryOptions) => new QueryCommand().Run(queryOptions),
                (ResultMatchingOptions baselineOptions) => new ResultMatchingCommand().Run(baselineOptions),
                (ResultMatchSetOptions options) => new ResultMatchSetCommand().Run(options),
                (FileWorkItemsOptions fileWorkItemsOptions) => new FileWorkItemsCommand().Run(fileWorkItemsOptions),
                errs => CommandBase.FAILURE);
        }
    }
}
