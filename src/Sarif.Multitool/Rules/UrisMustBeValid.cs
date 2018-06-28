// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UrisMustBeValid : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF1003_UrisMustBeValid;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1003
        /// </summary>
        public override string Id => RuleId.UrisMustBeValid;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF1003_Default)
                };
            }
        }

        protected override void Analyze(FileChange fileChange, string fileChangePointer)
        {
            AnalyzeUri(fileChange.Uri, fileChangePointer);
        }

        protected override void Analyze(FileData fileData, string fileKey, string filePointer)
        {
            // Check the property name, which must be a valid URI.
            // We can't use AnalyzeUri for this because that method appends "/uri"
            // to the JSON pointer, whereas here, the JSON pointer we have in
            // hand (filePointer) already points right to the property we are
            // examining.
            string fileUriReference = fileKey.UnescapeJsonPointer();
            try
            {
                Uri fileUri = new Uri(fileUriReference);
            }
            catch
            {
                LogResult(
                    filePointer,
                    nameof(RuleResources.SARIF1003_Default),
                    fileUriReference);
            }

            // Then check the "uri" property, if any, of the property value.
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

        protected override void Analyze(Rule rule, string rulePointer)
        {
            AnalyzeUri(rule.HelpUri, rulePointer, "helpUri");
        }

        private void AnalyzeUri(
            Uri uri,
            string parentPointer,
            string childPropertyName = SarifPropertyName.Uri)
        {
            if (uri != null)
            {
                try
                {
                    Uri fileUri = new Uri(uri.OriginalString);
                }
                catch
                {
                    string uriPointer = parentPointer.AtProperty(childPropertyName);

                    LogResult(
                        uriPointer,
                        nameof(RuleResources.SARIF1003_Default),
                        uri.OriginalString);
                }
            }
        }
    }
}
