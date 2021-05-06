// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AbsoluteUriCommand : MultifileCommandBase
    {
        protected override string ProcessingName => "absolute";

        public int Run(AbsoluteUriOptions options)
        {
            try
            {
                IEnumerable<FileProcessingData> filesData = GetFilesToProcess(options);

                if (!ValidateOptions(options, filesData))
                {
                    return FAILURE;
                }

                foreach (FileProcessingData fileData in filesData)
                {
                    fileData.SarifLog = fileData.SarifLog.MakeUrisAbsolute();

                    WriteSarifFile(FileSystem,
                                   fileData.SarifLog,
                                   fileData.OutputFilePath,
                                   options.PrettyPrint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }
    }
}
