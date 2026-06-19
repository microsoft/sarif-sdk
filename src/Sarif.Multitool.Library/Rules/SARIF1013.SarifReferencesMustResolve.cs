// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.Json.Pointer;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class SarifReferencesMustResolve : SarifValidationSkimmerBase
    {
        public SarifReferencesMustResolve()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// SARIF1013
        /// </summary>
        public override string Id => RuleId.SarifReferencesMustResolve;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1013_SarifReferencesMustResolve_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.SARIF1013_SarifReferencesMustResolve_Error_Default_Text)
        };

        protected override void Analyze(ArtifactLocation artifactLocation, string artifactLocationPointer)
        {
            string uri = artifactLocation.Uri?.OriginalString;

            if (string.IsNullOrEmpty(uri) || !uri.StartsWith("sarif:", StringComparison.Ordinal))
            {
                return;
            }

            string jsonPointerString = uri.Substring("sarif:".Length);

            try
            {
                var jsonPointer = new JsonPointer(jsonPointerString);
                JToken resolved = jsonPointer.Evaluate(Context.InputLogToken);
                if (resolved == null)
                {
                    LogResult(
                        artifactLocationPointer,
                        nameof(RuleResources.SARIF1013_SarifReferencesMustResolve_Error_Default_Text),
                        uri);
                }
            }
            catch (ArgumentException)
            {
                // A malformed or unresolvable JSON pointer is the violation this rule
                // reports; JsonPointer surfaces both as ArgumentException. Anything else
                // is an unexpected fault and propagates to the analysis engine's logging
                // handler rather than being masked here.
                LogResult(
                    artifactLocationPointer,
                    nameof(RuleResources.SARIF1013_SarifReferencesMustResolve_Error_Default_Text),
                    uri);
            }
        }
    }
}
