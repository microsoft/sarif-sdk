// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ConsoleLogger : IAnalysisLogger
    {
        public ConsoleLogger(bool verbose)
        {
            Verbose = verbose;
        }

        public bool Verbose { get; set; }


        public void AnalysisStarted()
        {
            Console.WriteLine(SdkResources.MSG_Analyzing);
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            Console.WriteLine();

            if (runtimeConditions == RuntimeConditions.NoErrors)
            {
                Console.WriteLine(SdkResources.MSG_AnalysisCompletedSuccessfully);
                return;
            }

            if ((runtimeConditions & RuntimeConditions.Fatal) != 0)
            {
                // One or more fatal conditions observed at runtime, so
                // we'll report a catastrophic exit (withuot paying
                // particular attention to anything non-fatal
                Console.WriteLine(SdkResources.MSG_UnexpectedApplicationExit);
            }
            else
            {
                // Analysis finished but was not complete due
                // to non-fatal runtime errors.
                Console.WriteLine(SdkResources.MSG_AnalysisIncomplete);
            }

            Console.WriteLine("Unexpected runtime condition(s) observed: " + runtimeConditions.ToString());
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            if (this.Verbose)
            {
                Console.WriteLine(string.Format(
                    SdkResources.MSG1001_AnalyzingTarget,
                        Path.GetFileName(context.TargetUri.LocalPath)));
            }
        }

        public void LogMessage(bool verbose, string message)
        {
            if (this.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        public void Log(IRule rule, Result result)
        {
            string message = result.GetMessageText(rule, concise: false);

            // TODO we need better retrieval for locations than these defaults
            // Note that we can potentially emit many messages from a single result
            PhysicalLocation physicalLocation = result.Locations?.First().ResultFile ?? result.Locations?.First().AnalysisTarget;
            WriteToConsole(
                result.Level,
                physicalLocation?.Uri,
                physicalLocation?.Region,
                result.RuleId,
                message);
        }

        private void WriteToConsole(ResultLevel level, Uri uri, Region region, string ruleId, string message)
        {
            switch (level)
            {
                // These result types are optionally emitted.
                case ResultLevel.Pass:
                case ResultLevel.Note:
                case ResultLevel.NotApplicable:
                    {
                        if (Verbose)
                        {
                            Console.WriteLine(GetMessageText(uri, region, ruleId, message, level));
                        }
                        break;
                    }

                // These result types are always emitted.
                case ResultLevel.Error:
                case ResultLevel.Warning:
                    {
                        Console.WriteLine(GetMessageText(uri, region, ruleId, message, level));
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        private static string GetMessageText(
            Uri uri,
            Region region,
            string ruleId,
            string message,
            ResultLevel resultLevel)
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

            string issueType = null;

            switch (resultLevel)
            {
                case ResultLevel.Error:
                    issueType = "error";
                    break;

                case ResultLevel.Warning:
                    issueType = "warning";
                    break;

                case ResultLevel.NotApplicable:
                case ResultLevel.Note:
                case ResultLevel.Pass:
                    issueType = "info";
                    break;

                default:
                    throw new InvalidOperationException("Unknown message kind:" + resultLevel.ToString());
            }

            string detailedDiagnosis = NormalizeMessage(message, enquote: false);

            string location = "";

            if (region != null)
            {
                // TODO 
                if (region.Offset > 0 ||
                    region.StartColumn == 0)
                {
                    throw new NotImplementedException();
                }

                if (region.StartLine == 0)
                {
                    throw new InvalidOperationException();
                }

                location = region.FormatForVisualStudio();
            }

            string result = (path != null ? (path + location + ": ") : "") +
                   issueType + (!string.IsNullOrEmpty(ruleId) ? " " : "") +
                   (resultLevel != ResultLevel.Note ? ruleId : "") + ": " +
                   detailedDiagnosis;

            return result;
        }

        public static string NormalizeMessage(string message, bool enquote)
        {
            return (enquote ? "\"" : "") +
                message.Replace('"', '\'') +
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
                case NotificationLevel.Note:
                    if (Verbose)
                    {
                        Console.WriteLine(FormatNotificationMessage(notification));
                    }
                    break;

                // These notification types are always emitted.
                case NotificationLevel.Error:
                case NotificationLevel.Warning:
                    Console.WriteLine(FormatNotificationMessage(notification));
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private string FormatNotificationMessage(Notification notification)
        {
            string issueType = null;

            switch (notification.Level)
            {
                case NotificationLevel.Error:
                case NotificationLevel.Warning:
                case NotificationLevel.Note:
                    issueType = notification.Level.ToString();
                    issueType = issueType.Substring(0, 1).ToLowerInvariant() + issueType.Substring(1);
                    break;

                default:
                    throw new InvalidOperationException("Unknown notification level: " + notification.Level);
            }

            var sb = new StringBuilder(issueType);

            if (!string.IsNullOrEmpty(notification.Id))
            {
                sb.Append($" {notification.Id}: ");
            }

            if (!string.IsNullOrEmpty(notification.RuleId))
            {
                sb.Append($"{notification.RuleId}: ");
            }

            sb.Append(notification.Message);

            return sb.ToString();
        }
    }
}