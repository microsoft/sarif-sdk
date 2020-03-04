// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ConsoleLogger : IAnalysisLogger
    {
        public ConsoleLogger(bool verbose, string toolName)
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
            RuntimeConditions fatalConditions = (runtimeConditions & ~RuntimeConditions.Nonfatal);

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

        public void LogMessage(bool verbose, string message)
        {
            if (Verbose)
            {
                WriteLineToConsole(message);
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

            WriteToConsole(
                result.Kind,
                result.Level,
                physicalLocation?.ArtifactLocation?.Uri,
                physicalLocation?.Region,
                result.RuleId,
                message);
        }

        private void WriteToConsole(ResultKind kind, FailureLevel level, Uri uri, Region region, string ruleId, string message)
        {
            ValidateKindAndLevel(kind, level);

            switch (level)
            {
                // These result types are optionally emitted.
                case FailureLevel.None:
                case FailureLevel.Note:
                {
                    if (Verbose)
                    {
                        WriteLineToConsole(GetMessageText(_toolName, uri, region, ruleId, message, kind, level));
                    }
                    break;
                }

                // These result types are always emitted.
                case FailureLevel.Error:
                case FailureLevel.Warning:
                {
                    WriteLineToConsole(GetMessageText(_toolName, uri, region, ruleId, message, kind, level));
                    break;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private static void ValidateKindAndLevel(ResultKind kind, FailureLevel level)
        {
            if (level != FailureLevel.None && kind != ResultKind.Fail)
            {
                throw new ArgumentException("Level indicated a failure but kind was not set to 'Fail'.");
            }

            if (level == FailureLevel.None && kind == ResultKind.Fail)
            {
                throw new ArgumentException("Level did not indicate a failure but kind was set to 'Fail'.");
            }
            return;
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
            string path = null;

            ValidateKindAndLevel(kind, level);

            path = ConstructPathFromUri(uri);

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
                    // Shorten to 'info' for compat reasons with previous behaviors.
                    if (issueType == "informational") { issueType = "info"; };
                    break;


                default:
                    throw new InvalidOperationException("Unknown message level:" + level.ToString());
            }

            string detailedDiagnosis = NormalizeMessage(message, enquote: false);

            string location = "";

            if (region != null)
            {
                // TODO 
                if (region.CharOffset > 0 ||
                    region.ByteOffset > 0 ||
                    region.StartColumn == 0)
                {
                    return string.Empty;
                }

                if (region.StartLine == 0)
                {
                    throw new InvalidOperationException();
                }

                location = region.FormatForVisualStudio();
            }

            string messageText =
                   (path != null ? (path + location) : toolName) + ": " +
                   issueType + (!string.IsNullOrEmpty(ruleId) ? " " : "") +
                   (!string.IsNullOrEmpty(ruleId) ? (ruleId + ": ") : "") +
                   detailedDiagnosis;

            return messageText;
        }

        public static string NormalizeMessage(string message, bool enquote)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return (enquote ? "\"" : "") +
                message +
                (enquote ? "\"" : "");
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

            sb.Append(issueType + " ");

            if (!string.IsNullOrEmpty(notification.Descriptor?.Id))
            {
                sb.Append(notification.Descriptor.Id + " : ");
            }

            if (!string.IsNullOrEmpty(notification.AssociatedRule?.Id))
            {
                sb.Append(notification.AssociatedRule.Id + " : ");
            }

            sb.Append(notification.Message.Text);

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