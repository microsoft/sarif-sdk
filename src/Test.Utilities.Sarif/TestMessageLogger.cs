// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class TestMessageLogger : IAnalysisLogger
    {
        public TestMessageLogger()
        {
            FailTargets = new HashSet<string>();
            PassTargets = new HashSet<string>();
            NotApplicableTargets = new HashSet<string>();
        }

        public RuntimeConditions RuntimeErrors { get; set; }

        public HashSet<string> PassTargets { get; set; }

        public HashSet<string> FailTargets { get; set; }

        public HashSet<string> NotApplicableTargets { get; set; }

        public List<string> Messages { get; set; }

        public List<Notification> ToolNotifications { get; set; }

        public List<Notification> ConfigurationNotifications { get; set; }

        public void AnalysisStarted()
        {
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            RuntimeErrors = runtimeConditions;
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
        }

        public void LogMessage(bool verbose, string message)
        {
            Messages = Messages ?? new List<string>();
            Messages.Add(message);
        }

        public void Log(ReportingDescriptor rule, Result result)
        {
            NoteTestResult(result.Kind, result.Locations.First().PhysicalLocation.ArtifactLocation.Uri.LocalPath);
        }

        public void NoteTestResult(ResultKind kind, string targetPath)
        {
            switch (kind)
            {
                case ResultKind.Pass:
                {
                    PassTargets.Add(targetPath);
                    break;
                }

                case ResultKind.Fail:
                {
                    FailTargets.Add(targetPath);
                    break;
                }

                case ResultKind.NotApplicable:
                {
                    NotApplicableTargets.Add(targetPath);
                    break;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void LogToolNotification(Notification notification)
        {
            ToolNotifications = ToolNotifications ?? new List<Notification>();
            ToolNotifications.Add(notification);
        }

        public void LogConfigurationNotification(Notification notification)
        {
            ConfigurationNotifications = ConfigurationNotifications ?? new List<Notification>();
            ConfigurationNotifications.Add(notification);
        }
    }
}