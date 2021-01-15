// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public static class DriverExtensionMethods
    {
        public static LoggingOptions ConvertToLoggingOptions(this AnalyzeOptionsBase analyzeOptions)
        {
            LoggingOptions loggingOptions = LoggingOptions.PrettyPrint;

            if (analyzeOptions.Force) { loggingOptions |= LoggingOptions.OverwriteExistingOutputFile; }
            if (analyzeOptions.Minify) { loggingOptions ^= LoggingOptions.PrettyPrint; }
            if (analyzeOptions.Quiet) { loggingOptions |= LoggingOptions.Quiet; }
            if (analyzeOptions.Optimize) { loggingOptions |= LoggingOptions.Optimize; }
            if (analyzeOptions.PrettyPrint) { loggingOptions |= LoggingOptions.PrettyPrint; }

            return loggingOptions;
        }

        /// <summary>
        /// Ensures the consistency of the command line options related to the location and format
        /// of the output file, and adjusts the options for ease of use.
        /// </summary>
        /// <param name="options">
        /// A <see cref="SingleFileOptionsBase"/> object containing the command line options.
        /// </param>
        /// <returns>
        /// true if the options are internally consistent; otherwise false.
        /// </returns>
        public static bool Validate(this SingleFileOptionsBase options)
        {
            bool valid = true;

            valid &= options.ValidateOutputLocationOptions();
            valid &= options.ValidateOutputFormatOptions();

            return valid;
        }

        private static bool ValidateOutputLocationOptions(this SingleFileOptionsBase options)
        {
            bool valid = true;

            if (options.Inline)
            {
                if (options.OutputFilePath == null)
                {
                    options.Force = true;
                    options.OutputFilePath = options.InputFilePath;
                }
                else
                {
                    ReportInvalidOutputOptions();
                    valid = false;
                }
            }

            return valid;
        }

        private static void ReportInvalidOutputOptions()
        {
            string inlineOptionDescription = DriverUtilities.GetOptionDescription<SingleFileOptionsBase>(nameof(SingleFileOptionsBase.Inline));
            string outputFilePathOptionDescription = DriverUtilities.GetOptionDescription<SingleFileOptionsBase>(nameof(SingleFileOptionsBase.OutputFilePath));

            Console.Error.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    DriverResources.ExactlyOneOfTwoOptionsIsRequired,
                    inlineOptionDescription,
                    outputFilePathOptionDescription));
        }

        private static bool ValidateOutputFormatOptions(this SingleFileOptionsBase options)
        {
            bool valid = true;

            if (options.PrettyPrint && options.Minify)
            {
                ReportInvalidOutputFormatOptions();
                valid = false;
            }
            else if (!options.PrettyPrint && !options.Minify)
            {
                options.PrettyPrint = true;
            }

            return valid;
        }

        private static void ReportInvalidOutputFormatOptions()
        {
            string prettyPrintOptionDescription = DriverUtilities.GetOptionDescription<CommonOptionsBase>(nameof(CommonOptionsBase.PrettyPrint));
            string minifyOptionDescription = DriverUtilities.GetOptionDescription<CommonOptionsBase>(nameof(CommonOptionsBase.Minify));

            Console.Error.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    DriverResources.OptionsAreMutuallyExclusive,
                    prettyPrintOptionDescription,
                    minifyOptionDescription));
        }

        /// <summary>
        /// Ensures the consistency of the MultipleFilesOptionsBase command line options related to
        /// the location of the output file, and adjusts the options for ease of use.
        /// </summary>
        /// <param name="options">
        /// A <see cref="MultipleFilesOptionsBase"/> object containing the relevant options.
        /// </param>
        /// <returns>
        /// true if the options are internally consistent; otherwise false.
        /// </returns>
        /// <remarks>
        /// At this time, this method does not actually do any validation. Unlike the case of
        /// SingleFileOptionsBase, where you have to specify exactly one of --inline and
        /// --output-file, it is _not_ necessary to specify --output-folder-path if --inline is
        /// absent, because by default each transformed file is written to the path containing
        /// corresponding input file.
        ///
        /// However, similarly to the case of SingleFileOptionsBase, we _do_ want to set --force
        /// whenever --inline is true, because there's no reason to force the user to type
        /// "--force" when they've already said that they want to overwrite the input file
        /// (see https://github.com/microsoft/sarif-sdk/issues/1642).
        ///
        /// So we introduce this method for three reasons:
        /// 1) For symmetry with the SingleFileOptionsBase,
        /// 2) To DRY out the logic for making --inline and --force consistent, and
        /// 3) To leave an obvious place to put output file option consistency logic if it's
        ///    needed in future.
        /// </remarks>
        public static bool ValidateOutputOptions(this MultipleFilesOptionsBase options)
        {
            bool valid = true;

            if (options.Inline)
            {
                options.Force = true;
            }

            return valid;
        }
    }
}
