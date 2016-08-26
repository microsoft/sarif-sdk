// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class UseAbsolutePathsForNestedFileUriFragments : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SV0002_UseAbsolutePathsForNestedFileUriFragmentsDescription;

        /// <summary>
        /// SV0002
        /// </summary>
        public override string Id => RuleId.UseAbsolutePathsForNestedFileUriFragments;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SV0002_DefaultFormatId)
                };
            }
        }

        protected override void Analyze(FileChange fileChange, string fileChangePointer)
        {
            AnalyzeUri(fileChange.Uri, fileChangePointer);
        }

        protected override void Analyze(FileData fileData, string fileKey, string filePointer)
        {
            try
            {
                Uri fileUri = new Uri(fileKey);
                if (UriHasNonAbsoluteFragment(fileUri))
                {
                    LogResult(
                        ResultLevel.Warning,
                        filePointer,
                        nameof(RuleResources.SV0002_DefaultFormatId),
                        fileUri.OriginalString);
                }
            }
            catch
            {
                // It wasn't a value URI. TODO: implement another rule to check that.
            }

            AnalyzeUri(fileData.Uri, filePointer);
        }

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            AnalyzeUri(physicalLocation.Uri, physicalLocationPointer);
        }

        protected override void Analyze(StackFrame frame, string framePointer)
        {
            AnalyzeUri(frame.Uri, framePointer);
        }

        private void AnalyzeUri(Uri uri, string parentPointer)
        {
            if (UriHasNonAbsoluteFragment(uri))
            {
                string uriPointer = parentPointer.AtProperty(SarifPropertyName.Uri);

                LogResult(
                    ResultLevel.Warning,
                    uriPointer,
                    nameof(RuleResources.SV0002_DefaultFormatId),
                    uri.OriginalString);
            }
        }

        private bool UriHasNonAbsoluteFragment(Uri uri)
        {
            if (uri == null)
            {
                return false;
            }

            // You can't access the Fragment property of a relative URI, so if this URI is
            // relative, turn it into a fake absolute URI, and get the fragment from that.
            Uri absoluteUri = uri.IsAbsoluteUri
                ? uri
                : MakeFakeAbsoluteUri(uri);

            string fragment = absoluteUri.Fragment;

            return !string.IsNullOrEmpty(fragment) && !fragment.StartsWith("#/", StringComparison.Ordinal);
        }

        private static readonly Uri _fakeBaseUri = new Uri("file:///root", UriKind.Absolute);

        private Uri MakeFakeAbsoluteUri(Uri relativeUri)
        {
            return new Uri(_fakeBaseUri, relativeUri);
        }
    }
}
