// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotPersistFingerprints : SarifValidationSkimmerBase
    {
        public DoNotPersistFingerprints()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1007
        /// </summary>
        public override string Id => RuleId.AIDoNotPersistFingerprints;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1007_DoNotPersistFingerprints_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1007_DoNotPersistFingerprints_Error_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Fingerprints != null && result.Fingerprints.Count > 0)
            {
                LogResult(
                    resultPointer.AtProperty(SarifPropertyName.Fingerprints),
                    nameof(RuleResources.AI1007_DoNotPersistFingerprints_Error_Default_Text),
                    SarifPropertyName.Fingerprints);
            }
        }
    }
}
