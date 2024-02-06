// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredSarifLogProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2008_ProvideSchema_FullDescription_Text };

        public virtual int MaximumRuns { get; set; } = int.MaxValue;

        protected override void Analyze(SarifLog sarifLog, string logPointer)
        {
            if (sarifLog.Runs == null)
            {
                // {0}: This 'sarifLog' object does not provide a 'runs' array, which is required by the {1} service.
                LogResult(
                    logPointer,
                    nameof(RuleResources.Base1013_SarifLogRunsArray_Note_Default_Text),
                    this.ServiceName);
            }
            else
            {
                if (!Context.InputLogToken.HasProperty(SarifPropertyName.Schema))
                {
                    // {0}: The SARIF log file does not contain a '$schema' property. Add a '$schema'
                    // property that refers to the final, OASIS standard version of the SARIF 2.1.0
                    // schema. This enables IDEs to provide Intellisense for SARIF log files.
                    LogResult(
                        logPointer,
                        nameof(RuleResources.Base1013_ProvideSchema_Warning_Default_Text));
                }

                if (sarifLog.Version != SarifVersion.Current)
                {
                    // {0}: The SARIF log file does not specify 'version' property that refers to
                    // the final, OASIS standard version of the SARIF 2.1.0 schema. This enables
                    // IDEs to provide Intellisense for SARIF log files.
                    LogResult(
                        logPointer,
                        nameof(RuleResources.Base1013_ProvideSchemaVersion_Warning_Default_Text));
                }

                if (sarifLog.Runs.Count > this.MaximumRuns)
                {
                    // {0}: This 'sarifLog' object's 'runs' array contains {1} element(s), which exceeds the limit of {2} imposed by the {3} service.
                    LogResult(
                        logPointer,
                        nameof(RuleResources.Base1013_MaximumRunsCount_Note_Default_Text),
                        sarifLog.Runs.Count.ToString(),
                        this.MaximumRuns.ToString(),
                        this.ServiceName);
                }
            }
        }
    }
}
