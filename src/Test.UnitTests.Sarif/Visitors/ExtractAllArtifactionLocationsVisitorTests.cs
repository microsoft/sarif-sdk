// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ExtractAllArtifactLocationsVisitorTests
    {
        [Fact]
        public void ExtractAllArtifactLocationsVisitor_EmptyLog()
        {
            var visitor = new ExtractAllArtifactLocationsVisitor();
            visitor.VisitRun(new Run());
            visitor.AllArtifactLocations.Count.Should().Be(0);
        }

        [Fact]
        public void ExtractAllArtifactLocationsVisitor_ExtractsMultipleLocations()
        {
            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Results = new[]
                        {
                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule1,
                                BaselineState = BaselineState.New,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestData.FileLocations.Location1),
                                             }
                                        }
                                    }
                                }
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule2,
                                BaselineState = BaselineState.Updated,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestData.FileLocations.Location2),
                                             }
                                        }
                                    }
                                }
                            },

                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule2,
                                BaselineState = BaselineState.New,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestData.FileLocations.Location3),
                                             }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var visitor = new ExtractAllArtifactLocationsVisitor();
            visitor.VisitSarifLog(sarifLog);
            visitor.AllArtifactLocations.Count.Should().Be(3);

            foreach (Result result in sarifLog.Runs[0].Results)
            {
                visitor.AllArtifactLocations.Contains(result.Locations[0].PhysicalLocation.ArtifactLocation).Should().BeTrue();
            }
        }

        [Fact]
        public void ExtractAllArtifactLocationsVisitor_ExtractsMultipleLocationsSingleResult()
        {
            SarifLog sarifLog = TestData.CreateOneIdThreeLocations();

            var visitor = new ExtractAllArtifactLocationsVisitor();
            visitor.VisitSarifLog(sarifLog);
            visitor.AllArtifactLocations.Count.Should().Be(3);

            foreach (Result result in sarifLog.Runs[0].Results)
            {
                visitor.AllArtifactLocations.Contains(result.Locations[0].PhysicalLocation.ArtifactLocation).Should().BeTrue();
            }
        }

        [Fact]
        public void ExtractAllArtifactLocationsVisitor_FetchesIndexedLocations()
        {
            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Artifacts = new List<Artifact>
                        {
                            new Artifact{ Location = new ArtifactLocation{  Uri = new Uri(TestData.FileLocations.Location1), }, Contents = new ArtifactContent { Text = "New" } },
                            new Artifact{ Location = new ArtifactLocation{  Uri = new Uri(TestData.FileLocations.Location2), }, Contents = new ArtifactContent { Text = "Child of new" } },
                        },
                        Results = new[]
                        {
                            new Result
                            {
                                RuleId = TestData.RuleIds.Rule1,
                                BaselineState = BaselineState.New,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Index = 0,
                                             }
                                        }
                                    },
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Index = 1,
                                             }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var visitor = new ExtractAllArtifactLocationsVisitor();
            visitor.VisitSarifLog(sarifLog);
            visitor.AllArtifactLocations.Count.Should().Be(2);

            foreach (Artifact artifact in sarifLog.Runs[0].Artifacts)
            {
                visitor.AllArtifactLocations.Contains(artifact.Location).Should().BeTrue();
            }
        }
    }
}
