// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class AddNotificationTool
    {
        private readonly SarifRunStore _store;

        public AddNotificationTool(SarifRunStore store) => this._store = store;

        [McpServerTool(Name = "sarif_add_notification")]
        [Description(
            "Add an execution or configuration notification to a SARIF run invocation. " +
            "Execution notifications capture scan narrative; configuration notifications " +
            "capture settings and environment feedback. The kind parameter determines " +
            "which list the notification lands in (toolExecutionNotifications vs " +
            "toolConfigurationNotifications).")]
        public string AddNotification(
            [Description("Run GUID from sarif_create_run")] string runGuid,
            [Description("Notification descriptor ID (e.g., 'SCAN-STARTED', 'MODEL-SELECTED')")] string descriptorId,
            [Description("Plain-text notification summary (one sentence)")] string message,
            [Description("Rich markdown notification narrative")] string messageMarkdown,
            [Description("Notification kind: execution or configuration")] string kind = "execution",
            [Description("Severity: error, warning, or note")] string level = "note",
            [Description("[ISO8601:2004] timestamp (defaults to now)")] string? timeUtc = null,
            [Description("Invocation index from sarif_start_invocation. Defaults to the most recent invocation.")] int? invocationIndex = null)
        {
            SarifRunContext ctx = this._store.Get(runGuid);
            ctx.ThrowIfFinalized();

            FailureLevel failureLevel = level.ToLowerInvariant() switch
            {
                "error" => FailureLevel.Error,
                "warning" => FailureLevel.Warning,
                _ => FailureLevel.Note
            };

            int descriptorIndex = ctx.EnsureNotificationDescriptor(descriptorId, failureLevel);

            DateTime resolvedTimeUtc = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(timeUtc) && DateTime.TryParse(
                    timeUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out DateTime parsedTime))
            {
                resolvedTimeUtc = parsedTime;
            }

            var notification = new Notification
            {
                Descriptor = new ReportingDescriptorReference
                {
                    Id = descriptorId,
                    Index = descriptorIndex
                },
                Level = failureLevel,
                Message = new Message { Text = message, Markdown = messageMarkdown },
                TimeUtc = resolvedTimeUtc
            };

            bool isConfig = kind.Equals("configuration", StringComparison.OrdinalIgnoreCase);

            ctx.AddNotification(notification, isConfig, invocationIndex);

            return JsonSerializer.Serialize(new { status = "added", descriptorId });
        }
    }
}
