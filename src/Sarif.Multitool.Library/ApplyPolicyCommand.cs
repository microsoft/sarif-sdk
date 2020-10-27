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
            _fileSystem = fileSystem ?? FileSystem.Instance;
        }

        public int Run(ApplyPolicyOptions applyPolicyOptions)
        {
            try
            {
                Console.WriteLine($"Applying policy '{applyPolicyOptions.InputFilePath}' => '{applyPolicyOptions.OutputFilePath}'...");
                Stopwatch w = Stopwatch.StartNew();

                bool valid = ValidateOptions(applyPolicyOptions);
                if (!valid) { return FAILURE; }

                SarifLog actualLog = ReadSarifFile<SarifLog>(_fileSystem, applyPolicyOptions.InputFilePath);

                actualLog.ApplyPolicies();

                string fileName = CommandUtilities.GetTransformedOutputFileName(applyPolicyOptions);

                WriteSarifFile(_fileSystem, actualLog, fileName, applyPolicyOptions.Formatting);

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

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(applyPolicyOptions.OutputFilePath, applyPolicyOptions.Force, _fileSystem);

            return valid;
        }
    }
}
