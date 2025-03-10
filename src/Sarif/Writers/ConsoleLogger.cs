﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class ConsoleLogger(bool quietConsole, string toolName, FailureLevelSet levels = null, ResultKindSet kinds = null)
        : BaseLogger(levels, kinds), IAnalysisLogger
    {
        private readonly string _toolName = toolName.ToUpperInvariant();
    private StringBuilder _capturedOutput;

    public bool CaptureOutput { get; set; }

    public string CapturedOutput => _capturedOutput?.ToString();

    private readonly bool _quietConsole = quietConsole;

    private void WriteLineToConsole(string text = null, bool forceEmitOfErrorNotifications = false)
    {
        if (!_quietConsole || forceEmitOfErrorNotifications)
        {
            Console.WriteLine(text);

            if (CaptureOutput)
            {
                _capturedOutput ??= new StringBuilder();
                _capturedOutput.AppendLine(text);
            }
        }
    }

    public FileRegionsCache FileRegionsCache { get; set; }

    public void AnalysisStarted()
    {
        WriteLineToConsole(SdkResources.MSG_Analyzing);
    }

    public void AnalysisStopped(RuntimeConditions runtimeConditions)
    {
        RuntimeConditions fatalConditions = runtimeConditions & ~RuntimeConditions.Nonfatal;

        if (fatalConditions == RuntimeConditions.None)
        {
            WriteLineToConsole(SdkResources.MSG_AnalysisCompletedSuccessfully);
        }

        WriteLineToConsole();

        if ((runtimeConditions & RuntimeConditions.RuleNotApplicableToTarget) != 0)
        {
            WriteLineToConsole(SdkResources.MSG_OneOrMoreNotApplicable);
            WriteLineToConsole();
        }

        if ((runtimeConditions & RuntimeConditions.TargetNotValidToAnalyze) != 0)
        {
            WriteLineToConsole(SdkResources.MSG_OneOrMoreInvalidTargets);
            WriteLineToConsole();
        }

        if (fatalConditions != 0)
        {
            // One or more fatal conditions observed at runtime,
            // so we'll report a catastrophic exit.
            WriteLineToConsole(SdkResources.MSG_UnexpectedApplicationExit);
            WriteLineToConsole(SdkResources.UnexpectedFatalRuntimeConditions + fatalConditions.ToString());
            WriteLineToConsole();
        }
    }

    public void AnalyzingTarget(IAnalysisContext context)
    {
        if (context.Traces.Contains(nameof(DefaultTraces.TargetsScanned)))
        {
            // Analyzing '{0}'.
            WriteLineToConsole(string.Format(CultureInfo.CurrentCulture,
                    SdkResources.MSG001_AnalyzingTarget,
                        context.CurrentTarget.Uri.GetFileName()));
        }
    }

    public void TargetAnalyzed(IAnalysisContext context)
    {
        if (context.Traces.Contains(nameof(DefaultTraces.TargetsScanned)))
        {
            // Completed: '{0}'.
            WriteLineToConsole(string.Format(CultureInfo.CurrentCulture,
                    SdkResources.MSG001_TargetAnalyzed,
                        context.CurrentTarget.Uri.GetFileName()));
        }
    }

    public void Log(ReportingDescriptor rule, Result result, int? extensionIndex = null)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (!ShouldLog(result) || _quietConsole)
        {
            return;
        }

        string message = result.GetMessageText(rule);

        // TODO we need better retrieval for locations than these defaults.
        // Note that we can potentially emit many messages from a single result.
        PhysicalLocation physicalLocation = result.Locations?.First().PhysicalLocation;

        WriteLineToConsole(GetMessageText(_toolName, physicalLocation?.ArtifactLocation?.Uri, physicalLocation?.Region, result.RuleId, message, result.Kind, result.Level));
    }

    public static string GetMessageText(string toolName,
                                        Uri uri,
                                        Region region,
                                        string ruleId,
                                        string message,
                                        ResultKind kind,
                                        FailureLevel level)
    {
        string path = ConstructPathFromUri(uri);

        string issueType = null;

        switch (level)
        {
            case FailureLevel.Note:
                issueType = "note";
                break;

            case FailureLevel.Error:
                issueType = "error";
                break;

            case FailureLevel.Warning:
                issueType = "warning";
                break;

            case FailureLevel.None:
                issueType = kind.ToString().ToLowerInvariant();
                // Shorten to 'info' for compatibility with previous behavior.
                if (issueType == "informational" || issueType == "fail") { issueType = "info"; }
                break;

            default:
                throw new InvalidOperationException("Unknown message level:" + level.ToString());
        }

        string detailedDiagnosis = NormalizeMessage(message, enquote: false);

        string location = "";

        if (region != null)
        {
            // TODO: FormatForVisualStudio doesn't handle
            // binary and char offsets only.
            location = region.FormatForVisualStudio();
        }

        return (!string.IsNullOrWhiteSpace(path) ? (path + location) : toolName)
               + $": {issueType} "
               + (!string.IsNullOrEmpty(ruleId) ? (ruleId + ": ") : "")
               + detailedDiagnosis;
    }

    public static string NormalizeMessage(string message, bool enquote)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return (enquote ? "\"" : "") + message + (enquote ? "\"" : "");
    }

    public void LogToolNotification(Notification notification, ReportingDescriptor associatedRule)
    {
        if (!ShouldLog(notification))
        {
            return;
        }

        WriteToConsole(notification);
    }

    public void LogConfigurationNotification(Notification notification)
    {
        if (!ShouldLog(notification))
        {
            return;
        }

        WriteToConsole(notification);
    }

    private void WriteToConsole(Notification notification)
    {
        switch (notification.Level)
        {
            // This notification type is optionally emitted.
            case FailureLevel.None:
            case FailureLevel.Note:
            case FailureLevel.Warning:
                WriteLineToConsole(FormatNotificationMessage(notification, _toolName));
                break;
            case FailureLevel.Error:
                WriteLineToConsole(FormatNotificationMessage(notification, _toolName), forceEmitOfErrorNotifications: true);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    static internal string FormatNotificationMessage(Notification notification, string toolName)
    {
        string issueType = null;

        switch (notification.Level)
        {
            case FailureLevel.Error:
            {
                issueType = "error";
                break;
            }
            case FailureLevel.Warning:
            {
                issueType = "warning";
                break;
            }
            case FailureLevel.Note:
            {
                issueType = "note";
                break;
            }
            case FailureLevel.None:
            {
                issueType = "info";
                break;
            }

            default:
                throw new InvalidOperationException("Unknown notification level: " + notification.Level);
        }

        // TODO we need better retrieval for locations than these defaults.
        // Note that we can potentially emit many messages from a single result.
        PhysicalLocation physicalLocation = notification.Locations?.First().PhysicalLocation;
        Uri uri = physicalLocation?.ArtifactLocation?.Uri;

        string path = ConstructPathFromUri(uri);
        var sb = new StringBuilder((string.IsNullOrWhiteSpace(path) ? toolName : path) + " : ");

        sb.Append(issueType).Append(' ');

        if (!string.IsNullOrEmpty(notification.Descriptor?.Id))
        {
            sb.Append(notification.Descriptor.Id).Append(" : ");
        }

        if (!string.IsNullOrEmpty(notification.AssociatedRule?.Id))
        {
            sb.Append(notification.AssociatedRule.Id).Append(" : ");
        }

        sb.Append(notification.Message.Text);

        if (notification.Exception != null)
        {
            sb.AppendLine();
            sb.AppendLine(notification.Exception.ToString());
        }

        return sb.ToString();
    }

    private static string ConstructPathFromUri(Uri uri)
    {
        string path = null;

        if (uri != null)
        {
            // If a path refers to a URI of form file://blah, we will convert to the local path           
            if (uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeFile)
            {
                path = uri.LocalPath;
            }
            else
            {
                path = uri.ToString();
            }
        }

        string suffix = uri?.IsAbsoluteUri == true ? uri.Query : null;
        return $"{path}{suffix}";
    }
}
}
