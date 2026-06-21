// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Conformance matrix for ai-log.schema.json, the WHOLE-LOG overlay get-schema serves
    // for emit-finalize output. Where AILogSchemaDriftTests pins the schema's accept/reject
    // verdicts against the C# rules it DOES encode, this class maps the deliberate fidelity
    // boundary in two directions:
    //
    //   ACCEPTANCE  - real, valid finalize output shapes the schema MUST pass. A wrongly
    //                 rejected case here is a schema bug: the overlay would block a log the
    //                 emitter legitimately produces. These are the maturity payoff.
    //
    //   BOUNDARY    - logs the schema accepts but a RICH (C#) AI rule rejects. JSON Schema
    //                 cannot express reciprocity, all-or-nothing co-presence, closed-value
    //                 enums on open property bags, or non-persistence, so these pass the
    //                 overlay and are caught only by emit-finalize --validate. Each case is
    //                 annotated with the governing rule so the gap is documented, not silent.
    //
    //   REJECTION   - a shape the overlay correctly refuses BY DESIGN (a --no-repo log has
    //                 no versionControlProvenance, so it is unpublishable and the schema
    //                 says so up front).
    //
    // It shares AiLogSchemaFixture's single offline SARIF-identity registration and the
    // canonical builders, so it never re-registers the base schema in SchemaRegistry.Global.
    public class AILogSchemaConformanceTests
    {
        // ---- ACCEPTANCE: shapes the overlay MUST pass ------------------------------------

        [Fact]
        public void Accepts_NovelRuleIdEscapeHatch()
        {
            JsonObject log = AiLogSchemaFixture.Log(
                AiLogSchemaFixture.RunWithResult(
                    AiLogSchemaFixture.FinalizedResult("NOVEL-prompt-injection-via-system-message")));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void Accepts_RollingHashPartialFingerprints()
        {
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            result["partialFingerprints"] = new JsonObject
            {
                ["aiRollingHash/v1"] = "9f86d081884c7d659a2feaa0c55ad015"
            };

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void Accepts_Suppressions()
        {
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-79/reflected-xss");
            result["suppressions"] = new JsonArray
            {
                new JsonObject { ["kind"] = "external", ["status"] = "accepted" }
            };

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void Accepts_ExploitabilityPropertyBag()
        {
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            result["properties"] = new JsonObject
            {
                ["ai/exploitability"] = "exploitable",
                ["ai/attackerPosition"] = "remote-unauthenticated",
                ["ai/evidence"] = "Tainted request parameter flows unsanitized into a string-concatenated query."
            };

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void Accepts_NotificationsAndLearningSignalOnEmptyResults()
        {
            JsonObject run = AiLogSchemaFixture.Run();
            run["results"] = new JsonArray();
            run["invocations"] = new JsonArray
            {
                new JsonObject
                {
                    ["executionSuccessful"] = true,
                    ["toolExecutionNotifications"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["level"] = "note",
                            ["message"] = new JsonObject { ["text"] = "LEARNING-SIGNAL: zero findings after full sweep." }
                        }
                    }
                }
            };

            JsonObject log = AiLogSchemaFixture.Log(run);

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void Accepts_ReciprocalTwoRunGrouping()
        {
            JsonObject left = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            left["guid"] = "11111111-1111-1111-1111-111111111111";
            left["properties"] = new JsonObject
            {
                ["ai/relatedResultGuids"] = new JsonArray { "22222222-2222-2222-2222-222222222222" }
            };

            JsonObject right = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            right["guid"] = "22222222-2222-2222-2222-222222222222";
            right["properties"] = new JsonObject
            {
                ["ai/relatedResultGuids"] = new JsonArray { "11111111-1111-1111-1111-111111111111" }
            };

            JsonObject log = AiLogSchemaFixture.Log(
                AiLogSchemaFixture.RunWithResult(left),
                AiLogSchemaFixture.RunWithResult(right));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(AiLogSchemaFixture.Explain(log));
        }

        // ---- BOUNDARY: overlay accepts, a rich (C#) rule rejects -------------------------
        // Each case is a log the JSON Schema cannot reject but emit-finalize --validate
        // catches. The assertion is BeTrue: it documents the gap by pinning what the schema
        // lets through, and names the rule that closes it.

        [Fact]
        public void BoundaryAI2014_PartialExploitabilityCoPresence()
        {
            // AI2014 requires the exploitability triple to be all-present or all-absent.
            // A lone ai/exploitability with no attackerPosition/evidence is a half-filled
            // claim; the open property bag in base SARIF cannot express the co-presence
            // constraint, so the overlay accepts it.
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            result["properties"] = new JsonObject { ["ai/exploitability"] = "exploitable" };

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(
                "the overlay cannot enforce AI2014 all-or-nothing co-presence: " + AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void BoundaryAI2014_InventedExploitabilityValue()
        {
            // AI2014 closes ai/exploitability to a known vocabulary. 'unconfirmed' is not in
            // it, but properties are an open string map in base SARIF, so the overlay accepts
            // any value.
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            result["properties"] = new JsonObject
            {
                ["ai/exploitability"] = "unconfirmed",
                ["ai/attackerPosition"] = "remote-unauthenticated",
                ["ai/evidence"] = "Reachability not established."
            };

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(
                "the overlay cannot enforce AI2014's closed exploitability vocabulary: " + AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void BoundaryAI1015_NonReciprocalGrouping()
        {
            // AI1015 requires grouping links to be reciprocal. Here left points at right but
            // right does not point back. The overlay validates each run independently and
            // cannot express the cross-run reciprocity, so it accepts the asymmetric log.
            JsonObject left = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            left["guid"] = "11111111-1111-1111-1111-111111111111";
            left["properties"] = new JsonObject
            {
                ["ai/relatedResultGuids"] = new JsonArray { "22222222-2222-2222-2222-222222222222" }
            };

            JsonObject right = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            right["guid"] = "22222222-2222-2222-2222-222222222222";

            JsonObject log = AiLogSchemaFixture.Log(
                AiLogSchemaFixture.RunWithResult(left),
                AiLogSchemaFixture.RunWithResult(right));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(
                "the overlay cannot enforce AI1015 grouping reciprocity: " + AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void BoundaryAI2011_PersistedPartialFingerprints()
        {
            // AI2011 forbids persisting the rolling-hash partialFingerprints back into the
            // emitted log as a stable identity. The overlay sees a well-formed string map and
            // cannot tell a transient hash from an illegally persisted one, so it accepts.
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-89/sql-injection");
            result["partialFingerprints"] = new JsonObject
            {
                ["aiRollingHash/v1"] = "9f86d081884c7d659a2feaa0c55ad015",
                ["persisted/v1"] = "do-not-persist-me"
            };

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(
                "the overlay cannot enforce AI2011 non-persistence: " + AiLogSchemaFixture.Explain(log));
        }

        [Fact]
        public void BoundaryCategoryRuleId_CategoryBaseIsNotAValidMappingTarget()
        {
            // A CWE Category (e.g. CWE-16 'Configuration') is an organizational bucket, not a
            // Weakness, and is never a valid mapping target -- MITRE's own guidance is that
            // Categories must not be mapped. The overlay's ruleId anyOf is purely structural
            // (CWE-<digits>/<kebab>), so 'CWE-16/some-config-issue' matches and is accepted.
            // Whether the base CWE is a Category or a Weakness is taxonomy data the schema
            // does not carry, so emit-finalize --validate (the #3080 Category-mapping rule)
            // owns this rejection as a hard producer-emit failure.
            JsonObject result = AiLogSchemaFixture.FinalizedResult("CWE-16/insecure-default-config");

            JsonObject log = AiLogSchemaFixture.Log(AiLogSchemaFixture.RunWithResult(result));

            AiLogSchemaFixture.Accepts(log).Should().BeTrue(
                "the overlay cannot tell a Category base CWE from a Weakness; the Category-mapping rule owns it: "
                + AiLogSchemaFixture.Explain(log));
        }

        // ---- REJECTION: a shape the overlay refuses BY DESIGN ----------------------------

        [Fact]
        public void Rejects_RunWithoutVersionControlProvenance()
        {
            // A --no-repo log has no versionControlProvenance. It is unpublishable, and the
            // overlay says so up front: versionControlProvenance is required (minItems 1).
            JsonObject run = AiLogSchemaFixture.Run();
            run.Remove("versionControlProvenance");

            JsonObject log = AiLogSchemaFixture.Log(run);

            AiLogSchemaFixture.Accepts(log).Should().BeFalse(
                "a --no-repo run has no versionControlProvenance and is unpublishable");
        }
    }
}
