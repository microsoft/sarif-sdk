// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
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
            RuntimeConditions fatalConditions = (runtimeConditions & ~RuntimeConditions.Nonfatal);

            if (fatalConditions == RuntimeConditions.None)
            {
                Console.WriteLine(SdkResources.MSG_AnalysisCompletedSuccessfully);
            }

            Console.WriteLine();

            if ((runtimeConditions & RuntimeConditions.RuleNotApplicableToTarget) != 0)
            {
                Console.WriteLine(SdkResources.MSG_OneOrMoreNotApplicable);
                Console.WriteLine();
            }

            if ((runtimeConditions & RuntimeConditions.TargetNotValidToAnalyze) != 0)
            {
                Console.WriteLine(SdkResources.MSG_OneOrMoreInvalidTargets);
                Console.WriteLine();
            }

            if (fatalConditions != 0)
            {
                // One or more fatal conditions observed at runtime,
                // so we'll report a catastrophic exit.
                Console.WriteLine(SdkResources.MSG_UnexpectedApplicationExit);
                Console.WriteLine(SdkResources.UnexpectedFatalRuntimeConditions + fatalConditions.ToString());
                Console.WriteLine();
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
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    SdkResources.MSG001_AnalyzingTarget,
                        context.TargetUri.GetFileName()));
            }
        }

        public void LogMessage(bool verbose, string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
            }
        }

        public void Log(IRule rule, Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

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

                case ResultLevel.Pass:
                    issueType = "pass";
                    break;

                case ResultLevel.NotApplicable:
                case ResultLevel.Note:
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
                   (!string.IsNullOrEmpty(ruleId) ? (ruleId + ": ") : "") +
                   detailedDiagnosis;

            return result;
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
                {
                    issueType = "error";
                    break;
                }
                case NotificationLevel.Warning:
                {
                    issueType = "warning";
                    break;
                }
                case NotificationLevel.Note:
                {
                    issueType = "note";
                    break;
                }

                default:
                throw new InvalidOperationException("Unknown notification level: " + notification.Level);
            }

            var sb = new StringBuilder(issueType + " ");

            if (!string.IsNullOrEmpty(notification.Id))
            {
                sb.Append(notification.Id + " : ");
            }

            if (!string.IsNullOrEmpty(notification.RuleId))
            {
                sb.Append(notification.RuleId + " : ");
            }

            sb.Append(notification.Message);

            return sb.ToString();
        }
    }
}