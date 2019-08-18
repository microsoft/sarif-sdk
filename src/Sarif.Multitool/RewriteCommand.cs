// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class RewriteCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public RewriteCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(RewriteOptions rewriteOptions)
        {
            try
            {
                bool valid = ValidateOptions(rewriteOptions);
                if (!valid) { return 1; }

                SarifLog actualLog = ReadSarifFile<SarifLog>(_fileSystem, rewriteOptions.InputFilePath);

                OptionallyEmittedData dataToInsert = rewriteOptions.DataToInsert.ToFlags();
                IDictionary<string, ArtifactLocation> originalUriBaseIds = rewriteOptions.ConstructUriBaseIdsDictionary();

                SarifLog reformattedLog = new InsertOptionalDataVisitor(dataToInsert, originalUriBaseIds).VisitSarifLog(actualLog);
                
                string fileName = CommandUtilities.GetTransformedOutputFileName(rewriteOptions);

                var formatting = rewriteOptions.PrettyPrint
                    ? Formatting.Indented
                    : Formatting.None;

                WriteSarifFile(_fileSystem, reformattedLog, fileName, formatting);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        private bool ValidateOptions(RewriteOptions rewriteOptions)
        {
            bool valid = true;

            valid &= rewriteOptions.ValidateOutputOptions();

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(rewriteOptions.OutputFilePath, rewriteOptions.Force, _fileSystem);

            return valid;
        }
    }
}
