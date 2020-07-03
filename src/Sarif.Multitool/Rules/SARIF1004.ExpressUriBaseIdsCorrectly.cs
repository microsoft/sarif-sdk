﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Placeholder_SARIF1004_ExpressUriBaseIdsCorrectly_FullDescription_Text
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdRequiresRelativeUri_Text),
            nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_TopLevelUriBaseIdMustBeAbsolute_Text),
            nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustEndWithSlash_Text),
            nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustNotContainDotDotSegment_Text),
            nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustNotContainQueryOrFragment_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(ArtifactLocation fileLocation, string fileLocationPointer)
        {
            // UriBaseIdRequiresRelativeUri: The 'uri' property of 'fileLocation' must be a relative uri, since 'uriBaseId' is present.
            if (fileLocation.UriBaseId != null && fileLocation.Uri.IsAbsoluteUri)
            {
                //{0}: {1} Placeholder_SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdRequiresRelativeUri_Text
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

                if (artifactLocation.UriBaseId == null && !uri.IsAbsoluteUri)
                {
                    // {0}: The '{1}' element of 'originalUriBaseIds' has no 'uriBaseId' property, but its 'uri'
                    // property '{2}' is not an absolute URI. According to the SARIF specification, every such
                    // "top-level" entry in 'originalUriBaseIds' must specify an absolute URI, because the purpose
                    // of 'originalUriBaseIds' is to enable the resolution of relative references to absolute URIs.
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_TopLevelUriBaseIdMustBeAbsolute_Text),
                        uriString,
                        uriBaseId);
                }

                if (!uriString.EndsWith("/"))
                {
                    // {0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that does not
                    // end with a slash. The trailing slash is required to minimize the likelihood of an error
                    // when concatenating URI segments together.
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustEndWithSlash_Text),
                        uriString,
                        uriBaseId);
                }

                if (uriString.Split('/').Any(x => x.Equals("..")))
                {
                    // {0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that contains
                    // a '..' segment. This is dangerous because if symbolic links are present, '..' might have
                    // different meanings on the machine that produced the log file and the machine where an end
                    // user or a tool consumes it.
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustNotContainDotDotSegment_Text),
                        uriString,
                        uriBaseId);
                }

                if (uri.IsAbsoluteUri && (!string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.Query)))
                {
                    // {0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that contains a
                    // query or a fragment. This is not valid because the purpose of the 'uriBaseId' property is
                    // to help resolve a relative reference to an absolute URI by concatenating the relative
                    // reference to the absolute base URI. This won't work if the base URI contains a query or a
                    // fragment.
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1004_ExpressUriBaseIdsCorrectly_Error_UriBaseIdValueMustNotContainQueryOrFragment_Text),
                        uriString,
                        uriBaseId);
                }
            }
        }
    }
}
