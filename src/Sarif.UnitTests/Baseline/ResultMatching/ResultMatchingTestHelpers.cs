// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    static class ResultMatchingTestHelpers
    {
        public static Result CreateMatchingResult(string target, string location, string regionContent, string contextRegionContent = null)
        {
            Result result = new Result()
            {
                RuleId = "TEST001",
                Level = FailureLevel.Error,
            };

            if (target != null)
            {
                result.AnalysisTarget = new FileLocation()
                {
                    Uri = new Uri(target)
                };
            }

            if (location != null)
            {
                result.Locations =
                    new Location[]
                   {
                        new Location()
                        {
                            PhysicalLocation = new PhysicalLocation()
                            {
                                FileLocation = new FileLocation()
                                {
                                    Uri = new Uri(location)
                                },
                                Region = regionContent != null ? new Region() { StartLine = 5, Snippet = new FileContent() { Text = regionContent } } : null,
                                ContextRegion = contextRegionContent != null ? new Region() { StartLine = 10, Snippet = new FileContent { Text = contextRegionContent } } : null,
                            }
                        }
                   };
            }

            return result;
        }
    }
}
