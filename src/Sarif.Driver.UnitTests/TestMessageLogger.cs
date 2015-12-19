// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    internal class TestMessageLogger : IResultLogger
    {
        public TestMessageLogger()
        {
            FailTargets = new HashSet<string>();
            PassTargets = new HashSet<string>();
            NotApplicableTargets = new HashSet<string>();
        }

        public HashSet<string> PassTargets { get; set; }

        public HashSet<string> FailTargets { get; set; }

        public HashSet<string> NotApplicableTargets { get; set; }

        public void Log(ResultKind messageKind, string formatSpecifier, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void Log(ResultKind messageKind, IAnalysisContext context, string formatSpecifierId, params string[] arguments)
        {
            NoteTestResult(messageKind, context.TargetUri.LocalPath);
        }

        public void NoteTestResult(ResultKind messageKind, string targetPath)
        {
            switch (messageKind)
            {
                case ResultKind.Pass:
                {
                    PassTargets.Add(targetPath);
                    break;
                }

                case ResultKind.Error:
                {
                    FailTargets.Add(targetPath);
                    break;
                }

                case ResultKind.NotApplicable:
                {
                    NotApplicableTargets.Add(targetPath);
                    break;
                }

                case ResultKind.Note:
                case ResultKind.InternalError:
                case ResultKind.ConfigurationError:
                {
                    throw new NotImplementedException();
                }
                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}