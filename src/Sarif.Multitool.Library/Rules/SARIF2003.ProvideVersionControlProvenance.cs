// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// Provide 'versionControlProvenance' to record which version of the code was analyzed,
    /// and to enable paths to be expressed relative to the root of the repository.
    /// </summary>
    public class ProvideVersionControlProvenance : SarifValidationSkimmerBase
    {
        public ProvideVersionControlProvenance() : base(
            RuleId.ProvideVersionControlProvenance,
            RuleResources.SARIF2003_ProvideVersionControlProvenance_FullDescription_Text,
            FailureLevel.Note,
            new string[] { nameof(RuleResources.SARIF2003_ProvideVersionControlProvenance_Note_Default_Text) }
        )
        { }

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.VersionControlProvenance == null || run.VersionControlProvenance.Count == 0)
            {
                // {0}: This run does not provide 'versionControlProvenance'. As a result, it is
                // not possible to determine which version of code was analyzed, nor to map
                // relative paths to their locations within the repository.
                LogResult(
                    runPointer,
                    nameof(RuleResources.SARIF2003_ProvideVersionControlProvenance_Note_Default_Text));
            }
        }
    }
}
