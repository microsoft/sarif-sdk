// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UrisShouldBeReachable : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2006
        /// </summary>
        public override string Id => RuleId.UrisShouldBeReachable;

        /// <summary>
        /// URIs that refer to locations such as rule help pages and result-related work items
        /// should be reachable via an HTTP GET request.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2006_UrisShouldBeReachable_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2006_UrisShouldBeReachable_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        private static readonly HttpClient s_httpClient = new HttpClient();
        private static readonly Dictionary<string, bool> s_checkedUris = new Dictionary<string, bool>();

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
            // If it's not a well-formed URI, or if it's not absolute, then don't bother triggering this rule.
            // If it's not well-formed, SARIF1003.UrisMustBeValid will catch it, and if it's not absolute,
            // SARIF1005.UriMustBeAbsolute will catch it.
            //
            // Check for well-formedness first, before attempting to create a Uri object, to
            // avoid having to do a try/catch. Unfortunately Uri.TryCreate will return true
            // even for a malformed URI string.
            if (uriString != null && Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
            {
                // Ok, it's a well-formed absolute URI. If it's not reachable, _now_ we can report it.
                Uri uri = new Uri(uriString, UriKind.Absolute);
                if (!IsUriReachable(uri.AbsoluteUri))
                {
                    // {0}: The URI '{1}' was not reachable via an HTTP GET request.
                    LogResult(
                        pointer, 
                        nameof(RuleResources.SARIF2006_UrisShouldBeReachable_Note_Default_Text), 
                        uriString);
                }
            }
        }

        private bool IsUriReachable(string uriString)
        {
            if (s_checkedUris.ContainsKey(uriString))
            {
                return s_checkedUris[uriString];
            }

            HttpResponseMessage httpResponseMessage = s_httpClient.GetAsync(uriString).GetAwaiter().GetResult();
            s_checkedUris.Add(uriString, httpResponseMessage.IsSuccessStatusCode);
            return httpResponseMessage.IsSuccessStatusCode;
        }
    }
}
