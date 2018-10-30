// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class AbsoluteUriCommand
    {
        private IFileSystem _fileSystem;

        public AbsoluteUriCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(AbsoluteUriOptions absoluteUriOptions)
        {
            try
            {
                var sarifFiles = GetSarifFiles(absoluteUriOptions);

                Directory.CreateDirectory(absoluteUriOptions.OutputFolderPath);
                foreach (var sarifLog in sarifFiles)
                {
                    sarifLog.Log = sarifLog.Log.MakeUrisAbsolute();

                    // Write output to file.
                    string outputName = sarifLog.GetOutputFileName(absoluteUriOptions);
                    var formatting = absoluteUriOptions.PrettyPrint
                        ? Formatting.Indented
                        : Formatting.None;

                    FileHelpers.WriteSarifFile(_fileSystem, sarifLog.Log, outputName, formatting);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        private IEnumerable<AbsoluteUriFile> GetSarifFiles(AbsoluteUriOptions absoluteUriOptions)
        {
            // Get files names first, as we may write more sarif logs to the same directory as we rebase them.
            HashSet<string> fileNames = FileHelpers.CreateTargetsSet(absoluteUriOptions.TargetFileSpecifiers, absoluteUriOptions.Recurse);
            foreach (var file in fileNames)
            {
                yield return new AbsoluteUriFile() { FileName = file, Log = FileHelpers.ReadSarifFile<SarifLog>(_fileSystem, file) };
            }
        }

        private class AbsoluteUriFile
        {
            public string FileName;

            public SarifLog Log;

            internal string GetOutputFileName(AbsoluteUriOptions mergeOptions)
            {
                return !string.IsNullOrEmpty(mergeOptions.OutputFolderPath)
                    ? Path.GetFullPath(mergeOptions.OutputFolderPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-absolute.sarif"
                    : Path.GetDirectoryName(FileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + "-absolute.sarif";
            }
        }
    }
}
