// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;

namespace Test.UnitTests.Sarif.Mcp.Server.Fixtures
{
    /// <summary>
    /// Validates SARIF text against the published SARIF 2.1.0 schema
    /// (<c>sarif-2.1.0-rtm.6.json</c>, the version pinned by the SDK's
    /// <c>SchemaVersionAsPublishedToSchemaStoreOrg</c> property). The schema
    /// is a strictly tighter contract than the SDK's <c>SarifLog.Load</c>:
    /// load is permissive (extra properties, missing $schema, etc.), the
    /// published schema is the bar downstream SARIF consumers actually enforce.
    /// </summary>
    public sealed class SchemaValidator
    {
        public const string SchemaFileName = "sarif-2.1.0-rtm.6.json";

        private readonly JsonSchema _schema;

        public SchemaValidator()
        {
            string schemaPath = Path.Combine(AppContext.BaseDirectory, SchemaFileName);

            if (!File.Exists(schemaPath))
            {
                throw new InvalidOperationException(
                    $"SARIF schema file '{SchemaFileName}' not present alongside the test binaries " +
                    $"at '{AppContext.BaseDirectory}'. The test project's csproj must copy " +
                    "..\\Sarif\\Schemata\\sarif-2.1.0-rtm.6.json to output (PreserveNewest).");
            }

            string schemaText = File.ReadAllText(schemaPath);
            this._schema = SchemaReader.ReadSchema(schemaText, SchemaFileName);
        }

        public void Validate(string sarifJsonText, string instanceLabel)
        {
            var validator = new Validator(this._schema);
            Result[] errors = validator.Validate(sarifJsonText, instanceLabel);

            errors.Should().BeEmpty(
                "produced SARIF must validate against the published 2.1.0 schema; errors:\n  "
                    + string.Join(
                        "\n  ",
                        errors.Select(e => $"{e.RuleId}: {e.Message?.Text} (at {e.Locations?.FirstOrDefault()?.PhysicalLocation?.Region?.StartLine})")));
        }

        public void ValidateFile(string sarifFilePath) =>
            this.Validate(File.ReadAllText(sarifFilePath), sarifFilePath);
    }
}

