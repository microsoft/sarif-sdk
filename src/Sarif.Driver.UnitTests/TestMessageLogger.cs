// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal class TestMessageLogger : IAnalysisLogger
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
        }

        public void Log(IRule rule, Result result)
        {
            NoteTestResult(result.Level, result.Locations.First().AnalysisTarget.Uri.LocalPath);
        }

        public void NoteTestResult(ResultLevel level, string targetPath)
        {
            switch (level)
            {
                case ResultLevel.Pass:
                {
                    PassTargets.Add(targetPath);
                    break;
                }

                case ResultLevel.Error:
                {
                    FailTargets.Add(targetPath);
                    break;
                }

                case ResultLevel.NotApplicable:
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
            throw new NotImplementedException();
        }

        public void LogConfigurationNotification(Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}