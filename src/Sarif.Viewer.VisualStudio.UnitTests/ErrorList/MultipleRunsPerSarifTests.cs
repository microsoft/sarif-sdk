// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.ErrorList;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    // Added tests to Collection because otherwise the other tests
    // will load in parallel, which causes issues with static collections.
    // Production code will only load one SARIF file at a time.
    [Collection("SarifObjectTests")]
    public class MultipleRunsPerSarifTests
    {
        public MultipleRunsPerSarifTests()
        {
            var testLog = new SarifLog
            {
                Runs = new List<Run>
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Name = "Test",
                            SemanticVersion = "1.0"
                        },
                        Results = new List<Result>
                        {
                            new Result
                            {
                                RuleId = "C0001",
                                Message = new Message { Text = "Error 1" },
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            FileLocation = new FileLocation
                                            {
                                                Uri = new Uri("file:///item1.cpp")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Run
                    {
                        Tool = new Tool
                        {
                            Name = "Test",
                            SemanticVersion = "1.0"
                        },
                        Results = new List<Result>
                        {
                            new Result
                            {
                                RuleId = "C0002",
                                Message = new Message { Text = "Error 2" },
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            FileLocation = new FileLocation
                                            {
                                                Uri = new Uri("file:///item2.cpp")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            TestUtilities.InitializeTestEnvironment(testLog);
        }

        [Fact]
        public void ErrorList_WithMultipleRuns_ListObjectHasAllRows()
        {
            var hasFirstError = SarifTableDataSource.Instance.HasErrors("/item1.cpp");
            var hasSecondError = SarifTableDataSource.Instance.HasErrors("/item2.cpp");

            var hasBothErrors = hasFirstError && hasSecondError;

            hasBothErrors.Should().BeTrue();
        }

        [Fact]
        public void ErrorList_WithMultipleRuns_ManagerHasAllRows()
        {
            var errorCount = CodeAnalysisResultManager.Instance.SarifErrors.Count;

            errorCount.Should().Be(2);
        }
    }
}
