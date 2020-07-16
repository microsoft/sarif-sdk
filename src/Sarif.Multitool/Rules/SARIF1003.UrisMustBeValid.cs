// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UrisMustBeValid : SarifValidationSkimmerBase
    {
        public UrisMustBeValid() : base(
            RuleId.UrisMustBeValid, 
            RuleResources.SARIF1003_UrisMustBeValid, 
            FailureLevel.Error,
            new string[] { nameof(RuleResources.SARIF1003_Default) } )
        { }

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

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.OriginalUriBaseIds != null)
            {
                string originalUriBaseIdsPointer = runPointer.AtProperty(SarifPropertyName.OriginalUriBaseIds);

                foreach (string key in run.OriginalUriBaseIds.Keys)
                {
                    AnalyzeUri(run.OriginalUriBaseIds[key].Uri, originalUriBaseIdsPointer.AtProperty(key).AtProperty(SarifPropertyName.Uri));
                }
            }
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

        private void AnalyzeUri(string uri, string pointer)
        {
            if (uri != null)
            {
                if (!Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute))
                {
                    LogResult(pointer, nameof(RuleResources.SARIF1003_Default), uri);
                }
            }
        }
    }
}
