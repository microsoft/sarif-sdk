// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public static class DriverExtensionMethods
    {
        public static LoggingOptions ConvertToLoggingOptions(this AnalyzeOptionsBase analyzeOptions)
        {
            LoggingOptions loggingOptions = LoggingOptions.None;

            if (analyzeOptions.Verbose) { loggingOptions |= LoggingOptions.Verbose; }
            if (analyzeOptions.PrettyPrint) { loggingOptions |= LoggingOptions.PrettyPrint; }
            if (analyzeOptions.Force) { loggingOptions |= LoggingOptions.OverwriteExistingOutputFile; }

            return loggingOptions;
        }

        /// <summary>
        /// Ensures the consistency of the command line options related to the location of the
        /// output file, and adjusts the options for ease of use.
        /// </summary>
        /// <param name="options">
        /// An object containing the relevant options.
        /// </param>
        /// <returns>
        /// true if the options
        /// </returns>
        public static bool ValidateOutputOptions(this SingleFileOptionsBase options)
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
                    Console.Error.WriteLine(DriverResources.ExactlyOneOfOutputFilePathAndInlineOptions);
                    valid = false;
                }
            }
            else
            {
                if (options.OutputFilePath == null)
                {
                    Console.Error.WriteLine(DriverResources.ExactlyOneOfOutputFilePathAndInlineOptions);
                    valid = false;
                }
            }

            return valid;
        }
    }
}
