// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Json.Schema;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Shared schema-evaluation surface for ai-sarif-log.schema.json. Centralizes the single
    // offline registration of the SARIF 2.1.0 base identity (so sibling test classes do
    // not double-register it in SchemaRegistry.Global) and exposes the whole-log accept
    // verdict plus the canonical JsonObject builders the schema tests assert over.
    //
    // It evaluates with JsonSchema.Net (json-everything), NOT Microsoft.Json.Schema, for
    // the Draft 2020-12 support ai-sarif-log.schema.json requires, and resolves the base $ref
    // against the vendored sarif-2.1.0.json copied next to the test assembly so validation
    // never reaches the network.
    internal static class AiLogSchemaFixture
    {
        public const string SarifSchemaIdentity = "https://json.schemastore.org/sarif-2.1.0.json";
        public const string Guid = "12345678-1234-1234-1234-1234567890ab";

        public static readonly JsonSchema Schema = LoadAiSchema("ai-sarif-log.schema.json");
        public static readonly EvaluationOptions Options = BuildOptions();

        private static readonly EvaluationOptions s_explainOptions =
            new EvaluationOptions { OutputFormat = OutputFormat.List };

        // True when ai-sarif-log.schema.json accepts the supplied whole log.
        public static bool Accepts(JsonNode log)
        {
            JsonElement element = JsonSerializer.SerializeToElement(log);
            return Schema.Evaluate(element, Options).IsValid;
        }

        // A human-readable account of why a log was rejected, for assertion messages.
        public static string Explain(JsonNode log)
        {
            JsonElement element = JsonSerializer.SerializeToElement(log);
            EvaluationResults results = Schema.Evaluate(element, s_explainOptions);
            if (results.IsValid) { return "(accepted)"; }

            var sb = new StringBuilder();
            AppendErrors(results, sb);
            foreach (EvaluationResults detail in results.Details)
            {
                AppendErrors(detail, sb);
            }
            return sb.Length == 0 ? "(rejected)" : sb.ToString();
        }

        private static void AppendErrors(EvaluationResults node, StringBuilder sb)
        {
            if (node.Errors != null && node.Errors.Count > 0)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, string> error in node.Errors)
                {
                    sb.AppendLine($"  {node.InstanceLocation}: {error.Key} -> {error.Value}");
                }
            }
        }

        // ---- Canonical whole-log builders -------------------------------------------------

        public static JsonObject FinalizedResult(string ruleId)
        {
            return new JsonObject
            {
                ["ruleId"] = ruleId,
                ["ruleIndex"] = 0,
                ["guid"] = Guid,
                ["message"] = new JsonObject { ["text"] = "A finding.", ["markdown"] = "**A finding.**" },
                ["locations"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["physicalLocation"] = new JsonObject
                        {
                            ["artifactLocation"] = new JsonObject { ["uri"] = "src/app.cs", ["index"] = 0 },
                            ["region"] = new JsonObject { ["startLine"] = 1 }
                        }
                    }
                }
            };
        }

        public static JsonObject Run()
        {
            return new JsonObject
            {
                ["tool"] = new JsonObject { ["driver"] = new JsonObject { ["name"] = "MyScanner" } },
                ["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "https://github.com/o/r" } },
                ["properties"] = new JsonObject { ["ai/origin"] = "generated" },
                ["results"] = new JsonArray { FinalizedResult("CWE-89/clean") }
            };
        }

        public static JsonObject RunWithResult(JsonObject result)
        {
            JsonObject run = Run();
            run["results"] = new JsonArray { result };
            return run;
        }

        public static JsonObject Log(JsonObject run)
        {
            return new JsonObject
            {
                ["$schema"] = SarifSchemaIdentity,
                ["version"] = "2.1.0",
                ["runs"] = new JsonArray { run }
            };
        }

        public static JsonObject Log(params JsonObject[] runs)
        {
            var array = new JsonArray();
            foreach (JsonObject run in runs) { array.Add(run); }
            return new JsonObject
            {
                ["$schema"] = SarifSchemaIdentity,
                ["version"] = "2.1.0",
                ["runs"] = array
            };
        }

        private static JsonSchema LoadAiSchema(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "GetSchema", fileName);
            return JsonSchema.FromText(File.ReadAllText(path));
        }

        private static EvaluationOptions BuildOptions()
        {
            string sarifPath = Path.Combine(AppContext.BaseDirectory, "GetSchema", "sarif-2.1.0.json");
            JsonSchema sarif = JsonSchema.FromText(File.ReadAllText(sarifPath));
            SchemaRegistry.Global.Register(new Uri(SarifSchemaIdentity), sarif);

            return new EvaluationOptions();
        }
    }
}
