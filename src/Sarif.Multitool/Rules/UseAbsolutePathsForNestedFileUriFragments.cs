// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UseAbsolutePathsForNestedFileUriFragments : SarifValidationSkimmerBase
    {
        private Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1002_UseAbsolutePathsForNestedFileUriFragmentsDescription
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1002
        /// </summary>
        public override string Id => RuleId.UseAbsolutePathsForNestedFileUriFragments;

        protected override IEnumerable<string> MessageResourceNames
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF1002_Default)
                };
            }
        }

        protected override void Analyze(FileLocation fileLocation, string fileLocationPointer)
        {
            AnalyzeUri(fileLocation.Uri, fileLocationPointer.AtProperty(SarifPropertyName.Uri));
        }

        // In addition to appearing in fileLocation objects, URIs with fragments might
        // appear as property names in the run.files dictionary.
        protected override void Analyze(FileData fileData, string fileKey, string filePointer)
        {
            if (!Uri.IsWellFormedUriString(fileKey, UriKind.RelativeOrAbsolute))
            {
                // It wasn't a value URI. Rule SARIF1003, UrisMustBeValid, will catch this problem.
                return;
            }

            Uri fileKeyUri = new Uri(fileKey, UriKind.RelativeOrAbsolute);
            AnalyzeUri(fileKeyUri, filePointer);
        }

        private void AnalyzeUri(Uri uri, string pointer)
        {
            if (UriHasNonAbsoluteFragment(uri))
            {
                LogResult(pointer, nameof(RuleResources.SARIF1002_Default), uri.OriginalString);
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
