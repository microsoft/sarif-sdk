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
    /// Header <c>results</c>, <c>invocations</c>, and <c>notifications</c> are ignored.</description></item>
    /// <item><description><c>result</c> events MUST be self-contained. <c>ruleIndex</c> is
    /// re-derived from <c>ruleId</c>, and every <see cref="Result.RuleId"/> MUST conform to
    /// <see cref="AIRuleIdConvention"/>.</description></item>
    /// <item><description><c>invocation</c> events are appended to <c>run.invocations</c> in
    /// event order and replayed verbatim.</description></item>
    /// </list>
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

                        // Header is the run skeleton; result and invocation events are authoritative.
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

                    case SarifEventKinds.RuleDescriptor:
                        // Merge explicit descriptors before result-driven auto-registration.
                        ruleDescriptors.Add(sarifEvent.Payload.ToObject<ReportingDescriptor>(serializer));
                        break;

                    case SarifEventKinds.NotificationDescriptor:
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

            // Explicit descriptors seed the idToIndex table before auto-registration.
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
            // Validate before mutating the descriptor table.
            AIRuleIdConvention.ThrowIfAnyUnacceptable(results);

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
                // Per SARIF §3.49.3 descriptor ids are base-only. A hierarchical result.ruleId
                // such as "CWE-79/dom-xss-via-sanitizer-bypass" registers or reuses descriptor
                // "CWE-79"; the full hierarchical form stays on the result per §3.52.4. NOVEL-
                // ruleIds are flat and register with the full id.
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
        /// Header entries are preserved by reference, and descriptor events are appended after
        /// them. For rules, this method must run before <see cref="RegisterDescriptorsFromResults"/>
        /// so explicit descriptors seed the <c>idToIndex</c> table.
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
