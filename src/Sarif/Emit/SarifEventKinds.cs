// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Canonical event kinds for the JSONL event log that backs incremental SARIF authoring.
    /// </summary>
    /// <remarks>
    /// The event log shape is:
    /// <code>{"v":1,"kind":"&lt;kind&gt;","payload":{ ... }}</code>
    /// Readers MAY skip unknown <c>kind</c> values when the schema version <c>v</c> is supported;
    /// readers MUST fail when <c>v</c> is unknown for a known <c>kind</c>.
    /// </remarks>
    public static class SarifEventKinds
    {
        /// <summary>
        /// A partial <see cref="Run"/> skeleton (everything except <c>results</c>, <c>invocations</c>,
        /// and <c>notifications</c>, which arrive as separate events). MUST appear at most once per
        /// event log; SHOULD be the first event in the log.
        /// </summary>
        public const string RunHeader = "run-header";

        /// <summary>
        /// A single self-contained <see cref="Result"/>. Self-contained means the result
        /// SHALL NOT carry index references (<c>ruleIndex</c>, <c>artifactLocation.index</c>,
        /// etc.) into run-level caches. Use <c>ruleId</c> rather than <c>ruleIndex</c>; the
        /// replay engine auto-registers descriptors keyed by <see cref="Result.RuleId"/>.
        /// </summary>
        public const string Result = "result";

        /// <summary>
        /// A single self-contained <see cref="Notification"/>. The replay engine appends it to
        /// <c>invocations[last].toolExecutionNotifications</c> by default.
        /// </summary>
        public const string Notification = "notification";

        /// <summary>
        /// A complete <see cref="Invocation"/> object. Producers may append multiple
        /// invocations per run.
        /// </summary>
        public const string Invocation = "invocation";

        /// <summary>
        /// A single <see cref="ReportingDescriptor"/> targeted at <c>run.tool.driver.rules</c>.
        /// Emitted by the <c>add-reporting-descriptor --rules</c> verb. The replayer appends the
        /// descriptor to the rules list before result-driven auto-registration runs, so an
        /// explicitly-supplied descriptor wins over the minimal one that would otherwise be
        /// synthesized from a result's <c>ruleId</c>. The verb enforces
        /// <see cref="AIRuleIdConvention.IsNovel(string)"/> on the descriptor id — this kind is
        /// reserved for NOVEL- novel-finding descriptors. Taxonomy-mapped descriptors (e.g.,
        /// <c>CWE-89</c>) come from the taxonomy enricher, not from this event.
        /// </summary>
        public const string RuleDescriptor = "rule-descriptor";

        /// <summary>
        /// A single <see cref="ReportingDescriptor"/> targeted at
        /// <c>run.tool.driver.notifications</c>. Emitted by the <c>add-reporting-descriptor</c>
        /// verb (default target). Notifications use opaque ids by convention (e.g.,
        /// <c>progress</c>, <c>config-error</c>) and carry no convention gate — any non-empty id
        /// is accepted. The replayer appends the descriptor to the notifications list verbatim.
        /// </summary>
        public const string NotificationDescriptor = "notification-descriptor";

        /// <summary>The current event-log schema version.</summary>
        public const int CurrentSchemaVersion = 1;
    }
}
