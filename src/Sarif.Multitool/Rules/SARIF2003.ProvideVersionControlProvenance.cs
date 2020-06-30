// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideVersionControlProvenance : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2003
        /// </summary>
        public override string Id => RuleId.ProvideVersionControlProvenance;

        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2003_ProvideVersionControlProvenance_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2003_ProvideVersionControlProvenance_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.VersionControlProvenance == null || run.VersionControlProvenance.Count == 0)
            {
                LogResult(
                    runPointer,
                    nameof(RuleResources.SARIF2003_ProvideVersionControlProvenance_Note_Default_Text));
            }
        }
    }
}
