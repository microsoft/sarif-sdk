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
        /// A complete <see cref="Invocation"/> object. Producer-supplied <see cref="Notification"/>
        /// objects travel inline on the invocation's <c>toolExecutionNotifications</c> /
        /// <c>toolConfigurationNotifications</c> arrays.
        /// </summary>
        public const string Invocation = "invocation";

        /// <summary>
        /// A <see cref="ReportingDescriptor"/> targeted at <c>run.tool.driver.rules</c>.
        /// Explicit descriptors are merged before result-driven auto-registration and are
        /// reserved for NOVEL- ruleIds.
        /// </summary>
        public const string RuleDescriptor = "rule-descriptor";

        /// <summary>
        /// A <see cref="ReportingDescriptor"/> targeted at <c>run.tool.driver.notifications</c>.
        /// Notification descriptor ids are opaque non-empty strings.
        /// </summary>
        public const string NotificationDescriptor = "notification-descriptor";

        /// <summary>The current event-log schema version.</summary>
        public const int CurrentSchemaVersion = 1;
    }
}
