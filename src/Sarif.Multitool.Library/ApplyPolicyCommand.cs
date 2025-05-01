// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ApplyPolicyCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public ApplyPolicyCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(ApplyPolicyOptions options)
        {
            try
            {
                Console.WriteLine($"Applying policy '{options.InputFilePath}' => '{options.OutputFilePath}'...");
                var w = Stopwatch.StartNew();

                bool valid = ValidateOptions(options);
                if (!valid) { return FAILURE; }

                SarifLog actualLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

                actualLog.ApplyPolicies();

                string fileName = CommandUtilities.GetTransformedOutputFileName(FileSystem, options);

                WriteSarifFile(_fileSystem, actualLog, fileName, options.Minify);

                w.Stop();
                Console.WriteLine($"Rewrite completed in {w.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private bool ValidateOptions(ApplyPolicyOptions applyPolicyOptions)
        {
            bool valid = true;

            valid &= applyPolicyOptions.Validate();

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(applyPolicyOptions.OutputFilePath, applyPolicyOptions.ForceOverwrite, _fileSystem);

            return valid;
        }
    }
}
