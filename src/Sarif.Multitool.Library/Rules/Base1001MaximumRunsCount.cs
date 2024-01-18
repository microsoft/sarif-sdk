// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseMaximumRunsCount
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        public virtual int MaximumRuns { get; set; }

        protected override void Analyze(SarifLog sarifLog, string runPointer)
        {
            if (sarifLog.Runs?.Count > this.MaximumRuns)
            {
                // {0}: This 'sarifLog' object's 'runs' array contains {1} elements, which exceeds the limit of {2} imposed by {3}.
                LogResult(
                    runPointer,
                    nameof(RuleResources.Base1001_MaximumRunsCount_Note_Default_Text),
                    sarifLog.Runs.Count.ToString(),
                    this.MaximumRuns.ToString(),
                    this.ServiceName);
            }
        }
    }
}
