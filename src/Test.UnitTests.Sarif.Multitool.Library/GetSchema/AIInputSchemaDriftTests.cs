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
    // serves (Sarif.Multitool.Library\GetSchema\ai-*.schema.json). The result and
    // run schemas are Draft 2020-12 OVERLAYS on the canonical SARIF result/run
    // shapes: they reuse the base by $ref and TIGHTEN it to the Error-level AI rules
    // plus the structural checks the emit verbs enforce. The invocation and
    // descriptor schemas are standalone thin contracts. This test pins the schemas'
    // accept/reject verdicts to that contract so a schema edit that drifts from
    // the C# rules (AIRuleIdConvention, AI1003/AI1004/AI1005/AI1006,
    // EmitRunCommand) fails loudly.
    //
    // It validates with JsonSchema.Net (json-everything), NOT Microsoft.Json.Schema:
    // the in-repo validator predates Draft 2020-12 and silently ignores the
    // Draft 2020-12 keywords the AI overlays rely on, which would make this a
    // false-green. The schemas $ref the public schemastore SARIF identity; the
    // base shape is resolved offline to the vendored sarif-2.1.0.json
    // (copied next to the test assembly) so the suite stays hermetic.
    //
    // Per repo idiom (and because [Theory] discovery is reflection-heavy), each
    // case table is walked inside a single [Fact] that collects offenders and
    // asserts the collection is empty with a teaching message.
    public class AIInputSchemaDriftTests
    {
        private const string SarifSchemaIdentity = "https://json.schemastore.org/sarif-2.1.0.json";

        private static readonly JsonSchema s_resultSchema = LoadAiSchema("ai-result.schema.json");
        private static readonly JsonSchema s_runSchema = LoadAiSchema("ai-run.schema.json");
        private static readonly JsonSchema s_invocationSchema = LoadAiSchema("ai-invocation.schema.json");
        private static readonly JsonSchema s_notificationDescriptorSchema = LoadAiSchema("ai-notification-reporting-descriptor.schema.json");
        private static readonly JsonSchema s_ruleDescriptorSchema = LoadAiSchema("ai-rule-reporting-descriptor.schema.json");
        private static readonly EvaluationOptions s_options = BuildOptions();

        private const string Guid = "12345678-1234-1234-1234-1234567890ab";

        #region ai-result.schema.json

        // ruleId accept/reject must match AIRuleIdConvention.cs (and the
        // AIRuleIdConventionTests / generating-sarif.md Rule-ID Convention tables)
        // case-for-case. AI rule ids are CWE-only; the CWE- and NOVEL- prefixes
        // are disjoint, so a plain anyOf is exact ('NOVEL-foo/bar' is rejected
        // by both branches).
        private static readonly string[] s_acceptedRuleIds =
        {
            "CWE-89/kql-injection-from-config",
            "CWE-79/dom-xss-via-sanitizer-bypass",
            "CWE-1/a",
            "CWE-327/md5-usage",
            "CWE-89/2nd-order-injection",
            "NOVEL-look-ma-i-hallucinated-outside-of-mitre",
            "NOVEL-prompt-injection-via-system-message",
            "NOVEL-a",
            "NOVEL-x509-bypass",
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
            "CWE/foo",        // base has no number
            "CWE-x/foo",      // base is not numeric
            "CWE-89/Foo",     // uppercase in sub-id
            "CWE-89/a--b",    // consecutive hyphens
            "CWE-89/a-",      // trailing hyphen
            "CWE-89/-a",      // leading hyphen
            "CVE-2021-12345/exploit-via-file-upload",   // CVE not accepted (CWE-only)
            "OWASP-A01-2021/broken-access-control",     // OWASP not accepted (CWE-only)
            "MY-CUSTOM-RULE",
            "NOVEL",
            "NOVEL-",
            "NOVEL-foo/bar",  // NOVEL- is flat — no slash (and not a valid CWE base)
            "NOVEL-foo-",     // trailing dash
            "NOVEL--foo",     // leading dash after prefix
            "NOVEL-a--b",     // consecutive hyphens
            "NOVEL-Foo",      // uppercase in sub-id
            "NOVEL-mixed-Case-123",  // mixed case in sub-id
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
                "A divergence means the schema drifted from the canonical grammar (CWE sub-id form " +
                "or the NOVEL- escape) or the anyOf ruleId patterns regressed:\n" +
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

        #endregion

        #region ai-run.schema.json

        [Fact]
        public void AIRunHeaderSchema_AcceptsAMinimalCleanHeader()
        {
            Accepts(s_runSchema, MakeHeader()).Should().BeTrue(
                "a header with tool.driver.name, a versionControlProvenance entry, and properties[ai/origin] " +
                "must validate against ai-run.schema.json");
        }

        [Fact]
        public void AIRunHeaderSchema_RequiresEssentials()
        {
            // tool.driver.name (emit header), versionControlProvenance (AI1004
            // Error), properties[ai/origin] (AI1006 Error).
            var offenders = new List<string>();

            void Expect(string label, JsonObject header, bool accept)
            {
                bool got = Accepts(s_runSchema, header);
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
                "ai-run.schema.json must require tool.driver.name (non-whitespace), a non-empty " +
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
                if (!Accepts(s_runSchema, MakeHeader(origin: origin)))
                {
                    offenders.Add($"  expected ACCEPT for origin '{origin}'");
                }
            }

            if (Accepts(s_runSchema, MakeHeader(origin: "invented")))
            {
                offenders.Add("  expected REJECT for origin 'invented'");
            }

            offenders.Should().BeEmpty(
                "ai-run.schema.json must enforce AI1006: properties[ai/origin] in " +
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
                bool got = Accepts(s_runSchema, header);
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
                "ai-run.schema.json must restrict informationUri/repositoryUri to a lowercase https:// " +
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
                bool got = Accepts(s_runSchema, header);
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
                "ai-run.schema.json must require automationDetails.guid/correlationGuid (when present) " +
                "to be a canonical 8-4-4-4-12 hex GUID:\n" + string.Join("\n", offenders));
        }

        [Fact]
        public void AIRunHeaderSchema_ForbidsReplayIgnoredFields()
        {
            // results / invocations are ignored when emit-run replays the
            // header, so the schema bounces them: they belong in their own emit
            // events, not the run header.
            var offenders = new List<string>();

            JsonObject withResults = MakeHeader();
            withResults["results"] = new JsonArray();
            if (Accepts(s_runSchema, withResults))
            {
                offenders.Add("  expected REJECT for header-level 'results'");
            }

            JsonObject withInvocations = MakeHeader();
            withInvocations["invocations"] = new JsonArray();
            if (Accepts(s_runSchema, withInvocations))
            {
                offenders.Add("  expected REJECT for header-level 'invocations'");
            }

            offenders.Should().BeEmpty(
                "ai-run.schema.json must forbid results/invocations on the run header (they are ignored " +
                "at replay and belong in their own emit events):\n" + string.Join("\n", offenders));
        }

        #endregion

        #region ai-invocation.schema.json

        [Fact]
        public void AIInvocationSchema_RequiresExecutionSuccessfulCommandLineAndWorkingDirectory()
        {
            // The AI invocation profile is an overlay on the SARIF-base invocation: it $refs the
            // base shape (so every base field keeps its type) and tightens it with three requireds
            // matching AddInvocationsCommand's receipt gate — a boolean executionSuccessful, a
            // non-whitespace string commandLine, and a workingDirectory artifactLocation with a
            // non-whitespace uri. Notifications travel INLINE, so a fully-formed invocation carrying
            // toolExecutionNotifications must validate. Each inline notification must carry a
            // producer-supplied timeUtc (the verb never stamps it). endTimeUtc must NOT be
            // required: the verb stamps it at emit time.
            var offenders = new List<string>();

            var accepted = new (string Label, JsonObject Value)[]
            {
                ("minimal", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" }
                }),
                ("rich-with-inline-notifications", new JsonObject
                {
                    ["executionSuccessful"] = false,
                    ["commandLine"] = "myscanner scan .",
                    ["exitCode"] = 1,
                    ["startTimeUtc"] = "2024-01-01T00:00:00.000Z",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" },
                    ["toolExecutionNotifications"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["level"] = "error",
                            ["timeUtc"] = "2024-01-01T00:00:01.000Z",
                            ["message"] = new JsonObject { ["text"] = "boom" }
                        }
                    }
                }),
                ("rich-with-inline-config-notifications", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" },
                    ["toolConfigurationNotifications"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["level"] = "warning",
                            ["timeUtc"] = "2024-01-01T00:00:02.000Z",
                            ["message"] = new JsonObject { ["text"] = "tweak your config" }
                        }
                    }
                }),
            };
            foreach ((string label, JsonObject value) in accepted)
            {
                if (!Accepts(s_invocationSchema, value))
                {
                    offenders.Add($"  [{label}] expected ACCEPT");
                }
            }

            var rejected = new (string Label, JsonObject Value)[]
            {
                ("missing-executionSuccessful", new JsonObject
                {
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" }
                }),
                ("missing-commandLine", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" }
                }),
                ("missing-workingDirectory", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "myscanner scan ."
                }),
                ("workingDirectory-without-uri", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject()
                }),
                ("workingDirectory-empty-uri", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "" }
                }),
                ("executionSuccessful-as-string", new JsonObject
                {
                    ["executionSuccessful"] = "true",
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" }
                }),
                ("empty-commandLine", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" }
                }),
                ("whitespace-commandLine", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "   ",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" }
                }),
                ("exec-notification-missing-timeUtc", new JsonObject
                {
                    ["executionSuccessful"] = false,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" },
                    ["toolExecutionNotifications"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["level"] = "error",
                            ["message"] = new JsonObject { ["text"] = "boom" }
                        }
                    }
                }),
                ("config-notification-missing-timeUtc", new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" },
                    ["toolConfigurationNotifications"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["level"] = "warning",
                            ["message"] = new JsonObject { ["text"] = "tweak" }
                        }
                    }
                }),
                ("exec-notification-empty-timeUtc", new JsonObject
                {
                    ["executionSuccessful"] = false,
                    ["commandLine"] = "myscanner scan .",
                    ["workingDirectory"] = new JsonObject { ["uri"] = "file:///work" },
                    ["toolExecutionNotifications"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["level"] = "error",
                            ["timeUtc"] = "",
                            ["message"] = new JsonObject { ["text"] = "boom" }
                        }
                    }
                }),
            };
            foreach ((string label, JsonObject value) in rejected)
            {
                if (Accepts(s_invocationSchema, value))
                {
                    offenders.Add($"  [{label}] expected REJECT");
                }
            }

            CollectNonObjectRejections(s_invocationSchema, offenders);

            offenders.Should().BeEmpty(
                "ai-invocation.schema.json must require a boolean executionSuccessful, a non-whitespace " +
                "string commandLine, and a workingDirectory with a non-whitespace uri (mirroring " +
                "AddInvocationsCommand's receipt gate), accept a fully-formed invocation carrying inline " +
                "notifications that each carry a timeUtc, reject inline notifications missing timeUtc, and " +
                "reject non-objects. It must NOT require " +
                "endTimeUtc (the verb stamps it):\n" + string.Join("\n", offenders));
        }

        #endregion

        #region ai-notification-reporting-descriptor.schema.json

        [Fact]
        public void AINotificationReportingDescriptorSchema_RequiresNonEmptyStringId()
        {
            // emit-notification-descriptors requires a non-empty string id (SARIF §3.49.3),
            // gating on string.IsNullOrEmpty — so whitespace-only is accepted but "" is
            // not. Extra properties are accepted (the verb only inspects id). Any id string
            // is accepted here; the NOVEL- grammar is a separate verb/schema. The
            // id-taxonomy/id-novel/id-arbitrary accepts below assert that non-gating.
            var offenders = new List<string>();

            void Expect(string label, JsonNode value, bool accept)
            {
                if (Accepts(s_notificationDescriptorSchema, value) != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}");
                }
            }

            Expect("id-taxonomy", new JsonObject { ["id"] = "CWE-89" }, true);
            Expect("id-novel", new JsonObject { ["id"] = "NOVEL-foo" }, true);
            Expect("id-arbitrary", new JsonObject { ["id"] = "anything" }, true);
            Expect("id-whitespace", new JsonObject { ["id"] = "   " }, true);
            Expect("id-with-extra-props", new JsonObject { ["id"] = "X", ["name"] = "X", ["unknown"] = 1 }, true);

            Expect("no-id", new JsonObject { ["name"] = "X" }, false);
            Expect("id-empty", new JsonObject { ["id"] = "" }, false);
            Expect("id-null", new JsonObject { ["id"] = null }, false);
            Expect("id-number", new JsonObject { ["id"] = 123 }, false);

            CollectNonObjectRejections(s_notificationDescriptorSchema, offenders);

            offenders.Should().BeEmpty(
                "ai-notification-reporting-descriptor.schema.json must require a non-empty string id (minLength:1 mirrors the " +
                "verb's IsNullOrEmpty gate, so '   ' is accepted) and otherwise accept any object:\n" +
                string.Join("\n", offenders));
        }

        #endregion

        #region ai-rule-reporting-descriptor.schema.json

        [Fact]
        public void AIRuleReportingDescriptorSchema_RequiresWellFormedNovelId()
        {
            // ai-rule-reporting-descriptor is a standalone schema that pins the descriptor id
            // to the full NOVEL- escape-hatch grammar ^NOVEL-[a-z0-9]+(-[a-z0-9]+)*$ — the SAME
            // lowercase-kebab form a result's NOVEL- ruleId must satisfy, so a descriptor id is
            // byte-identical to the ruleId that references it. Bare 'NOVEL-', a slash, an
            // uppercase tail, and a trailing hyphen are all rejected. The non-empty-string-id
            // rejects (id-empty, no-id, id-number) follow from the inline type/pattern checks.
            var offenders = new List<string>();

            void Expect(string label, JsonNode value, bool accept)
            {
                if (Accepts(s_ruleDescriptorSchema, value) != accept)
                {
                    offenders.Add($"  [{label}] expected accept={accept}");
                }
            }

            Expect("novel-simple", new JsonObject { ["id"] = "NOVEL-prompt-injection" }, true);
            Expect("novel-multi-segment", new JsonObject { ["id"] = "NOVEL-prompt-injection-via-system-message" }, true);
            Expect("novel-sql-injection", new JsonObject { ["id"] = "NOVEL-sql-injection" }, true);
            Expect("novel-single-char-tail", new JsonObject { ["id"] = "NOVEL-a" }, true);
            Expect("novel-alphanumeric", new JsonObject { ["id"] = "NOVEL-cwe-89-lookalike" }, true);

            Expect("taxonomy-id", new JsonObject { ["id"] = "CWE-89" }, false);
            Expect("novel-bare-prefix", new JsonObject { ["id"] = "NOVEL-" }, false);
            Expect("novel-with-slash", new JsonObject { ["id"] = "NOVEL-foo/bar" }, false);
            Expect("novel-mixed-case-tail", new JsonObject { ["id"] = "NOVEL-mixedCase-123" }, false);
            Expect("novel-uppercase-tail", new JsonObject { ["id"] = "NOVEL-PROMPT" }, false);
            Expect("novel-trailing-hyphen", new JsonObject { ["id"] = "NOVEL-trailing-" }, false);
            Expect("novel-double-hyphen", new JsonObject { ["id"] = "NOVEL--double" }, false);
            Expect("novel-no-dash", new JsonObject { ["id"] = "NOVEL" }, false);
            Expect("lowercase-prefix", new JsonObject { ["id"] = "novel-foo" }, false);
            Expect("id-empty", new JsonObject { ["id"] = "" }, false);
            Expect("no-id", new JsonObject { ["name"] = "X" }, false);
            Expect("id-number", new JsonObject { ["id"] = 123 }, false);

            CollectNonObjectRejections(s_ruleDescriptorSchema, offenders);

            offenders.Should().BeEmpty(
                "ai-rule-reporting-descriptor.schema.json must require a non-empty string id and " +
                "pin the full NOVEL- grammar gate inline " +
                "^NOVEL-[a-z0-9]+(-[a-z0-9]+)*$ — the same lowercase-kebab form the result-side NOVEL- ruleId " +
                "must satisfy, so 'NOVEL-', 'NOVEL-foo/bar', an uppercase tail, and a trailing hyphen are all " +
                "rejected:\n" + string.Join("\n", offenders));
        }

        #endregion

        #region helpers

        // The three thin-contract schemas pin "must be a JSON object" as their entire
        // structural check. Each non-object instance must be REJECTED; a regression
        // that loosened type:object (or imported a base $ref that changed the verdict)
        // would surface here.
        private static void CollectNonObjectRejections(JsonSchema schema, List<string> offenders)
        {
            var nonObjects = new (string Label, JsonNode Value)[]
            {
                ("array", new JsonArray()),
                ("string", JsonValue.Create("x")),
                ("number", JsonValue.Create(123)),
                ("boolean", JsonValue.Create(true)),
                ("null", null),
            };
            foreach ((string label, JsonNode value) in nonObjects)
            {
                if (Accepts(schema, value))
                {
                    offenders.Add($"  [non-object:{label}] expected REJECT");
                }
            }
        }

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

        // The result and run schemas $ref the public schemastore SARIF identity. Resolve
        // that identity offline to the vendored sarif-2.1.0.json (copied next to the
        // test assembly) so validation never reaches the network; JsonSchema.Net resolves
        // $refs through the global registry. The invocation and descriptor schemas are
        // standalone thin contracts (no $ref), so they need no registration.
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
