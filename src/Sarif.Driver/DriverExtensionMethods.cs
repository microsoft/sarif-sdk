// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public static class DriverExtensionMethods
    {
        public static LogFilePersistenceOptions ConvertToLogFilePersistenceOptions(this AnalyzeOptionsBase analyzeOptions)
        {
            LogFilePersistenceOptions logFilePersistenceOptions = LogFilePersistenceOptions.PrettyPrint;

            if (analyzeOptions.Force) { logFilePersistenceOptions |= LogFilePersistenceOptions.OverwriteExistingOutputFile; }
            if (analyzeOptions.Minify) { logFilePersistenceOptions ^= LogFilePersistenceOptions.PrettyPrint; }
            if (analyzeOptions.Optimize) { logFilePersistenceOptions |= LogFilePersistenceOptions.Optimize; }
            if (analyzeOptions.PrettyPrint) { logFilePersistenceOptions |= LogFilePersistenceOptions.PrettyPrint; }

            return logFilePersistenceOptions;
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
            if (options.SarifOutputVersion == SarifVersion.Unknown)
            {
                //  Parsing the output version failed and the the enum evaluated to 0.
                Console.WriteLine(DriverResources.ErrorInvalidTransformTargetVersion);
                return false;
            }

            if (!options.ValidateOutputLocationOptions())
            {
                return false;
            }

            if (!options.ValidateOutputFormatOptions())
            {
                return false;
            }

            return true;
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

            //  TODO:  Is it correct to modify options in a "validate" method?
            //  #2267 https://github.com/microsoft/sarif-sdk/issues/2267
            if (options.Inline)
            {
                options.Force = true;
            }

            return valid;
        }

        public static bool ValidateOutputOptions(this AnalyzeOptionsBase options, IAnalysisContext context)
        {
            bool valid = true;

            if (options.Quiet && string.IsNullOrWhiteSpace(options.OutputFilePath))
            {
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(options.OutputFilePath) && !string.IsNullOrWhiteSpace(options.BaselineSarifFile))
            {
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(options.OutputFilePath) && !string.IsNullOrWhiteSpace(options.PostUri))
            {
                valid = false;
            }

            if (!string.IsNullOrWhiteSpace(options.PostUri) && !Uri.IsWellFormedUriString(options.PostUri, UriKind.Absolute))
            {
                valid = false;
            }

            if (!valid)
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
            }

            return valid;
        }
    }
}
