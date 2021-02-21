// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerFingerprintSplittingVisitorTests
    {
        [Fact]
        public void PerRunPerFingerprintSplittingVisitor_EmptyLog()
        {
            var visitor = new PerRunPerFingerprintSplittingVisitor();
            visitor.VisitRun(new Run());
            visitor.SplitSarifLogs.Count.Should().Be(0);
        }

        [Fact]
        public void PerRunPerFingerprintSplittingVisitor_CheckingSplitResults()
        {
            var sarif = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Results = new []
                        {
                            new Result
                            {
                                Fingerprints = new Dictionary<string, string>
                                {
                                    { "fingerprint", "a" }
                                },
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                           ArtifactLocation = new ArtifactLocation
                                           {
                                               Uri = new Uri("1.txt", UriKind.RelativeOrAbsolute)
                                           }
                                        }
                                    }
                                }
                            },
                            new Result
                            {
                                Fingerprints = new Dictionary<string, string>
                                {
                                    { "fingerprint", "a" }
                                },
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                           ArtifactLocation = new ArtifactLocation
                                           {
                                               Uri = new Uri("2.txt", UriKind.RelativeOrAbsolute)
                                           }
                                        }
                                    }
                                }
                            },
                            new Result
                            {
                                Fingerprints = new Dictionary<string, string>
                                {
                                    { "fingerprint", "b" }
                                },
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                           ArtifactLocation = new ArtifactLocation
                                           {
                                               Uri = new Uri("1.txt", UriKind.RelativeOrAbsolute)
                                           }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var splitting = new PerRunPerFingerprintSplittingVisitor();
            splitting.Visit(sarif);

            SarifLog sarifLog = splitting.SplitSarifLogs[0];

            // We must find one result with fingerprint a with two locations.
            sarifLog.Runs[0].Results[0].Locations.Count.Should().Be(2);

            // We must find one result with fingerprint b with two locations.
            sarifLog.Runs[0].Results[1].Locations.Count.Should().Be(1);
        }
    }
}
