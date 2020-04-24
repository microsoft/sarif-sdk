// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class InvalidUriInOriginalUriBaseIds : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1018_InvalidUriInOriginalUriBaseIds
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1018
        /// </summary>
        public override string Id => RuleId.InvalidUriInOriginalUriBaseIds;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1018_NotAbsolute),
            nameof(RuleResources.SARIF1018_LacksTrailingSlash)
        };

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

                if (artifactLocation.UriBaseId == null && !uri.IsAbsoluteUri)
                {
                    LogResult(pointer, nameof(RuleResources.SARIF1018_NotAbsolute), uriString, uriBaseId);
                }

                if (!uriString.EndsWith("/"))
                {
                    LogResult(pointer, nameof(RuleResources.SARIF1018_LacksTrailingSlash), uriString, uriBaseId);
                }
            }
        }
    }
}
