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
    // Drift guard for the AI emit-profile input schemas that the get-schema verb
    // serves (Sarif.Multitool.Library\GetSchema\ai-*.schema.json). These schemas
    // are Draft 2020-12 OVERLAYS on the canonical SARIF result/run shapes: they
    // reuse the base by $ref and TIGHTEN it to the Error-level AI rules plus the
    // structural checks the emit verbs enforce. This test pins the schemas'
    // accept/reject verdicts to that contract so a schema edit that drifts from
    // the C# rules (AIRuleIdConvention, AI1003/AI1004/AI1005/AI1006,
    // EmitInitRunCommand) fails loudly.
    //
    // It validates with JsonSchema.Net (json-everything), NOT Microsoft.Json.Schema:
    // the in-repo validator predates Draft 2020-12 and silently ignores the
    // load-bearing if/then/else keywords, which would make this a
    // false-green. The schemas $ref the public schemastore SARIF identity; the
    // base shape is resolved offline to the vendored sarif-2.1.0-rtm.6.json
    // (copied next to the test assembly) so the suite stays hermetic.
    //
    // Per repo idiom (and because [Theory] discovery is reflection-heavy), each
    // case table is walked inside a single [Fact] that collects offenders and
    // asserts the collection is empty with a teaching message.
    public class AIInputSchemaDriftTests
    {
        private const string SarifSchemaIdentity = "https://json.schemastore.org/sarif-2.1.0.json";

        private static readonly JsonSchema s_resultSchema = LoadAiSchema("ai-result.schema.json");
        private static readonly JsonSchema s_runHeaderSchema = LoadAiSchema("ai-run-header.schema.json");
        private static readonly EvaluationOptions s_options = BuildOptions();

        private const string Guid = "12345678-1234-1234-1234-1234567890ab";

        #region ai-result.schema.json

        // ruleId accept/reject must match AIRuleIdConvention.cs (and the
        // AIRuleIdConventionTests / docs/AI-RuleId-Convention.md tables)
        // case-for-case. The if/then/else is load-bearing: 'NOVEL-foo/bar'
        // matches the taxonomy grammar but is rejected because NOVEL- is exclusive.
        private static readonly string[] s_acceptedRuleIds =
        {
            "CWE-89/kql-injection-from-config",
            "CWE-79/dom-xss-via-sanitizer-bypass",
            "CWE-1/a",
            "CVE-2021-12345/exploit-via-file-upload",
            "OWASP-A01-2021/broken-access-control",
            "NOVEL-look-ma-i-hallucinated-outside-of-mitre",
            "NOVEL-prompt-injection-via-system-message",
            "NOVEL-a",
            "NOVEL-mixed-Case-123",
        };

        private static readonly string[] s_rejectedRuleIds =
        {
            "",               // empty
            "   ",            // whitespace-only
            "CWE-89",         // bare taxonomy id
            "CWE-89/",        // empty sub-id
            "CWE-89/a/b",     // sub-id has slash
            "CWE-89/a b",     // sub-id has whitespace
            "cwe-89/foo",     // lowercase base
            "CWE/foo",        // base has no '-' segment
            "MY-CUSTOM-RULE",
            "NOVEL",
            "NOVEL-",
            "NOVEL-foo/bar",  // <-- the drift-catch (taxonomy-shaped but NOVEL-exclusive)
            "NOVEL-foo-",     // trailing dash
            "novel-foo",      // lowercase prefix
            "NOVEL-foo bar",  // whitespace in sub-id
        };

        [Fact]
        public void AIResultSchema_RuleIdGrammarMatchesConvention()
        {
            var offenders = new List<string>();

            foreach (string ruleId in s_acceptedRuleIds)
            {
                if (!Accepts(s_resultSchema, MakeResult(ruleId: ruleId)))
                {
                    offenders.Add($"  expected ACCEPT, got REJECT: '{ruleId}'");
                }
            }

            foreach (string ruleId in s_rejectedRuleIds)
            {
                if (Accepts(s_resultSchema, MakeResult(ruleId: ruleId)))
                {
                    offenders.Add($"  expected REJECT, got ACCEPT: '{ruleId}'");
                }
            }

            offenders.Should().BeEmpty(
                "ai-result.schema.json's ruleId verdict must match AIRuleIdConvention.cs case-for-case. " +
                "A divergence means the schema drifted from the canonical grammar (taxonomy sub-id form " +
                "or the NOVEL- escape) or the load-bearing if/then/else routing regressed:\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AIResultSchema_ForbidsProducerOwnedIndexAndIdentityState()
        {
            var offenders = new List<string>();

            JsonObject baseOk = MakeResult("CWE-89/clean");
            if (!Accepts(s_resultSchema, baseOk))
            {
                offenders.Add("  [base] a minimal clean result should ACCEPT");
            }

            // Top-level identity/index state the multitool owns at finalize time.
            var forbids = new (string Name, JsonNode Value)[]
            {
                ("ruleIndex", 0),
                ("guid", "11111111-1111-1111-1111-111111111111"),
                ("fingerprints", new JsonObject { ["x"] = "y" }),
                ("partialFingerprints", new JsonObject { ["x"] = "y" }),
            };
            foreach ((string name, JsonNode value) in forbids)
            {
                JsonObject result = MakeResult("CWE-89/clean");
                result[name] = value;
                if (Accepts(s_resultSchema, result))
                {
                    offenders.Add($"  [forbid] expected REJECT for top-level '{name}'");
                }
            }

            // Nested artifactLocation.index on the primary location path.
            JsonObject idxResult = MakeResult("CWE-89/clean");
            idxResult["locations"][0]["physicalLocation"]["artifactLocation"]["index"] = 3;
            if (Accepts(s_resultSchema, idxResult))
            {
                offenders.Add("  [forbid] expected REJECT for locations[].artifactLocation.index");
            }

            offenders.Should().BeEmpty(
                "ai-result.schema.json must bounce producer-supplied index/identity state. ruleIndex, guid, " +
                "fingerprints, partialFingerprints, and artifactLocation.index are owned by the emit-finalize " +
                "pass (Run.GetFileIndex / fingerprint computation), not the AI producer:\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AIResultSchema_RequiresEssentialPayload()
        {
            var offenders = new List<string>();

            foreach (string missing in new[] { "message", "locations" })
            {
                JsonObject result = MakeResult("CWE-89/clean");
                result.Remove(missing);
                if (Accepts(s_resultSchema, result))
                {
                    offenders.Add($"  expected REJECT when missing '{missing}'");
                }
            }

            JsonObject noRuleId = MakeResult("CWE-89/clean");
            noRuleId.Remove("ruleId");
            if (Accepts(s_resultSchema, noRuleId))
            {
                offenders.Add("  expected REJECT when missing 'ruleId'");
            }

            offenders.Should().BeEmpty(
                "ai-result.schema.json must require ruleId, message, and locations on every result:\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AIResultSchema_EnforcesMarkdownMessage()
        {
            // AI1005 (Error): result.message.markdown must be present and
            // non-whitespace (the \S pattern mirrors IsNullOrWhiteSpace).
            var cases = new (string Name, JsonObject Message, bool Accept)[]
            {
                ("missing-markdown", new JsonObject { ["text"] = "no md." }, false),
                ("whitespace-markdown", new JsonObject { ["text"] = "x.", ["markdown"] = "   " }, false),
                ("empty-markdown", new JsonObject { ["text"] = "x.", ["markdown"] = "" }, false),
                ("good-markdown", new JsonObject { ["text"] = "x.", ["markdown"] = "**x**" }, true),
            };

            var offenders = new List<string>();
            foreach ((string name, JsonObject message, bool accept) in cases)
            {
                bool got = Accepts(s_resultSchema, MakeResult("CWE-89/clean", message: message));
                if (got != accept)
                {
                    offenders.Add($"  [{name}] expected accept={accept}, got {got}");
                }
            }

            offenders.Should().BeEmpty(
                "ai-result.schema.json must enforce AI1005: result.message.markdown is required and " +
                "must be non-whitespace:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AIResultSchema_EnforcesRegionConstraints()
        {
            // AI1003 (Error): a location carrying a physicalLocation must carry a
            // region with startLine >= 1 (the C# rule treats startLine == 0 as
            // missing). A logical-only location does not fire the rule. locations
            // must be non-empty.
            var offenders = new List<string>();

            JsonArray noRegion = new()
            {
                new JsonObject { ["physicalLocation"] = new JsonObject { ["artifactLocation"] = new JsonObject { ["uri"] = "a.cs" } } }
            };
            if (Accepts(s_resultSchema, MakeResult("CWE-89/clean", locations: noRegion)))
            {
                offenders.Add("  expected REJECT: physicalLocation without region");
            }

            JsonArray zeroLine = new()
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
            if (Accepts(s_resultSchema, MakeResult("CWE-89/clean", locations: zeroLine)))
            {
                offenders.Add("  expected REJECT: region.startLine == 0");
            }

            JsonArray logicalOnly = new()
            {
                new JsonObject { ["logicalLocations"] = new JsonArray { new JsonObject { ["fullyQualifiedName"] = "A.B" } } }
            };
            if (!Accepts(s_resultSchema, MakeResult("CWE-89/clean", locations: logicalOnly)))
            {
                offenders.Add("  expected ACCEPT: logical-only location (AI1003 does not fire)");
            }

            if (Accepts(s_resultSchema, MakeResult("CWE-89/clean", locations: new JsonArray())))
            {
                offenders.Add("  expected REJECT: empty locations array (minItems)");
            }

            offenders.Should().BeEmpty(
                "ai-result.schema.json must enforce AI1003 region constraints and require a non-empty " +
                "locations array:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AIResultSchema_IfThenElseRuleIdRoutingIsLoadBearing()
        {
            // Prove the if/then/else cannot be flattened to a naive anyOf. A naive
            // anyOf(taxonomy, novel) WRONGLY ACCEPTS 'NOVEL-foo/bar' (it matches the
            // taxonomy branch), whereas the shipped if/then/else REJECTS it because
            // a NOVEL- id is routed exclusively to the novelEscape grammar. This is
            // the exact anyOf trap the if/then/else exists to avoid.
            JsonSchema naive = JsonSchema.FromText(
                """
                {
                  "$schema": "https://json-schema.org/draft/2020-12/schema",
                  "type": "object",
                  "properties": {
                    "ruleId": {
                      "type": "string",
                      "minLength": 1,
                      "anyOf": [
                        { "pattern": "^[A-Z][A-Z0-9]*(-[A-Za-z0-9]+)+/[^/\\s]+$" },
                        { "pattern": "^NOVEL-[A-Za-z0-9]+(-[A-Za-z0-9]+)*$" }
                      ]
                    }
                  }
                }
                """);

            JsonObject trap = MakeResult("NOVEL-foo/bar");
            bool naiveAccepts = Accepts(naive, trap);
            bool oursRejects = !Accepts(s_resultSchema, trap);

            naiveAccepts.Should().BeTrue(
                "a naive anyOf(taxonomy, novel) accepts 'NOVEL-foo/bar' because it matches the taxonomy branch; " +
                "this is the trap the if/then/else exists to avoid");
            oursRejects.Should().BeTrue(
                "ai-result.schema.json's if/then/else must route a NOVEL- id exclusively to the novelEscape grammar " +
                "and REJECT 'NOVEL-foo/bar'; if this fails the routing has been flattened and the schema drifted " +
                "from AIRuleIdConvention's NOVEL-exclusivity rule");
        }

        #endregion

        #region ai-run-header.schema.json

        [Fact]
        public void AIRunHeaderSchema_AcceptsAMinimalCleanHeader()
        {
            Accepts(s_runHeaderSchema, MakeHeader()).Should().BeTrue(
                "a header with tool.driver.name, a versionControlProvenance entry, and properties[ai/origin] " +
                "must validate against ai-run-header.schema.json");
        }

        [Fact]
        public void AIRunHeaderSchema_RequiresEssentials()
        {
            // tool.driver.name (emit header), versionControlProvenance (AI1004
            // Error), properties[ai/origin] (AI1006 Error).
            var offenders = new List<string>();

            void Expect(string label, JsonObject header, bool accept)
            {
                bool got = Accepts(s_runHeaderSchema, header);
                if (got != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}, got {got}");
                }
            }

            JsonObject noTool = MakeHeader();
            noTool.Remove("tool");
            Expect("no-tool", noTool, false);

            Expect("no-driver-name", new JsonObject
            {
                ["tool"] = new JsonObject { ["driver"] = new JsonObject() },
                ["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "https://github.com/o/r" } },
                ["properties"] = new JsonObject { ["ai/origin"] = "generated" }
            }, false);

            JsonObject wsName = MakeHeader();
            wsName["tool"]["driver"]["name"] = "   ";
            Expect("whitespace-driver-name", wsName, false);

            JsonObject noVcp = MakeHeader();
            noVcp.Remove("versionControlProvenance");
            Expect("no-vcp", noVcp, false);

            JsonObject emptyVcp = MakeHeader();
            emptyVcp["versionControlProvenance"] = new JsonArray();
            Expect("empty-vcp", emptyVcp, false);

            JsonObject noProps = MakeHeader();
            noProps.Remove("properties");
            Expect("no-properties", noProps, false);

            Expect("no-ai-origin", new JsonObject
            {
                ["tool"] = new JsonObject { ["driver"] = new JsonObject { ["name"] = "X" } },
                ["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "https://github.com/o/r" } },
                ["properties"] = new JsonObject()
            }, false);

            offenders.Should().BeEmpty(
                "ai-run-header.schema.json must require tool.driver.name (non-whitespace), a non-empty " +
                "versionControlProvenance (AI1004), and properties[ai/origin] (AI1006):\n" +
                string.Join("\n", offenders));
        }

        [Fact]
        public void AIRunHeaderSchema_EnforcesOriginEnum()
        {
            // AI1006 (Error): properties[ai/origin] is one of generated|annotated|synthesized.
            var offenders = new List<string>();

            foreach (string origin in new[] { "generated", "annotated", "synthesized" })
            {
                if (!Accepts(s_runHeaderSchema, MakeHeader(origin: origin)))
                {
                    offenders.Add($"  expected ACCEPT for origin '{origin}'");
                }
            }

            if (Accepts(s_runHeaderSchema, MakeHeader(origin: "invented")))
            {
                offenders.Add("  expected REJECT for origin 'invented'");
            }

            offenders.Should().BeEmpty(
                "ai-run-header.schema.json must enforce AI1006: properties[ai/origin] in " +
                "{generated, annotated, synthesized}:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AIRunHeaderSchema_EnforcesUriSchemes()
        {
            // TryValidateUri / TryValidateRunHeader restrict informationUri and
            // repositoryUri to https, and SRCROOT.uri to https|file. The AI profile
            // additionally requires a lowercase scheme (canonical-form tightening:
            // .NET's uri.Scheme is lowercased, so C# would accept HTTPS://; the
            // schema deliberately does not).
            var offenders = new List<string>();

            void Expect(string label, JsonObject header, bool accept)
            {
                bool got = Accepts(s_runHeaderSchema, header);
                if (got != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}, got {got}");
                }
            }

            JsonObject infoHttps = MakeHeader();
            infoHttps["tool"]["driver"]["informationUri"] = "https://x/y";
            Expect("infouri-https", infoHttps, true);

            JsonObject infoHttp = MakeHeader();
            infoHttp["tool"]["driver"]["informationUri"] = "http://x/y";
            Expect("infouri-http", infoHttp, false);

            JsonObject infoUpper = MakeHeader();
            infoUpper["tool"]["driver"]["informationUri"] = "HTTPS://x/y";
            Expect("infouri-uppercase-scheme", infoUpper, false);

            JsonObject repoHttp = MakeHeader();
            repoHttp["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "http://x/y" } };
            Expect("repo-http", repoHttp, false);

            Expect("srcroot-https", SrcRootHeader("https://x/"), true);
            Expect("srcroot-file", SrcRootHeader("file:///c/"), true);
            Expect("srcroot-ftp", SrcRootHeader("ftp://x/"), false);

            offenders.Should().BeEmpty(
                "ai-run-header.schema.json must restrict informationUri/repositoryUri to a lowercase https:// " +
                "scheme and SRCROOT.uri to lowercase https|file (canonical-form tightening over the C# scheme " +
                "check):\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AIRunHeaderSchema_EnforcesCanonicalGuids()
        {
            // TryValidateOptionalGuidToken requires automationDetails.guid /
            // correlationGuid (when present) to be a canonical 8-4-4-4-12 hex GUID.
            var offenders = new List<string>();

            void Expect(string label, JsonObject header, bool accept)
            {
                bool got = Accepts(s_runHeaderSchema, header);
                if (got != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}, got {got}");
                }
            }

            Expect("guid-good", AutomationHeader("guid", Guid), true);
            Expect("guid-bad", AutomationHeader("guid", "not-a-guid"), false);
            Expect("correlationGuid-good", AutomationHeader("correlationGuid", Guid), true);
            Expect("correlationGuid-bad", AutomationHeader("correlationGuid", "1234"), false);

            offenders.Should().BeEmpty(
                "ai-run-header.schema.json must require automationDetails.guid/correlationGuid (when present) " +
                "to be a canonical 8-4-4-4-12 hex GUID:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AIRunHeaderSchema_ForbidsReplayIgnoredFields()
        {
            // results / invocations are ignored when emit-init-run replays the
            // header, so the schema bounces them: they belong in their own emit
            // events, not the run header.
            var offenders = new List<string>();

            JsonObject withResults = MakeHeader();
            withResults["results"] = new JsonArray();
            if (Accepts(s_runHeaderSchema, withResults))
            {
                offenders.Add("  expected REJECT for header-level 'results'");
            }

            JsonObject withInvocations = MakeHeader();
            withInvocations["invocations"] = new JsonArray();
            if (Accepts(s_runHeaderSchema, withInvocations))
            {
                offenders.Add("  expected REJECT for header-level 'invocations'");
            }

            offenders.Should().BeEmpty(
                "ai-run-header.schema.json must forbid results/invocations on the run header (they are ignored " +
                "at replay and belong in their own emit events):\n" + string.Join("\n", offenders));
        }

        #endregion

        #region helpers

        private static bool Accepts(JsonSchema schema, JsonNode instance)
        {
            // JsonSchema.Net 9.x evaluates a JsonElement, so project the constructed
            // JsonNode instance into one.
            JsonElement element = JsonSerializer.SerializeToElement(instance);
            return schema.Evaluate(element, s_options).IsValid;
        }

        private static JsonObject MakeResult(string ruleId = null, JsonObject message = null, JsonArray locations = null)
        {
            var result = new JsonObject();

            if (ruleId != null)
            {
                result["ruleId"] = ruleId;
            }

            result["message"] = message ?? new JsonObject { ["text"] = "A finding.", ["markdown"] = "**A finding.**" };

            result["locations"] = locations ?? new JsonArray
            {
                new JsonObject
                {
                    ["physicalLocation"] = new JsonObject
                    {
                        ["artifactLocation"] = new JsonObject { ["uri"] = "src/app.cs" },
                        ["region"] = new JsonObject { ["startLine"] = 1 }
                    }
                }
            };

            return result;
        }

        private static JsonObject MakeHeader(string origin = "generated")
        {
            return new JsonObject
            {
                ["tool"] = new JsonObject { ["driver"] = new JsonObject { ["name"] = "MyScanner" } },
                ["versionControlProvenance"] = new JsonArray { new JsonObject { ["repositoryUri"] = "https://github.com/o/r" } },
                ["properties"] = new JsonObject { ["ai/origin"] = origin }
            };
        }

        private static JsonObject SrcRootHeader(string uri)
        {
            JsonObject header = MakeHeader();
            header["originalUriBaseIds"] = new JsonObject { ["SRCROOT"] = new JsonObject { ["uri"] = uri } };
            return header;
        }

        private static JsonObject AutomationHeader(string field, string value)
        {
            JsonObject header = MakeHeader();
            header["automationDetails"] = new JsonObject { [field] = value };
            return header;
        }

        private static JsonSchema LoadAiSchema(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "GetSchema", fileName);
            return JsonSchema.FromText(File.ReadAllText(path));
        }

        // The AI schemas $ref the public schemastore SARIF identity. Resolve that
        // identity offline to the vendored sarif-2.1.0-rtm.6.json (copied next to
        // the test assembly) so validation never reaches the network. JsonSchema.Net
        // resolves $refs through the global registry, so register the base shape there.
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
