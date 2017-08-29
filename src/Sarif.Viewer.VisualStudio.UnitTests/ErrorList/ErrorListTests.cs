﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.ErrorList;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class ErrorListTests
    {
        public ErrorListTests()
        {
            new SarifViewerPackage();

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
                                Message = "Error 1",
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        AnalysisTarget = new PhysicalLocation
                                        {
                                            Uri = new Uri("file:///item1.cpp")
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
                                Message = "Error 2",
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        AnalysisTarget = new PhysicalLocation
                                        {
                                            Uri = new Uri("file:///item2.cpp")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            ErrorListService.ProcessSarifLog(testLog, "TestPath.sarif", null);
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
