// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UriMustBeAbsolute : SarifValidationSkimmerBase
    {
        private Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1015_UriMustBeAbsolute
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1015
        /// </summary>
        public override string Id => RuleId.UriMustBeAbsolute;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1015_Default)
        };

        protected override void Analyze(SarifLog log, string logPointer)
        {
            AnalyzeUri(log.SchemaUri, logPointer.AtProperty(SarifPropertyName.Schema));
        }

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.OriginalUriBaseIds != null)
            {
                string originalUriBaseIdsPointer = runPointer.AtProperty(SarifPropertyName.OriginalUriBaseIds);

                foreach (string key in run.OriginalUriBaseIds.Keys)
                {
                    AnalyzeUri(run.OriginalUriBaseIds[key], originalUriBaseIdsPointer.AtProperty(key));
                }
            }
        }

        protected override void Analyze(Tool tool, string toolPointer)
        {
            AnalyzeUri(tool.DownloadUri, toolPointer.AtProperty(SarifPropertyName.DownloadUri));
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
                // Ok, it's a well-formed URI of some kind. But if it's not absolute, _now_ we
                // can report it.
                Uri uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    LogResult(pointer, nameof(RuleResources.SARIF1015_Default), uriString);
                }
            }
        }
    }
}
