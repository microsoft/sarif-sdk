// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Replays a SARIF event log into an in-memory <see cref="SarifLog"/>.
    /// </summary>
    /// <remarks>
    /// <para>v1 contract:</para>
    /// <list type="bullet">
    /// <item><description>At most one <c>run-header</c> event; if present, it SHOULD be first.
    /// The header MAY carry a partial <see cref="Run"/> shape (tool, language, columnKind,
    /// defaultEncoding, defaultSourceLanguage, originalUriBaseIds, versionControlProvenance,
    /// automationDetails, baselineGuid, redactionTokens, etc.). <c>results</c>, <c>invocations</c>,
    /// and <c>notifications</c> on a header are ignored — those belong in their own events.</description></item>
    /// <item><description><c>result</c> events MUST be self-contained: <c>ruleIndex</c> is ignored
    /// (re-derived from <c>ruleId</c>); index references into run-level caches are not validated
    /// in v1 (producers needing indexed references should use <see cref="Writers.SarifLogger"/>
    /// directly). Every <see cref="Result.RuleId"/> MUST conform to
    /// <see cref="AIRuleIdConvention"/> — taxonomy sub-id form
    /// (<c>&lt;BASE&gt;/&lt;sub-id&gt;</c>, e.g., <c>CWE-89/kql-injection-from-config</c>) or
    /// NOVEL escape hatch (<c>NOVEL-&lt;sub-id&gt;</c>). Violations throw
    /// <see cref="AIRuleIdConventionException"/> listing every offender at once.</description></item>
    /// <item><description><c>invocation</c> events are appended to <c>run.invocations</c> in
    /// event order.</description></item>
    /// <item><description><c>notification</c> events are buffered and attached at finalize to
    /// <c>run.invocations[last].toolExecutionNotifications</c>. If no invocation has been
    /// supplied, a synthetic <c>{ "executionSuccessful": true }</c> invocation is created to
    /// hold them (SARIF requires a home for notifications). Notifications whose <c>timeUtc</c>
    /// is unset on the event payload are stamped with <see cref="DateTime.UtcNow"/> at
    /// replay time so AI execution-timeline consumers can order events without burdening
    /// producers to track wall-clock themselves (cf. AI2019). Producer-supplied
    /// <c>timeUtc</c> values are preserved.</description></item>
    /// </list>
    /// <para>Descriptor auto-registration mirrors <see cref="Writers.SarifLogger"/>: on first
    /// sighting of a <see cref="Result.RuleId"/>, the replayer appends a minimal
    /// <see cref="ReportingDescriptor"/> to <c>run.tool.driver.rules</c> and back-fills
    /// <see cref="Result.RuleIndex"/>.</para>
    /// </remarks>
    public static class SarifEventReplayer
    {
        /// <summary>
        /// Reads the event log at <paramref name="eventLogPath"/> and returns a
        /// <see cref="SarifLog"/> with a single <see cref="Run"/>.
        /// </summary>
        public static SarifLog Replay(string eventLogPath)
        {
            if (string.IsNullOrEmpty(eventLogPath))
            {
                throw new ArgumentException("Event log path must be supplied.", nameof(eventLogPath));
            }

            var reader = new SarifEventLogReader();
            return Replay(reader.Read(eventLogPath));
        }

        /// <summary>
        /// Reads the supplied <paramref name="events"/> and returns a <see cref="SarifLog"/> with
        /// a single <see cref="Run"/>.
        /// </summary>
        public static SarifLog Replay(IEnumerable<SarifEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            Run run = null;
            var results = new List<Result>();
            var invocations = new List<Invocation>();
            var notifications = new List<Notification>();
            var ruleDescriptors = new List<ReportingDescriptor>();
            var notificationDescriptors = new List<ReportingDescriptor>();
            bool headerSeen = false;

            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            foreach (SarifEvent sarifEvent in events)
            {
                switch (sarifEvent.Kind)
                {
                    case SarifEventKinds.RunHeader:
                        if (headerSeen)
                        {
                            throw new SarifEventLogException(
                                "Event log contains more than one 'run-header' event; at most one is permitted.");
                        }

                        run = sarifEvent.Payload.ToObject<Run>(serializer) ?? new Run();

                        // Header carries the skeleton only; clear anything that belongs in its
                        // own event. Producers that pre-populate these on a header are tolerated
                        // but their contents are discarded so that event-based authoring remains
                        // the source of truth.
                        run.Results = null;
                        run.Invocations = null;
                        headerSeen = true;
                        break;

                    case SarifEventKinds.Result:
                        results.Add(sarifEvent.Payload.ToObject<Result>(serializer));
                        break;

                    case SarifEventKinds.Invocation:
                        invocations.Add(sarifEvent.Payload.ToObject<Invocation>(serializer));
                        break;

                    case SarifEventKinds.Notification:
                        Notification notification = sarifEvent.Payload.ToObject<Notification>(serializer);
                        // AI execution-timeline consumers (AI2019) expect every notification to
                        // carry a UTC timestamp. Producers that already populated timeUtc keep
                        // their value; everyone else gets the moment of replay, which is close
                        // enough to "now" for any practical timeline reconstruction and avoids
                        // requiring every event author to remember to stamp manually.
                        if (notification != null && notification.TimeUtc == default(DateTime))
                        {
                            notification.TimeUtc = DateTime.UtcNow;
                        }
                        notifications.Add(notification);
                        break;

                    case SarifEventKinds.RuleDescriptor:
                        // Producer-supplied rule descriptor (NOVEL- only per emit-time gate).
                        // Buffered and merged into run.tool.driver.rules ahead of result-driven
                        // auto-registration so the explicit descriptor wins over the minimal
                        // synthesized one.
                        ruleDescriptors.Add(sarifEvent.Payload.ToObject<ReportingDescriptor>(serializer));
                        break;

                    case SarifEventKinds.NotificationDescriptor:
                        // Producer-supplied notification descriptor. Buffered and merged into
                        // run.tool.driver.notifications.
                        notificationDescriptors.Add(sarifEvent.Payload.ToObject<ReportingDescriptor>(serializer));
                        break;

                    // The reader filters unknown kinds; an unknown kind reaching us here is a
                    // contract violation.
                    default:
                        throw new SarifEventLogException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Unexpected event kind '{0}' reached the replayer.",
                                sarifEvent.Kind));
                }
            }

            run ??= new Run();
            run.Tool ??= new Tool();
            run.Tool.Driver ??= new ToolComponent { Name = "Unknown" };

            // Merge producer-supplied descriptors BEFORE result-driven auto-registration so the
            // explicit descriptors seed the idToIndex table — auto-registration only fills in
            // ids that aren't already represented.
            MergeDescriptors(
                existing: run.Tool.Driver.Rules,
                additions: ruleDescriptors,
                target: nameof(ToolComponent.Rules),
                assign: d => run.Tool.Driver.Rules = d);

            MergeDescriptors(
                existing: run.Tool.Driver.Notifications,
                additions: notificationDescriptors,
                target: nameof(ToolComponent.Notifications),
                assign: d => run.Tool.Driver.Notifications = d);

            RegisterDescriptorsFromResults(run, results);

            run.Results = results.Count > 0 ? results : null;
            run.Invocations = invocations.Count > 0 ? invocations : null;

            if (notifications.Count > 0)
            {
                run.Invocations ??= new List<Invocation>
                {
                    new Invocation { ExecutionSuccessful = true },
                };

                Invocation host = run.Invocations[run.Invocations.Count - 1];
                host.ToolExecutionNotifications ??= new List<Notification>();
                foreach (Notification notification in notifications)
                {
                    host.ToolExecutionNotifications.Add(notification);
                }
            }

            return new SarifLog
            {
                Runs = new List<Run> { run },
            };
        }

        /// <summary>
        /// Replays the event log and writes the resulting <see cref="SarifLog"/> atomically to
        /// <paramref name="destinationPath"/>.
        /// </summary>
        public static SarifLog ReplayToFile(string eventLogPath, string destinationPath, bool prettyPrint = true)
        {
            SarifLog log = Replay(eventLogPath);

            AtomicSarifWriter.Write(destinationPath, stream =>
            {
                using var sw = new StreamWriter(stream, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                using var jw = new JsonTextWriter(sw);
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = prettyPrint ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None,
                };
                JsonSerializer.Create(settings).Serialize(jw, log);
            });

            return log;
        }

        private static void RegisterDescriptorsFromResults(Run run, IList<Result> results)
        {
            // Reject every result whose ruleId violates the AI convention BEFORE we
            // start mutating the run's descriptor table. The thrown exception lists
            // every offender in one shot so an AI orchestrator can correct them all
            // in a single retry rather than discovering them one at a time.
            AIRuleIdConvention.ThrowIfAnyUnacceptable(results);

            // Build an index of already-registered descriptors so we don't duplicate.
            var idToIndex = new Dictionary<string, int>(StringComparer.Ordinal);

            run.Tool.Driver.Rules ??= new List<ReportingDescriptor>();
            for (int i = 0; i < run.Tool.Driver.Rules.Count; i++)
            {
                ReportingDescriptor existing = run.Tool.Driver.Rules[i];
                if (!string.IsNullOrEmpty(existing.Id))
                {
                    idToIndex[existing.Id] = i;
                }
            }

            foreach (Result result in results)
            {
                // result.RuleId is guaranteed non-empty here (AIRuleIdConvention.ThrowIfAnyUnacceptable
                // rejects empty / null ruleIds along with malformed ones).
                //
                // Per SARIF §3.49.3 descriptor ids are base-only. A hierarchical result.ruleId
                // such as "CWE-79/dom-xss-via-sanitizer-bypass" registers (or reuses) a
                // descriptor with the base id "CWE-79"; the full hierarchical form stays on
                // the result per §3.52.4 (and is what AI1012 expects from a well-shaped AI
                // finding). NOVEL- prefixed ruleIds are flat (no slash) and register a
                // descriptor with the full id. Producers that need a slash-bearing descriptor
                // id can pre-register it on the run header — the pre-registered entry wins
                // because the idToIndex seed above runs first.
                string descriptorId = result.RuleId;
                int slash = descriptorId.IndexOf('/');
                if (slash > 0)
                {
                    descriptorId = descriptorId.Substring(0, slash);
                }

                if (!idToIndex.TryGetValue(descriptorId, out int index))
                {
                    index = run.Tool.Driver.Rules.Count;
                    run.Tool.Driver.Rules.Add(new ReportingDescriptor { Id = descriptorId });
                    idToIndex[descriptorId] = index;
                }

                result.RuleIndex = index;
            }
        }

        /// <summary>
        /// Merges producer-supplied descriptors emitted as <c>rule-descriptor</c> /
        /// <c>notification-descriptor</c> events into the target list on the run's driver.
        /// </summary>
        /// <remarks>
        /// <para>Header pre-populated entries (if any) are preserved by reference, so a producer
        /// that supplied a descriptor on the run-header AND via an event for the same id is
        /// already a contract violation that the verb's emit-time dedup should have rejected.
        /// At replay we trust the invariant and append events after pre-populated entries; if
        /// the invariant is violated (e.g., a manually-edited event log) the resulting SARIF
        /// will carry two descriptors with the same id and the validator will flag it.</para>
        /// <para>For the rules array specifically, this method must run BEFORE
        /// <see cref="RegisterDescriptorsFromResults"/> so that the explicit descriptors seed
        /// the <c>idToIndex</c> table — auto-registration synthesizes minimal descriptors only
        /// for ids that aren't already represented.</para>
        /// </remarks>
        private static void MergeDescriptors(
            IList<ReportingDescriptor> existing,
            IList<ReportingDescriptor> additions,
            string target,
            Action<IList<ReportingDescriptor>> assign)
        {
            if (additions == null || additions.Count == 0)
            {
                return;
            }

            if (existing == null)
            {
                existing = new List<ReportingDescriptor>();
                assign(existing);
            }

            foreach (ReportingDescriptor descriptor in additions)
            {
                if (descriptor != null)
                {
                    existing.Add(descriptor);
                }
            }
        }
    }
}
