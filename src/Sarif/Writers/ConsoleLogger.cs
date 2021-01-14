// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class ConsoleLogger : BaseLogger, IAnalysisLogger
    {
        public ConsoleLogger(bool verbose, string toolName, IEnumerable<FailureLevel> level = null, IEnumerable<ResultKind> kind = null) : base(level, kind)
        {
            Verbose = verbose;
            _toolName = toolName.ToUpperInvariant();
        }

        private readonly string _toolName;
        private StringBuilder _capturedOutput;

        public bool CaptureOutput { get; set; }

        public string CapturedOutput => _capturedOutput?.ToString();

        public bool Verbose { get; set; }

        private void WriteLineToConsole(string text = null)
        {
            Console.WriteLine(text);

            if (CaptureOutput)
            {
                _capturedOutput = _capturedOutput ?? new StringBuilder();
                _capturedOutput.AppendLine(text);
            }
        }

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
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (Verbose)
            {
                WriteLineToConsole(string.Format(CultureInfo.CurrentCulture,
                    SdkResources.MSG001_AnalyzingTarget,
                        context.TargetUri.GetFileName()));
            }
        }

        public void Log(ReportingDescriptor rule, Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            string message = result.GetMessageText(rule);

            // TODO we need better retrieval for locations than these defaults.
            // Note that we can potentially emit many messages from a single result.
            PhysicalLocation physicalLocation = result.Locations?.First().PhysicalLocation;

            if (!ShouldLog(result))
            {
                return;
            }

            WriteLineToConsole(GetMessageText(_toolName, physicalLocation?.ArtifactLocation?.Uri, physicalLocation?.Region, result.RuleId, message, result.Kind, result.Level));
        }

        private static string GetMessageText(
            string toolName,
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
                    if (issueType == "informational") { issueType = "info"; }
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

            return (path != null ? (path + location) : toolName)
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

        public void LogToolNotification(Notification notification)
        {
            WriteToConsole(notification);
        }

        public void LogConfigurationNotification(Notification notification)
        {
            WriteToConsole(notification);
        }

        private void WriteToConsole(Notification notification)
        {
            switch (notification.Level)
            {
                // This notification type is optionally emitted.
                case FailureLevel.None:
                case FailureLevel.Note:
                    if (Verbose)
                    {
                        WriteLineToConsole(FormatNotificationMessage(notification, _toolName));
                    }
                    break;

                // These notification types are always emitted.
                case FailureLevel.Error:
                case FailureLevel.Warning:
                    WriteLineToConsole(FormatNotificationMessage(notification, _toolName));
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

            var sb = new StringBuilder((ConstructPathFromUri(uri) ?? toolName) + " : ");

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

            return path;
        }
    }
}
