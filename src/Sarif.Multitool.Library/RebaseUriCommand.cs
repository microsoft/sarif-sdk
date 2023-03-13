// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RebaseUriCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public RebaseUriCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(RebaseUriOptions options)
        {
            try
            {
                if (!Uri.TryCreate(options.BasePath, UriKind.Absolute, out Uri baseUri))
                {
                    Console.Error.WriteLine($"The value '{options.BasePath}' of the --base-path-value option is not an absolute URI.");
                    return FAILURE;
                }

                // In case someone accidentally passes C:\bld\src and meant C:\bld\src\--the base path should always be a folder, not something that points to a file.
                if (!string.IsNullOrEmpty(baseUri.GetFileName()))
                {
                    baseUri = new Uri(baseUri.ToString() + "/");
                }

                IEnumerable<RebaseUriFile> rebaseUriFiles = GetRebaseUriFiles(options);

                if (!ValidateOptions(options, rebaseUriFiles)) { return FAILURE; }

                if (!options.Inline)
                {
                    _fileSystem.DirectoryCreateDirectory(options.OutputDirectoryPath);
                }

                OptionallyEmittedData dataToRemove = options.DataToRemove.ToFlags();
                OptionallyEmittedData dataToInsert = options.DataToInsert.ToFlags();

                foreach (RebaseUriFile rebaseUriFile in rebaseUriFiles)
                {
                    if (dataToRemove != 0)
                    {
                        rebaseUriFile.Log = new RemoveOptionalDataVisitor(dataToRemove).VisitSarifLog(rebaseUriFile.Log);
                    }

                    if (dataToInsert != 0)
                    {
                        rebaseUriFile.Log = new InsertOptionalDataVisitor(dataToInsert, insertProperties: options.InsertProperties).VisitSarifLog(rebaseUriFile.Log);
                    }

                    rebaseUriFile.Log = rebaseUriFile.Log.RebaseUri(options.BasePathToken, options.RebaseRelativeUris, baseUri);

                    WriteSarifFile(_fileSystem, rebaseUriFile.Log, rebaseUriFile.OutputFilePath, options.Minify);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private IEnumerable<RebaseUriFile> GetRebaseUriFiles(RebaseUriOptions rebaseUriOptions)
        {
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.
            HashSet<string> inputFilePaths = CreateTargetsSet(rebaseUriOptions.TargetFileSpecifiers, rebaseUriOptions.Recurse, _fileSystem);
            foreach (string inputFilePath in inputFilePaths)
            {
                yield return new RebaseUriFile
                {
                    InputFilePath = inputFilePath,
                    OutputFilePath = GetOutputFilePath(inputFilePath, rebaseUriOptions),
                    Log = ReadSarifFile<SarifLog>(_fileSystem, inputFilePath)
                };
            }
        }

        private bool ValidateOptions(RebaseUriOptions rebaseOptions, IEnumerable<RebaseUriFile> rebaseUriFiles)
        {
            bool valid = true;

            valid &= rebaseOptions.ValidateOutputOptions();

            valid &= DriverUtilities.ReportWhetherOutputFilesCanBeCreated(rebaseUriFiles.Select(f => f.OutputFilePath), rebaseOptions.ForceOverwrite, _fileSystem);

            return valid;
        }

        internal string GetOutputFilePath(string inputFilePath, RebaseUriOptions rebaseUriOptions)
        {
            if (rebaseUriOptions.Inline) { return inputFilePath; }

            return !string.IsNullOrEmpty(rebaseUriOptions.OutputDirectoryPath)
                ? Path.GetFullPath(rebaseUriOptions.OutputDirectoryPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inputFilePath) + "-rebased.sarif"
                : Path.GetDirectoryName(inputFilePath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(inputFilePath) + "-rebased.sarif";
        }

        private class RebaseUriFile
        {
            public string InputFilePath;
            public string OutputFilePath;

            public SarifLog Log;
        }
    }
}
