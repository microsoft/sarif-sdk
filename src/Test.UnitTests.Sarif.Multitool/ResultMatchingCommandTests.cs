using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
{
    public class ResultMatchingCommandTests
    {
        [Fact]
        public void ResultMatchingCommand_TestingSameResult()
        {
            SarifLog baselineSarif, currentSarif;

            #region .: Arrange :.

            baselineSarif = CreateBaseline();
            currentSarif = CreateBaseline();

            #endregion

            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            SarifLog output = matcher.Match(new SarifLog[] { baselineSarif }, new SarifLog[] { currentSarif }).First();

            output.Runs.First().Results.First().BaselineState.Should().Be(BaselineState.Unchanged);
        }

        [Fact]
        public void ResultMatchingCommand_TestingNewResults()
        {
            SarifLog baselineSarif, currentSarif;

            #region .: Arrange :.

            baselineSarif = CreateBaseline();
            currentSarif = CreateCurrent();

            #endregion

            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            SarifLog output = matcher.Match(new SarifLog[] { baselineSarif }, new SarifLog[] { currentSarif }).First();

            output.Runs[0].Results[0].BaselineState.Should().Be(BaselineState.Unchanged);
        }

        private SarifLog CreateBaseline()
        {
            return new SarifLog
            {
                Runs = new List<Run> {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "TEST",
                                Rules = new List<ReportingDescriptor>
                                {
                                    new ReportingDescriptor
                                    {
                                        Id = "TEST0001",
                                        Name = "Test 0001",
                                        ShortDescription = new MultiformatMessageString
                                        {
                                            Text = "Test short description"
                                        }
                                    }
                                }
                            }
                        },
                        Results = new List<Result>
                        {
                            new Result
                            {
                                RuleId = "TEST0001",
                                RuleIndex = 0,
                                Message = new Message
                                {
                                    Text = "Error found"
                                },
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            ArtifactLocation = new ArtifactLocation
                                            {
                                                Uri = new System.Uri("src/test0001.cs", UriKind.Relative)
                                            }
                                        }
                                    }
                                },
                                BaselineState = BaselineState.Unchanged
                            }
                        }
                    }
                },
            };
        }

        private SarifLog CreateCurrent()
        {
            return new SarifLog
            {
                Runs = new List<Run> {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "TEST",
                                Rules = new List<ReportingDescriptor>
                                {
                                    new ReportingDescriptor
                                    {
                                        Id = "TEST0000",
                                        Name = "Test 0000",
                                        ShortDescription = new MultiformatMessageString
                                        {
                                            Text = "Test short description"
                                        }
                                    },
                                    new ReportingDescriptor
                                    {
                                        Id = "TEST0001",
                                        Name = "Test 0001",
                                        ShortDescription = new MultiformatMessageString
                                        {
                                            Text = "Test short description"
                                        }
                                    }
                                }
                            }
                        },
                        Results = new List<Result>
                        {
                            new Result
                            {
                                RuleId = "TEST0001",
                                RuleIndex = 1,
                                Message = new Message
                                {
                                    Text = "Error found"
                                },
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            ArtifactLocation = new ArtifactLocation
                                            {
                                                Uri = new System.Uri("src/test0001.cs", UriKind.Relative)
                                            }
                                        }
                                    }
                                },
                                BaselineState = BaselineState.Unchanged
                            }
                        }
                    }
                },
            };
        }
    }
}
