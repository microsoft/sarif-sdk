// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriMustBeAbsolute : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1005
        /// </summary>
        public override string Id => RuleId.UriMustBeAbsolute;

        /// <summary>
        /// Certain URIs are required to be absolute. For the most part, these are URIs that refer to http
        /// addresses, such as work items or rule help topics.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1005_UriMustBeAbsolute_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1005_UriMustBeAbsolute_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(SarifLog log, string logPointer)
        {
            AnalyzeUri(log.SchemaUri, logPointer.AtProperty(SarifPropertyName.Schema));
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.WorkItemUris != null)
            {
                Uri[] workItemUris = result.WorkItemUris.ToArray();
                string workItemUrisPointer = resultPointer.AtProperty(SarifPropertyName.WorkItemUris);

                for (int i = 0; i < workItemUris.Length; ++i)
                {
                    AnalyzeUri(workItemUris[i], workItemUrisPointer.AtIndex(i));
                }
            }
        }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            AnalyzeUri(reportingDescriptor.HelpUri, reportingDescriptorPointer.AtProperty(SarifPropertyName.HelpUri));
        }

        protected override void Analyze(ToolComponent toolComponent, string toolComponentPointer)
        {
            AnalyzeUri(toolComponent.DownloadUri, toolComponentPointer.AtProperty(SarifPropertyName.DownloadUri));
        }

        protected override void Analyze(VersionControlDetails versionControlDetails, string versionControlDetailsPointer)
        {
            AnalyzeUri(versionControlDetails.RepositoryUri, versionControlDetailsPointer.AtProperty(SarifPropertyName.RepositoryUri));
        }

        private void AnalyzeUri(Uri uri, string pointer)
        {
            AnalyzeUri(uri?.OriginalString, pointer);
        }

        private void AnalyzeUri(string uriString, string pointer)
        {
            // If it's not a well-formed URI of _any_ kind, then don't bother triggering this rule.
            // Rule SARIF1003, UrisMustBeValid, will catch it.
            // Check for well-formedness first, before attempting to create a Uri object, to
            // avoid having to do a try/catch. Unfortunately Uri.TryCreate will return true
            // even for a malformed URI string.
            if (uriString != null && Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute))
            {
                // Ok, it's a well-formed URI of some kind. If it's not absolute, _now_ we
                // can report it.
                Uri uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    // {0}: The value of this property is required to be an absolute URI, but '{1}' is a relative URI reference.
                    LogResult(
                        pointer, 
                        nameof(RuleResources.SARIF1005_UriMustBeAbsolute_Error_Default_Text), 
                        uriString);
                }
            }
        }
    }
}
