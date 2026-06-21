// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

using FluentAssertions;

using Json.Schema;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Drift guard for ai-log.schema.json — the WHOLE-LOG contract the get-schema verb
    // serves for emit-finalize (the output side, completing the verb->schema map to
    // 6 of 6). Unlike the per-object input schemas, this is a POST-enrichment overlay:
    // it $refs the canonical SARIF document and run/result shapes and tightens them to
    // the Error-level AI rules at whole-log scale while ACCEPTING the index/identity
    // state finalize assigns (result.ruleIndex, result.guid, artifactLocation.index).
    //
    // This test pins the schema's accept/reject verdicts to the C# rules so a schema
    // edit that drifts from them fails loudly. The rules it encodes, and the deliberate
    // fidelity boundary, are:
    //   - AIRuleIdConvention  : result.ruleId is a CWE sub-id or NOVEL- escape.
    //   - AI1005              : result.message.markdown present and non-whitespace.
    //   - AI1003              : a physicalLocation carries region.startLine >= 1.
    //   - AI1004 / AI1006     : per run, a versionControlProvenance entry and
    //                           properties[ai/origin] in {generated,annotated,synthesized}.
    //   - GHAzDO1020 / GHAzDO1019 : when a run carries the Azure DevOps pipeline SHAPE
    //                           (an azuredevops/pipeline/build/* property key or an
    //                           automationDetails.id bearing that prefix), the id must
    //                           start with the prefix AND all four pipeline properties
    //                           must be present and well-typed. A non-pipeline run may
    //                           carry an automationDetails with only guid/correlationGuid
    //                           and is NOT held to the GHAzDO contract — this gate is the
    //                           reconciliation between the GHAzDO profile (which fires on
    //                           automationDetails presence) and the AI profile (which lets
    //                           a run carry a bare automationDetails). The residual id
    //                           structure beyond the prefix (org/project/.../buildId) is
    //                           deferred to emit-finalize --validate.
    //
    // It validates with JsonSchema.Net (json-everything), NOT Microsoft.Json.Schema, for
    // the same Draft 2020-12 reason AIInputSchemaDriftTests documents, and resolves the
    // SARIF base $ref offline to the vendored sarif-2.1.0.json copied next to the test
    // assembly. Per repo idiom each case table is walked inside a single [Fact].
    public class AILogSchemaDriftTests
    {
        private const string SarifSchemaIdentity = "https://json.schemastore.org/sarif-2.1.0.json";
        private const string Guid = "12345678-1234-1234-1234-1234567890ab";

        private static readonly JsonSchema s_logSchema = LoadAiSchema("ai-log.schema.json");
        private static readonly EvaluationOptions s_options = BuildOptions();

        [Fact]
        public void AILogSchema_AcceptsAMinimalFinalizedLog()
        {
            Accepts(s_logSchema, MakeLog(MakeRun())).Should().BeTrue(
                "a whole log with one finalized run (named driver, a versionControlProvenance entry, " +
                "properties[ai/origin], and a results array) must validate against ai-log.schema.json");
        }

        [Fact]
        public void AILogSchema_AcceptsACleanScanWithEmptyResults()
        {
            JsonObject run = MakeRun();
            run["results"] = new JsonArray();
            Accepts(s_logSchema, MakeLog(run)).Should().BeTrue(
                "a clean scan finalizes to a run carrying an empty results array, which must validate");
        }

        [Fact]
        public void AILogSchema_RequiresVersionAndNonEmptyRuns()
        {
            var offenders = new List<string>();

            void Expect(string label, JsonObject log, bool accept)
            {
                if (Accepts(s_logSchema, log) != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}");
                }
            }

            JsonObject noVersion = MakeLog(MakeRun());
            noVersion.Remove("version");
            Expect("no-version", noVersion, false);

            JsonObject wrongVersion = MakeLog(MakeRun());
            wrongVersion["version"] = "2.0.0";
            Expect("wrong-version", wrongVersion, false);

            JsonObject noRuns = MakeLog(MakeRun());
            noRuns.Remove("runs");
            Expect("no-runs", noRuns, false);

            JsonObject emptyRuns = MakeLog(MakeRun());
            emptyRuns["runs"] = new JsonArray();
            Expect("empty-runs", emptyRuns, false);

            offenders.Should().BeEmpty(
                "ai-log.schema.json must require version == 2.1.0 and a non-empty runs array:\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AILogSchema_RequiresRunEssentials()
        {
            // AI1004 (versionControlProvenance), AI1006 (properties[ai/origin]), the
            // emit header (tool.driver.name non-whitespace), and the finalized-log
            // requirement that runs carry a results array.
            var offenders = new List<string>();

            void Expect(string label, JsonObject run, bool accept)
            {
                if (Accepts(s_logSchema, MakeLog(run)) != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}");
                }
            }

            JsonObject noTool = MakeRun();
            noTool.Remove("tool");
            Expect("no-tool", noTool, false);

            JsonObject wsName = MakeRun();
            wsName["tool"]["driver"]["name"] = "   ";
            Expect("whitespace-driver-name", wsName, false);

            JsonObject noVcp = MakeRun();
            noVcp.Remove("versionControlProvenance");
            Expect("no-vcp", noVcp, false);

            JsonObject emptyVcp = MakeRun();
            emptyVcp["versionControlProvenance"] = new JsonArray();
            Expect("empty-vcp", emptyVcp, false);

            JsonObject httpVcp = MakeRun();
            httpVcp["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "http://x/y" } };
            Expect("http-repo-uri", httpVcp, false);

            JsonObject noProps = MakeRun();
            noProps.Remove("properties");
            Expect("no-properties", noProps, false);

            JsonObject badOrigin = MakeRun();
            badOrigin["properties"] = new JsonObject { ["ai/origin"] = "invented" };
            Expect("bad-origin", badOrigin, false);

            JsonObject noResults = MakeRun();
            noResults.Remove("results");
            Expect("no-results", noResults, false);

            offenders.Should().BeEmpty(
                "ai-log.schema.json must require, per run, tool.driver.name (non-whitespace), a non-empty " +
                "versionControlProvenance with lowercase-https repositoryUri (AI1004), properties[ai/origin] " +
                "in {generated,annotated,synthesized} (AI1006), and a results array:\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AILogSchema_EnforcesResultRuleIdGrammar()
        {
            // The whole-log result-side ruleId grammar must match AIRuleIdConvention.cs:
            // a CWE sub-id 'CWE-<n>/<kebab>' or the NOVEL- escape 'NOVEL-<kebab>'.
            var offenders = new List<string>();

            // The finalized (output) ruleId grammar is a SUPERSET of the input grammar:
            // it additionally accepts the bare collapsed 'CWE-<number>' form that
            // emit-finalize writes for GitHub-hosted runs (the GitHub-hosted collapse).
            string[] accepted =
            {
                "CWE-89/kql-injection-from-config",
                "CWE-1/a",
                "CWE-89",          // bare collapsed taxonomy id (GitHub-hosted post-collapse)
                "CWE-1220",        // bare collapsed, multi-digit
                "NOVEL-prompt-injection-via-system-message",
                "NOVEL-a",
            };

            string[] rejected =
            {
                "",
                "CWE-89/",         // empty sub-id
                "CWE-89/a/b",      // slash in sub-id
                "cwe-89/foo",      // lowercase base
                "cwe-89",          // lowercase bare base
                "CWE-89/Foo",      // uppercase in sub-id
                "NOVEL",
                "NOVEL-",
                "NOVEL-foo/bar",   // NOVEL- is flat
                "NOVEL-Foo",       // uppercase in sub-id
                "CVE-2021-1/x",    // CVE not accepted (CWE-only)
            };

            foreach (string ruleId in accepted)
            {
                if (!Accepts(s_logSchema, MakeLog(MakeRunWithResult(MakeFinalizedResult(ruleId: ruleId)))))
                {
                    offenders.Add($"  expected ACCEPT, got REJECT: '{ruleId}'");
                }
            }

            foreach (string ruleId in rejected)
            {
                if (Accepts(s_logSchema, MakeLog(MakeRunWithResult(MakeFinalizedResult(ruleId: ruleId)))))
                {
                    offenders.Add($"  expected REJECT, got ACCEPT: '{ruleId}'");
                }
            }

            offenders.Should().BeEmpty(
                "ai-log.schema.json's result ruleId verdict must match AIRuleIdConvention.cs at finalize scale: " +
                "the CWE sub-id form, the NOVEL- escape, AND the bare collapsed 'CWE-<number>' form that " +
                "emit-finalize writes for GitHub-hosted runs:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AILogSchema_AcceptsFinalizeAssignedIndexAndIdentityState()
        {
            // The contrast with ai-result.schema.json: the per-object INPUT schema bounces
            // ruleIndex, guid, and artifactLocation.index (the producer must not author them);
            // the finalized whole-log schema ACCEPTS them, because finalize is what assigns them.
            var offenders = new List<string>();

            JsonObject withIdentity = MakeFinalizedResult("CWE-89/clean");
            withIdentity["ruleIndex"] = 7;
            withIdentity["guid"] = Guid;
            if (!Accepts(s_logSchema, MakeLog(MakeRunWithResult(withIdentity))))
            {
                offenders.Add("  expected ACCEPT: result carrying finalize-assigned ruleIndex + guid");
            }

            JsonObject withIndex = MakeFinalizedResult("CWE-89/clean");
            withIndex["locations"][0]["physicalLocation"]["artifactLocation"]["index"] = 3;
            if (!Accepts(s_logSchema, MakeLog(MakeRunWithResult(withIndex))))
            {
                offenders.Add("  expected ACCEPT: locations[].artifactLocation.index assigned at finalize");
            }

            JsonObject withTaxonomies = MakeRunWithResult(MakeFinalizedResult("CWE-89/clean"));
            withTaxonomies["taxonomies"] = new JsonArray
            {
                new JsonObject { ["name"] = "CWE", ["taxa"] = new JsonArray { new JsonObject { ["id"] = "89" } } }
            };
            if (!Accepts(s_logSchema, MakeLog(withTaxonomies)))
            {
                offenders.Add("  expected ACCEPT: run.taxonomies injected by the taxonomy enricher");
            }

            // A negative-or-fractional ruleIndex is still nonsense and must reject.
            JsonObject badIndex = MakeFinalizedResult("CWE-89/clean");
            badIndex["ruleIndex"] = -1;
            if (Accepts(s_logSchema, MakeLog(MakeRunWithResult(badIndex))))
            {
                offenders.Add("  expected REJECT: negative ruleIndex");
            }

            offenders.Should().BeEmpty(
                "ai-log.schema.json must ACCEPT the index/identity state finalize assigns (ruleIndex >= 0, " +
                "guid, artifactLocation.index, run.taxonomies) that the input schemas bounce:\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AILogSchema_EnforcesResultRegionAndMarkdown()
        {
            // AI1003 region constraints and AI1005 markdown still hold at whole-log scale.
            var offenders = new List<string>();

            JsonObject noMarkdown = MakeFinalizedResult("CWE-89/clean");
            noMarkdown["message"] = new JsonObject { ["text"] = "no md." };
            if (Accepts(s_logSchema, MakeLog(MakeRunWithResult(noMarkdown))))
            {
                offenders.Add("  expected REJECT: result.message without markdown (AI1005)");
            }

            JsonObject noRegion = MakeFinalizedResult("CWE-89/clean");
            noRegion["locations"] = new JsonArray
            {
                new JsonObject { ["physicalLocation"] = new JsonObject { ["artifactLocation"] = new JsonObject { ["uri"] = "a.cs" } } }
            };
            if (Accepts(s_logSchema, MakeLog(MakeRunWithResult(noRegion))))
            {
                offenders.Add("  expected REJECT: physicalLocation without region (AI1003)");
            }

            JsonObject zeroLine = MakeFinalizedResult("CWE-89/clean");
            zeroLine["locations"] = new JsonArray
            {
                new JsonObject
                {
                    ["physicalLocation"] = new JsonObject
                    {
                        ["artifactLocation"] = new JsonObject { ["uri"] = "a.cs" },
                        ["region"] = new JsonObject { ["startLine"] = 0 }
                    }
                }
            };
            if (Accepts(s_logSchema, MakeLog(MakeRunWithResult(zeroLine))))
            {
                offenders.Add("  expected REJECT: region.startLine == 0 (AI1003)");
            }

            JsonObject emptyLocations = MakeFinalizedResult("CWE-89/clean");
            emptyLocations["locations"] = new JsonArray();
            if (Accepts(s_logSchema, MakeLog(MakeRunWithResult(emptyLocations))))
            {
                offenders.Add("  expected REJECT: empty locations array");
            }

            offenders.Should().BeEmpty(
                "ai-log.schema.json must enforce AI1005 (markdown) and AI1003 (region.startLine >= 1, " +
                "non-empty locations) on finalized results:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AILogSchema_EnforcesGHAzDOPipelineContractWhenShapePresent()
        {
            // GHAzDO1020 (id prefix) + GHAzDO1019 (four well-typed pipeline properties),
            // gated on the pipeline shape being present.
            var offenders = new List<string>();

            void Expect(string label, JsonObject automationDetails, bool accept)
            {
                JsonObject run = MakeRun();
                run["automationDetails"] = automationDetails;
                if (Accepts(s_logSchema, MakeLog(run)) != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}");
                }
            }

            // Non-pipeline automationDetails (guid only): accepted, not held to the contract.
            Expect("guid-only", new JsonObject { ["guid"] = Guid }, true);

            // A bare arbitrary id with no pipeline props is not the pipeline shape: accepted.
            Expect("arbitrary-id-no-props", new JsonObject { ["id"] = "my-run-7" }, true);

            // Fully compliant pipeline automationDetails: accepted.
            Expect("pipeline-compliant", CompliantPipeline(), true);

            // Pipeline props present but id missing the prefix: GHAzDO1020 rejects.
            JsonObject badPrefix = CompliantPipeline();
            badPrefix["id"] = "not-the-prefix/123";
            Expect("pipeline-bad-id-prefix", badPrefix, false);

            // Prefixed id present but a pipeline property missing: GHAzDO1019 rejects.
            JsonObject missingProp = CompliantPipeline();
            ((JsonObject)missingProp["properties"]).Remove("azuredevops/pipeline/build/phaseName");
            Expect("pipeline-missing-phaseName", missingProp, false);

            // buildDefinitionId == "0": GHAzDO1019 rejects (int != 0).
            JsonObject zeroId = CompliantPipeline();
            zeroId["properties"]["azuredevops/pipeline/build/buildDefinitionId"] = "0";
            Expect("pipeline-zero-buildDefinitionId", zeroId, false);

            // phaseId == empty GUID: GHAzDO1019 rejects (Guid != Empty).
            JsonObject emptyPhase = CompliantPipeline();
            emptyPhase["properties"]["azuredevops/pipeline/build/phaseId"] = "00000000-0000-0000-0000-000000000000";
            Expect("pipeline-empty-phaseId", emptyPhase, false);

            // whitespace buildDefinitionName: GHAzDO1019 rejects (non-empty).
            JsonObject wsName = CompliantPipeline();
            wsName["properties"]["azuredevops/pipeline/build/buildDefinitionName"] = "   ";
            Expect("pipeline-whitespace-name", wsName, false);

            // A prefixed id alone (the shape) demands the four props too.
            Expect("prefixed-id-without-props", new JsonObject
            {
                ["id"] = "azuredevops/pipeline/build/org/proj/1/p/main/2"
            }, false);

            offenders.Should().BeEmpty(
                "ai-log.schema.json must enforce GHAzDO1019 (four well-typed azuredevops/pipeline/build/* " +
                "properties) and GHAzDO1020 (id prefix) once a run carries the pipeline shape, while leaving a " +
                "bare guid-only automationDetails untouched:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AILogSchema_AcceptsRealFinalizedSampleLogs()
        {
            // The strongest guard: the schema must accept the actual teacher samples the
            // emit pipeline produces. CweGhasSample (GitHub-hosted, bare collapsed CWE ids) and
            // GroupingSample (multi-run generated+synthesized, bare collapsed ids) exercise the
            // GitHub-hosted collapse; CweGHAzDOSample (ADO-hosted) exercises the sub-id form AND
            // the full GHAzDO automationDetails pipeline contract. If the schema drifts from what
            // finalize emits, one of these breaks.
            var offenders = new List<string>();

            foreach (string sample in new[]
            {
                "CweGhasSample.sarif",
                "CweGHAzDOSample.sarif",
                "GroupingSample.sarif",
            })
            {
                string path = Path.Combine(AppContext.BaseDirectory, "GetSchema", "samples", sample);
                if (!File.Exists(path))
                {
                    offenders.Add($"  [{sample}] sample not found at '{path}'");
                    continue;
                }

                JsonNode log = JsonNode.Parse(File.ReadAllText(path));
                if (!Accepts(s_logSchema, log))
                {
                    offenders.Add($"  [{sample}] a real finalized sample log was REJECTED by ai-log.schema.json");
                }
            }

            offenders.Should().BeEmpty(
                "ai-log.schema.json must accept the real finalized teacher samples emit-finalize produces:\n" +
                string.Join("\n", offenders));
        }

        #region helpers

        private static JsonObject CompliantPipeline()
        {
            return new JsonObject
            {
                ["id"] = "azuredevops/pipeline/build/myorg/projectguid/123/phaseguid/refs-heads-main/4567",
                ["properties"] = new JsonObject
                {
                    ["azuredevops/pipeline/build/buildDefinitionId"] = "123",
                    ["azuredevops/pipeline/build/buildDefinitionName"] = "MyPipeline",
                    ["azuredevops/pipeline/build/phaseId"] = "12345678-1234-1234-1234-123456789abc",
                    ["azuredevops/pipeline/build/phaseName"] = "Build"
                }
            };
        }

        private static JsonObject MakeFinalizedResult(string ruleId)
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

        private static JsonObject MakeRun()
        {
            return new JsonObject
            {
                ["tool"] = new JsonObject { ["driver"] = new JsonObject { ["name"] = "MyScanner" } },
                ["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "https://github.com/o/r" } },
                ["properties"] = new JsonObject { ["ai/origin"] = "generated" },
                ["results"] = new JsonArray { MakeFinalizedResult("CWE-89/clean") }
            };
        }

        private static JsonObject MakeRunWithResult(JsonObject result)
        {
            JsonObject run = MakeRun();
            run["results"] = new JsonArray { result };
            return run;
        }

        private static JsonObject MakeLog(JsonObject run)
        {
            return new JsonObject
            {
                ["$schema"] = SarifSchemaIdentity,
                ["version"] = "2.1.0",
                ["runs"] = new JsonArray { run }
            };
        }

        private static bool Accepts(JsonSchema schema, JsonNode instance)
        {
            JsonElement element = JsonSerializer.SerializeToElement(instance);
            return schema.Evaluate(element, s_options).IsValid;
        }

        private static JsonSchema LoadAiSchema(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "GetSchema", fileName);
            return JsonSchema.FromText(File.ReadAllText(path));
        }

        // The schema $refs the public schemastore SARIF identity (document, run, and result
        // shapes). Resolve that identity offline to the vendored sarif-2.1.0.json copied next
        // to the test assembly so validation never reaches the network.
        private static EvaluationOptions BuildOptions()
        {
            string sarifPath = Path.Combine(AppContext.BaseDirectory, "GetSchema", "sarif-2.1.0.json");
            JsonSchema sarif = JsonSchema.FromText(File.ReadAllText(sarifPath));
            SchemaRegistry.Global.Register(new Uri(SarifSchemaIdentity), sarif);

            return new EvaluationOptions();
        }

        #endregion
    }
}
