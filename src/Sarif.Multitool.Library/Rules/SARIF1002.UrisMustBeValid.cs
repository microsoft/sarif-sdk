// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UrisMustBeValid : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1002
        /// </summary>
        public override string Id => RuleId.UrisMustBeValid;

        /// <summary>
        /// Specify a valid URI reference for every URI-valued property.
        ///
        /// URIs must conform to [RFC 3986](https://tools.ietf.org/html/rfc3986). In addition,
        /// 'file' URIs must not include '..' segments. If symbolic links are present, '..'
        /// might have different meanings on the machine that produced the log file and the
        /// machine where an end user or a tool consumes it.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1002_UrisMustBeValid_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1002_UrisMustBeValid_Error_UrisMustConformToRfc3986_Text),
            nameof(RuleResources.SARIF1002_UrisMustBeValid_Error_FileUrisMustNotIncludeDotDotSegments_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(SarifLog log, string logPointer)
        {
            AnalyzeUri(log.SchemaUri, logPointer.AtProperty(SarifPropertyName.Schema));
        }

        protected override void Analyze(ArtifactLocation fileLocation, string fileLocationPointer)
        {
            AnalyzeUri(fileLocation.Uri, fileLocationPointer.AtProperty(SarifPropertyName.Uri));
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

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string messageDescriptorPointer)
        {
            AnalyzeUri(reportingDescriptor.HelpUri, messageDescriptorPointer.AtProperty(SarifPropertyName.HelpUri));
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
            string uriString = uri?.OriginalString;
            if (uriString != null)
            {
                if (!Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute))
                {
                    // {0}: The string '{1}' is not a valid URI reference. URIs must conform to
                    // [RFC 3986](https://tools.ietf.org/html/rfc3986).
                    LogResult(
                        pointer,
                        nameof(RuleResources.SARIF1002_UrisMustBeValid_Error_UrisMustConformToRfc3986_Text),
                        uriString);
                }

                if (uri.IsAbsoluteUri && uri.IsFile)
                {
                    if (uriString.Split('/').Any(x => x.Equals("..")))
                    {
                        // {0}: The 'file' URI '{1}' contains a '..' segment. This is dangerous because
                        // if symbolic links are present, '..' might have different meanings on the
                        // machine that produced the log file and the machine where an end user or
                        // a tool consumes it.
                        LogResult(
                            pointer,
                            nameof(RuleResources.SARIF1002_UrisMustBeValid_Error_FileUrisMustNotIncludeDotDotSegments_Text),
                            uriString);
                    }
                }
            }
        }
    }
}
