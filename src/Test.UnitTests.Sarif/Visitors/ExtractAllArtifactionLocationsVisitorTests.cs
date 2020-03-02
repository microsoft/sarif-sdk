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
        public void PerRunPerRuleSplittingVisitor_EmptyLog()
        {
            var visitor = new ExtractAllArtifactLocationsVisitor();
            visitor.VisitRun(new Run());
            visitor.allArtifactLocations.Count.Should().Be(0);
        }

        [Fact]
        public void PerRunPerRuleSplittingVisitor_ExtractsMultipleLocations()
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
                                RuleId = TestConstants.RuleIds.Rule1,
                                BaselineState = BaselineState.New,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestConstants.FileLocations.Location1),
                                             }
                                        }
                                    }
                                }
                            },

                            new Result
                            {
                                RuleId = TestConstants.RuleIds.Rule2,
                                BaselineState = BaselineState.Updated,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestConstants.FileLocations.Location2),
                                             }
                                        }
                                    }
                                }
                            },

                            new Result
                            {
                                RuleId = TestConstants.RuleIds.Rule2,
                                BaselineState = BaselineState.New,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestConstants.FileLocations.Location3),
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

            foreach (Result result in sarifLog.Runs[0].Results)
            {
                visitor.allArtifactLocations.Contains(result.Locations[0].PhysicalLocation.ArtifactLocation).Should().BeTrue();
            }
        }

        [Fact]
        public void PerRunPerRuleSplittingVisitor_ExtractsMultipleLocationsSingleResult()
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
                                RuleId = TestConstants.RuleIds.Rule1,
                                BaselineState = BaselineState.New,
                                Locations = new []
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestConstants.FileLocations.Location1),
                                             }
                                        }
                                    },
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestConstants.FileLocations.Location2),
                                             }
                                        }
                                    },
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestConstants.FileLocations.Location3),
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

            foreach (Location location in sarifLog.Runs[0].Results[0].Locations)
            {
                visitor.allArtifactLocations.Contains(location.PhysicalLocation.ArtifactLocation).Should().BeTrue();
            }
        }
    }
}
