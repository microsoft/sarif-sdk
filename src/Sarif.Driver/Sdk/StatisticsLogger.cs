// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class StatisticsLogger : IResultLogger
    {
        private Stopwatch _stopwatch;
        private long _targetsCount;
        private long _invalidTargetsCount;

        public StatisticsLogger()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void Log(ResultKind messageKind, IAnalysisContext context, Region region, string formatSpecifierId, params string[] arguments)
        {
            Log(messageKind, context?.Rule?.Id);
        }

        public void Log(ResultKind messageKind, string ruleId)
        {
            switch (messageKind)
            {

                case ResultKind.Pass:
                    {
                        break;
                    }

                case ResultKind.Error:
                {
                    break;
                }

                case ResultKind.Warning:
                {
                    break;
                }

                case ResultKind.NotApplicable:
                    {
                        if (ruleId == NoteDescriptors.InvalidTarget.Id)
                        {
                            _invalidTargetsCount++;
                        }
                        break;
                    }

                case ResultKind.Note:
                {
                    // Currently, we only use this id to fire verbose
                    // messages indicating a target is under analysis
                    if (ruleId == NoteDescriptors.GeneralMessage.Id)
                    {
                        _targetsCount++;
                    }
                    break;
                }

                case ResultKind.InternalError:
                    {
                        break;
                    }

                case ResultKind.ConfigurationError:
                    {
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        public void Dispose()
        {
            Console.WriteLine();
            Console.WriteLine("# valid targets: " + _targetsCount.ToString());
            Console.WriteLine("# invalid targets: " + _invalidTargetsCount.ToString());
            Console.WriteLine("Time elapsed: " + _stopwatch.Elapsed.ToString());
        }
    }
}
