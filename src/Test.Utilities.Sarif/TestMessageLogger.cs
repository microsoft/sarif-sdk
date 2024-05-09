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
            Results = new List<Tuple<ReportingDescriptor, Result>>();
        }

        public int AnalyzingTargetCount { get; set; }

        public int TargetAnalyzedCount { get; set; }


        public List<Tuple<ReportingDescriptor, Result>> Results { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public HashSet<string> PassTargets { get; set; }

        public HashSet<string> FailTargets { get; set; }

        public HashSet<string> NotApplicableTargets { get; set; }

        public List<string> Messages { get; set; }

        public List<Notification> ToolNotifications { get; set; }

        public List<Notification> ConfigurationNotifications { get; set; }

        public FileRegionsCache FileRegionsCache { get; set; }

        public void AnalysisStarted()
        {
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            RuntimeErrors = runtimeConditions;
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            AnalyzingTargetCount++;
        }

        public void TargetAnalyzed(IAnalysisContext context)
        {
            TargetAnalyzedCount++;
        }

        public void Log(IAnalysisContext context, ReportingDescriptor rule, Result result, int? extensionIndex)
        {
            Results.Add(new Tuple<ReportingDescriptor, Result>(rule, result));
            NoteTestResult(result.Kind, result.Locations.First().PhysicalLocation.ArtifactLocation.Uri.ToString());
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

        public void LogToolNotification(Notification notification, ReportingDescriptor associatedRule)
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
