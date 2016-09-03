// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class UseAbsolutePathsForNestedFileUriFragments : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF002_UseAbsolutePathsForNestedFileUriFragmentsDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF002
        /// </summary>
        public override string Id => RuleId.UseAbsolutePathsForNestedFileUriFragments;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF002_Default)
                };
            }
        }

        protected override void Analyze(FileChange fileChange, string fileChangePointer)
        {
            AnalyzeUri(fileChange.Uri, fileChangePointer);
        }

        protected override void Analyze(FileData fileData, string fileKey, string filePointer)
        {
            Uri fileUri;
            try
            {
                fileUri = new Uri(fileKey);
            }
            catch (UriFormatException)
            {
                // It wasn't a value URI. Rule SARIF003, UrisMustBeValid, will catch this problem.
                return;
            }

            if (UriHasNonAbsoluteFragment(fileUri))
            {
                LogResult(
                    filePointer,
                    nameof(RuleResources.SARIF002_Default),
                    fileUri.OriginalString);
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
                    uriPointer,
                    nameof(RuleResources.SARIF002_Default),
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
