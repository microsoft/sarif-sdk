// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class UriBaseIdRequiresRelativeUri : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF014_UriBaseIdRequiresRelativeUri;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF014
        /// </summary>
        public override string Id => RuleId.UriBaseIdRequiresRelativeUri;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF014_Default)
                };
            }
        }

        protected override void Analyze(
            FileChange fileChange,
            string fileChangePointer)
        {
            EnsureRelativeUriWithUriBaseId(
                fileChange.Uri,
                fileChange.UriBaseId,
                fileChangePointer);
        }

        protected override void Analyze(
            PhysicalLocation physicalLocation,
            string physicalLocationPointer)
        {
            EnsureRelativeUriWithUriBaseId(
                physicalLocation.Uri,
                physicalLocation.UriBaseId,
                physicalLocationPointer);
        }

        protected override void Analyze(StackFrame frame, string framePointer)
        {
            EnsureRelativeUriWithUriBaseId(
                frame.Uri,
                frame.UriBaseId,
                framePointer);
        }

        private void EnsureRelativeUriWithUriBaseId(
            Uri uri,
            string uriBaseId,
            string objectPointer)
        {
            if (uriBaseId != null && uri.IsAbsoluteUri)
            {
                string uriPointer = objectPointer.AtProperty(SarifPropertyName.Uri);

                LogResult(
                    uriPointer,
                    nameof(RuleResources.SARIF014_Default),
                    uri.OriginalString);
            }
        }
    }
}
