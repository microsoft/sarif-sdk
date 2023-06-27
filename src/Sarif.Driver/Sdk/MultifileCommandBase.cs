// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class MultifileCommandBase : CommandBase
    {
        /// <summary>
        /// Gets a label that reflects the processing capability of this command. This
        /// label will be used as a suffix for default log file names created during
        /// execution. e.g., a ProcessingName value of 'merged' applied to an input
        /// file named 'MyInput.sarif' will, by default, create an output log file named
        /// 'MyInput-merged.sarif'.
        /// </summary>
        protected abstract string ProcessingName { get; }

        protected IEnumerable<FileProcessingData> GetFilesToProcess(MultipleFilesOptionsBase options)
        {
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.

            HashSet<string> inputFilePaths = CreateTargetsSet(options.TargetFileSpecifiers, options.Recurse, FileSystem);

            foreach (string inputFilePath in inputFilePaths)
            {
                yield return new FileProcessingData
                {
                    InputFilePath = inputFilePath,
                    OutputFilePath = GetOutputFilePath(inputFilePath, options),
                    SarifLog = ReadSarifFile<SarifLog>(FileSystem, inputFilePath)
                };
            }
        }

        internal string GetOutputFilePath(string inputFilePath, MultipleFilesOptionsBase absoluteUriOptions)
        {
            if (absoluteUriOptions.Inline) { return inputFilePath; }

            return !string.IsNullOrEmpty(absoluteUriOptions.OutputDirectoryPath) ?
                Path.GetFullPath(absoluteUriOptions.OutputDirectoryPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inputFilePath) + $"-{ProcessingName}.sarif" :
                Path.GetDirectoryName(inputFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inputFilePath) + $"-{ProcessingName}.sarif";
        }

        protected bool ValidateOptions(MultipleFilesOptionsBase options, IEnumerable<FileProcessingData> filesData)
        {
            bool valid = true;

            valid &= options.ValidateOutputOptions();

            valid &= DriverUtilities.ReportWhetherOutputFilesCanBeCreated(filesData.Select(f => f.OutputFilePath), options.ForceOverwrite, FileSystem);

            if (!options.Inline)
            {
                FileSystem.DirectoryCreateDirectory(options.OutputDirectoryPath);
            }

            return valid;
        }
    }
}
