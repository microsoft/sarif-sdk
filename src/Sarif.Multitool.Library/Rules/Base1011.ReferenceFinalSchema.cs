﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseReferenceFinalSchema : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        /// <summary>
        /// The '$schema' property must refer to the final version of the SARIF 2.1.0 schema. This
        /// enables IDEs to provide Intellisense for SARIF log files.
        ///
        /// The SARIF standard was developed over several years, and many intermediate versions of
        /// the schema were produced. Now that the standard is final, only the OASIS standard version
        /// of the schema is valid.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        private readonly List<string> _baseMessageResourceNames = new List<string>
        {
            nameof(RuleResources.Base1011_ReferenceFinalSchema_Error_Default_Text)
        };

        protected ICollection<string> BaseMessageResourceNames => _baseMessageResourceNames;

        protected override void Analyze(SarifLog log, string logPointer)
        {
            AnalyzeSchema(log.SchemaUri, logPointer.AtProperty(SarifPropertyName.Schema));
        }

        private void AnalyzeSchema(Uri schemaUri, string schema)
        {
            if (schemaUri == null)
            {
                // If SchemaUri is not present, it will be caught by Base2008 rule.
                // ToDo: Base2008 rule is deleted.
                return;
            }

            if (!schemaUri.OriginalString.EndsWith(VersionConstants.StableSarifVersion)
                && !schemaUri.OriginalString.EndsWith($"{VersionConstants.StableSarifVersion}.json")
                && !schemaUri.OriginalString.EndsWith(VersionConstants.SchemaVersionAsPublishedToSchemaStoreOrg)
                && !schemaUri.OriginalString.EndsWith($"{VersionConstants.SchemaVersionAsPublishedToSchemaStoreOrg}.json"))
            {
                // {0}: The '$schema' property value '{1}' does not refer to the final version of the SARIF
                // 2.1.0 schema. If you are using an earlier version of the SARIF format, consider upgrading
                // your analysis tool to produce the final version. If this file does in fact conform to the
                // final version of the schema, upgrade the tool to populate the '$schema' property with a URL
                // that refers to the final version of the schema.
                LogResult(
                    schema,
                    nameof(RuleResources.Base1011_ReferenceFinalSchema_Error_Default_Text),
                    schemaUri.OriginalString);
            }
        }
    }
}
