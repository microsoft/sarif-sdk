// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ResultMatchingCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public ResultMatchingCommand(IFileSystem fileSystem = null)
        {
            this._fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(ResultMatchingOptions options)
        {
            try
            {
                SarifLog baselineFile = null;
                if (!string.IsNullOrEmpty(options.PreviousFilePath))
                {
                    baselineFile = ReadSarifFile<SarifLog>(_fileSystem, options.PreviousFilePath);
                }

                string outputFilePath = options.OutputFilePath;
                if (string.IsNullOrEmpty(outputFilePath))
                {
                    outputFilePath = Path.GetFileNameWithoutExtension(options.PreviousFilePath) + "-annotated.sarif";
                }

                if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, options.Force, _fileSystem))
                {
                    return FAILURE;
                }

                var currentSarifLogs = new List<SarifLog>();

                foreach (string currentFilePath in options.CurrentFilePaths)
                {
                    currentSarifLogs.Add(ReadSarifFile<SarifLog>(_fileSystem, currentFilePath));
                }

                ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();

                SarifLog output = matcher.Match(new SarifLog[] { baselineFile }, currentSarifLogs).First();

                WriteSarifFile(_fileSystem, output, outputFilePath, options.Minify);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return FAILURE;
            }

            return SUCCESS;
        }
    }
}
