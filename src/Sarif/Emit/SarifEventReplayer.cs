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
    /// directly).</description></item>
    /// <item><description><c>invocation</c> events are appended to <c>run.invocations</c> in
    /// event order.</description></item>
    /// <item><description><c>notification</c> events are buffered and attached at finalize to
    /// <c>run.invocations[last].toolExecutionNotifications</c>. If no invocation has been
    /// supplied, a synthetic <c>{ "executionSuccessful": true }</c> invocation is created to
    /// hold them (SARIF requires a home for notifications).</description></item>
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
                        notifications.Add(sarifEvent.Payload.ToObject<Notification>(serializer));
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
                if (string.IsNullOrEmpty(result.RuleId))
                {
                    // RuleId-less result: leave RuleIndex at its default sentinel (-1). The SDK
                    // and SARIF2004 cleanup handle that shape.
                    result.RuleIndex = -1;
                    continue;
                }

                if (!idToIndex.TryGetValue(result.RuleId, out int index))
                {
                    index = run.Tool.Driver.Rules.Count;
                    run.Tool.Driver.Rules.Add(new ReportingDescriptor { Id = result.RuleId });
                    idToIndex[result.RuleId] = index;
                }

                result.RuleIndex = index;
            }
        }
    }
}
