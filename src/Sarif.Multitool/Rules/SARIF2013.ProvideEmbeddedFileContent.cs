// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideEmbeddedFileContent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2013
        /// </summary>
        public override string Id => RuleId.ProvideEmbeddedFileContent;

        /// <summary>
        /// Provide embedded file content so that users can examine results in their full context
        /// without having to enlist in the source repository. Embedding file content in a SARIF
        /// log file can dramatically increase its size, so consider the usage scenario when you
        /// decide whether to provide it.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2013_ProvideEmbeddedFileContent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2013_ProvideEmbeddedFileContent_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Artifacts != null && run.Artifacts.All(artifact => artifact.Contents == null))
            {
                // {0}: This run does not provide embedded file content. Providing embedded file
                // content enables users to examine results in their full context without having
                // to enlist in the source repository. Embedding file content in a SARIF log file
                // can dramatically increase its size, so consider the usage scenario when you
                // decide whether to provide it.
                LogResult(
                    runPointer,
                    nameof(RuleResources.SARIF2013_ProvideEmbeddedFileContent_Note_Default_Text));
            }
        }
    }
}
