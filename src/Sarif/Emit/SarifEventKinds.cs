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
        /// A complete <see cref="Invocation"/> object. Producers may append multiple invocations
        /// per run (SARIF <c>run.invocations[]</c> is an array, so parallel/overlapping processes
        /// are each modeled by their own self-contained invocation). Producer-supplied
        /// <see cref="Notification"/> objects travel INLINE on the invocation's
        /// <c>toolExecutionNotifications</c> / <c>toolConfigurationNotifications</c> arrays; there
        /// is no separate notification event kind.
        /// </summary>
        public const string Invocation = "invocation";

        /// <summary>
        /// A single <see cref="ReportingDescriptor"/> targeted at <c>run.tool.driver.rules</c>.
        /// Emitted by the <c>add-reporting-descriptor --rules</c> verb. The replayer appends the
        /// descriptor to the rules list before result-driven auto-registration, so an explicit
        /// descriptor takes precedence over an auto-synthesized one. The verb gates the descriptor
        /// id on the NOVEL- grammar — the lowercase-kebab form a result's NOVEL- <c>ruleId</c>
        /// uses, so the id equals the ruleId that references it. This kind is reserved for
        /// novel-finding descriptors; taxonomy-mapped descriptors (e.g., <c>CWE-89</c>) come from
        /// the taxonomy enricher, not from this event.
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
