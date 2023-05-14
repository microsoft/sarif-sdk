// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.Diagnostics.Tracing;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class DumpEventsCommand
    {
        public int Run(DumpEventsOptions options)
        {
            string path = options.EventsFilePath;

            using (var source = new ETWTraceEventSource(path))
            {
                double enumerateArtifactsStartMSec = 0;
                TimeSpan timeSpentEnumeratingArtifacts = default;
                TimeSpan firstArtifactQueued = default;

                source.Dynamic.All += delegate (TraceEvent traceEvent)
                {
                    switch (traceEvent.EventName)
                    {
                        case "FirstArtifactQueued":
                        {
                            firstArtifactQueued = TimeSpan.FromMilliseconds(traceEvent.TimeStampRelativeMSec);
                            break;
                        }

                        case "EnumerateArtifacts/Start":
                        {
                            enumerateArtifactsStartMSec = traceEvent.TimeStampRelativeMSec;
                            break;
                        }

                        case "EnumerateArtifacts/Stop":
                        {
                            timeSpentEnumeratingArtifacts = TimeSpan.FromMilliseconds(
                                traceEvent.TimeStampRelativeMSec - enumerateArtifactsStartMSec);
                            break;
                        }
                    }
                };

                source.Process();

                Console.WriteLine($@"Time elapsed until first artifact queued for analysis: {firstArtifactQueued}");
                Console.WriteLine($@"Time elapsed until artifact enumeration completed: {timeSpentEnumeratingArtifacts}");
            }
            return 0;
        }
    }
}
