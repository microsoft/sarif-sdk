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
                ConvertOptions,
                RewriteOptions,
                TransformOptions,
                MergeOptions,
                RebaseUriOptions,
                AbsoluteUriOptions,
                BaselineOptions>(args)
              .MapResult(
                (ConvertOptions convertOptions) => ConvertCommand.Run(convertOptions),
                (RewriteOptions rewriteOptions) => RewriteCommand.Run(rewriteOptions),
                (TransformOptions transformOptions) => TransformCommand.Run(transformOptions),
                (MergeOptions mergeOptions) => MergeCommand.Run(mergeOptions),
                (RebaseUriOptions rebaseOptions) => RebaseUriCommand.Run(rebaseOptions),
                (AbsoluteUriOptions absoluteUriOptions) => AbsoluteUriCommand.Run(absoluteUriOptions),
                (BaselineOptions baselineOptions) => BaselineCommand.Run(baselineOptions),
                errs => 1);
        }
    }
}
