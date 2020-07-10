// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AbsoluteUriCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public AbsoluteUriCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(AbsoluteUriOptions absoluteUriOptions)
        {
            try
            {
                IEnumerable<AbsoluteUriFile> absoluteUriFiles = GetAbsoluteUriFiles(absoluteUriOptions);

                if (!ValidateOptions(absoluteUriOptions, absoluteUriFiles)) { return FAILURE; }

                if (!absoluteUriOptions.Inline)
                {
                    _fileSystem.CreateDirectory(absoluteUriOptions.OutputFolderPath);
                }

                Formatting formatting = absoluteUriOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                foreach (AbsoluteUriFile absoluteUriFile in absoluteUriFiles)
                {
                    absoluteUriFile.Log = absoluteUriFile.Log.MakeUrisAbsolute();

                    WriteSarifFile(_fileSystem, absoluteUriFile.Log, absoluteUriFile.OutputFilePath, formatting);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private bool ValidateOptions(AbsoluteUriOptions absoluteUriOptions, IEnumerable<AbsoluteUriFile> absoluteUriFiles)
        {
            bool valid = true;

            valid &= absoluteUriOptions.ValidateOutputOptions();

            valid &= DriverUtilities.ReportWhetherOutputFilesCanBeCreated(absoluteUriFiles.Select(f => f.OutputFilePath), absoluteUriOptions.Force, _fileSystem);

            return valid;
        }

        private IEnumerable<AbsoluteUriFile> GetAbsoluteUriFiles(AbsoluteUriOptions absoluteUriOptions)
        {
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.
            HashSet<string> inputFilePaths = CreateTargetsSet(absoluteUriOptions.TargetFileSpecifiers, absoluteUriOptions.Recurse, _fileSystem);
            foreach (string inputFilePath in inputFilePaths)
            {
                yield return new AbsoluteUriFile
                {
                    InputFilePath = inputFilePath,
                    OutputFilePath = GetOutputFilePath(inputFilePath, absoluteUriOptions),
                    Log = ReadSarifFile<SarifLog>(_fileSystem, inputFilePath)
                };
            }
        }

        internal string GetOutputFilePath(string inputFilePath, AbsoluteUriOptions absoluteUriOptions)
        {
            if (absoluteUriOptions.Inline) { return inputFilePath; }

            return !string.IsNullOrEmpty(absoluteUriOptions.OutputFolderPath)
                ? Path.GetFullPath(absoluteUriOptions.OutputFolderPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inputFilePath) + "-absolute.sarif"
                : Path.GetDirectoryName(inputFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inputFilePath) + "-absolute.sarif";
        }

        private class AbsoluteUriFile
        {
            public string InputFilePath;
            public string OutputFilePath;

            public SarifLog Log;
        }
    }
}
