// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;
using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DirectoryUriMustEndWithSlash : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1019_DirectoryUriMustEndWithSlash
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1019
        /// </summary>
        public override string Id => RuleId.DirectoryUriMustEndWithSlash;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1019_Default)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.OriginalUriBaseIds != null)
            {
                string originalUriBaseIdsPointer = runPointer.AtProperty(SarifPropertyName.OriginalUriBaseIds);

                foreach (string key in run.OriginalUriBaseIds.Keys)
                {
                    AnalyzeOriginalUriBaseIdsEntry(key, run.OriginalUriBaseIds[key], originalUriBaseIdsPointer.AtProperty(key));
                }
            }
        }

        private void AnalyzeOriginalUriBaseIdsEntry(string originalUriBaseId, ArtifactLocation artifactLocation, string pointer)
        {
            string uriString = artifactLocation?.Uri?.OriginalString;

            // If it's not a well-formed URI, don't bother triggering this rule. Rule SARIF1003,
            // UrisMustBeValid, will catch it.
            //
            // Check for well-formedness first, before attempting to create a Uri object, to
            // avoid having to do a try/catch. Unfortunately Uri.TryCreate will return true
            // even for a malformed URI string.
            if (uriString != null && Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute))
            {
                // Ok, it's a well-formed URI. If it doesn't end with a slash, _now_ we can report it.

                // CAUTION: Do not change this test to 'uriString?.EndsWith("/") != true'.
                // That would fire if uriString were null. We only want to fire if it is non-null
                // but does not end with a slash.
                if (uriString != null && !uriString.EndsWith("/"))
                {
                    LogResult(pointer, nameof(RuleResources.SARIF1019_Default), uriString, originalUriBaseId);
                }
            }
        }
    }
}
