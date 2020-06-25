// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class OptimizeFileSize : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2004
        /// </summary>
        public override string Id => RuleId.OptimizeFileSize;

        /// <summary>
        /// Placeholder_SARIF2004_OptimizeFileSize_FullDescription_Text
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2004_OptimizeFileSize_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
                    nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text)
                };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        protected override void Analyze(ArtifactLocation artifactLocation, string artifactLocationPointer)
        {
            Result firstResult = Context.CurrentRun.Results[0];

            //if ()
            //{
            //    // {0}: Placeholder_SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text
            //    LogResult(
            //        regionPointer,
            //        nameof(RuleResources.SARIF2004_OptimizeFileSize_Warning_EliminateLocationOnlyArtifacts_Text));
            //}
        }
    }
}
