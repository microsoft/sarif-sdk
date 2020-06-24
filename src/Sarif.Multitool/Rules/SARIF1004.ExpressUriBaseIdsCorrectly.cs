// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ExpressUriBaseIdsCorrectly : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1004
        /// </summary>
        public override string Id => RuleId.ExpressUriBaseIdsCorrectly;

        /// <summary>
        /// 
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
                    nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdRequiresRelativeUri_Text),
                    nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_TopLevelUriBaseIdMustBeAbsolute_Text),
                    nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustEndWithSlash_Text)
                };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(ArtifactLocation fileLocation, string fileLocationPointer)
        {
            // UriBaseIdRequiresRelativeUri: The 'uri' property of 'fileLocation' must be a relative uri, since 'uriBaseId' is present.
            if (fileLocation.UriBaseId != null && fileLocation.Uri.IsAbsoluteUri)
            {
                // {0}: This fileLocation object contains a "uriBaseId" property, which means that the value
                // of the "uri" property must be a relative URI reference, but "{1}" is an absolute URI reference.
                LogResult(
                    fileLocationPointer.AtProperty(SarifPropertyName.Uri),
                    nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdRequiresRelativeUri_Text),
                    fileLocation.Uri.OriginalString);
            }
        }

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.OriginalUriBaseIds != null)
            {
                string originalUriBaseIdsPointer = runPointer.AtProperty(SarifPropertyName.OriginalUriBaseIds);

                foreach (string uriBaseId in run.OriginalUriBaseIds.Keys)
                {
                    AnalyzeOriginalUriBaseIdsEntry(uriBaseId, run.OriginalUriBaseIds[uriBaseId], originalUriBaseIdsPointer.AtProperty(uriBaseId));
                }
            }
        }

        private void AnalyzeOriginalUriBaseIdsEntry(string uriBaseId, ArtifactLocation artifactLocation, string pointer)
        {
            if (artifactLocation.Uri == null) { return; }

            // If it's not a well-formed URI of _any_ kind, then don't bother triggering this rule.
            // Rule SARIF1003, UrisMustBeValid, will catch it.
            // Check for well-formedness first, before attempting to create a Uri object, to
            // avoid having to do a try/catch. Unfortunately Uri.TryCreate will return true
            // even for a malformed URI string.
            string uriString = artifactLocation.Uri.OriginalString;
            if (uriString != null && Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute))
            {
                var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

                // TopLevelUriBaseIdMustBeAbsolute: TODO
                if (artifactLocation.UriBaseId == null && !uri.IsAbsoluteUri)
                {
                    // {0}: The URI '{1}' belonging to the '{2}' element of run.originalUriBaseIds is not an absolute URI.
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_TopLevelUriBaseIdMustBeAbsolute_Text),
                        uriString,
                        uriBaseId);
                }

                // UriBaseIdValueMustEndWithSlash: TODO
                if (!uriString.EndsWith("/"))
                {
                    // {0}: The URI '{1}' belonging to the '{2}' element of run.originalUriBaseIds does not end with a slash.
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustEndWithSlash_Text),
                        uriString,
                        uriBaseId);
                }
            }
        }
    }
}
