// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;
using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ReferenceFinalSchema : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1011_ReferenceFinalSchema_FullDescription_Text
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.ReferenceFinalSchema;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1011_ReferenceToOldSchemaVersion),
            nameof(RuleResources.SARIF1011_SchemaReferenceMissing)
        };

        protected override void Analyze(SarifLog log, string logPointer)
        {
            AnalyzeSchema(log.SchemaUri, logPointer);
        }

        private void AnalyzeSchema(Uri schemaUri, string pointer)
        {
            if (schemaUri == null)
            {
                LogResult(pointer, nameof(RuleResources.SARIF1011_SchemaReferenceMissing));
                return;
            }

            if (!schemaUri.OriginalString.EndsWith(VersionConstants.StableSarifVersion)
                && !schemaUri.OriginalString.EndsWith($"{VersionConstants.StableSarifVersion}.json")
                && !schemaUri.OriginalString.EndsWith(VersionConstants.SchemaVersionAsPublishedToSchemaStoreOrg)
                && !schemaUri.OriginalString.EndsWith($"{VersionConstants.SchemaVersionAsPublishedToSchemaStoreOrg}.json"))
            {
                LogResult(pointer.AtProperty(SarifPropertyName.Schema), nameof(RuleResources.SARIF1011_ReferenceToOldSchemaVersion), schemaUri.OriginalString);
                return;
            }
        }
    }
}
