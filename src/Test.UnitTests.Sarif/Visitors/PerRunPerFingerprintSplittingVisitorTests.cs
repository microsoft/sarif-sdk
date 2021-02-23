// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

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

            // We must find one run with two results pointing to fingerprint=a
            sarifLog.Runs[0].Results.Should().HaveCount(2);
            sarifLog.Runs[0].Results.Count(r => r.Fingerprints["fingerprint"] == "a").Should().Be(2);

            // We must find one run with one result pointing to fingerprint=b
            sarifLog.Runs[1].Results.Should().HaveCount(1);
            sarifLog.Runs[1].Results.Count(r => r.Fingerprints["fingerprint"] == "b").Should().Be(1);
        }
    }
}
