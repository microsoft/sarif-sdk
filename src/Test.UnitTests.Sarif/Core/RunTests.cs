// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class RunTests
    {
        private const string s_UriBaseId = "$$SomeUriBaseId$$";
        private readonly Uri s_Uri = new Uri("relativeUri/toSomeFile.txt", UriKind.Relative);

        private static readonly ReadOnlyCollection<HasAbsentResultsTestCase> s_hasAbsentResultsTestCases = new List<HasAbsentResultsTestCase>
        {
            new HasAbsentResultsTestCase
            {
                Name = "Null results",
                Run = new Run(),
                ExpectedResult = false
            },

            new HasAbsentResultsTestCase
            {
                Name = "Empty results",
                Run = new Run
                {
                    Results = new List<Result>()
                },
                ExpectedResult = false
            },

            new HasAbsentResultsTestCase
            {
                Name = "No baseline",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result()
                    }
                },
                ExpectedResult = false
            },

            new HasAbsentResultsTestCase
            {
                Name = "New result",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            BaselineState = BaselineState.New
                        }
                    }
                },
                ExpectedResult = false
            },

            new HasAbsentResultsTestCase
            {
                Name = "Absent result",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            BaselineState = BaselineState.Absent
                        }
                    }
                },
                ExpectedResult = true
            },

            new HasAbsentResultsTestCase
            {
                Name = "Absent result after new result",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            BaselineState = BaselineState.New
                        },

                        new Result
                        {
                            BaselineState = BaselineState.Absent
                        }
                    }
                },
                ExpectedResult = true
            },

            new HasAbsentResultsTestCase
            {
                Name = "Absent result after result with no BaselineState",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result(),

                        new Result
                        {
                            BaselineState = BaselineState.Absent
                        }
                    }
                },
                ExpectedResult = false
            }
        }.AsReadOnly();

        private static readonly ReadOnlyCollection<HasSuppressedResultsTestCase> s_hasSuppressedResultsTestCases = new List<HasSuppressedResultsTestCase>
        {
            new HasSuppressedResultsTestCase
            {
                Name = "Null results",
                Run = new Run(),
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Empty results",
                Run = new Run
                {
                    Results = new List<Result>()
                },
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Null suppressions array",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result()
                    }
                },
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Empty suppressions array",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            Suppressions = new List<Suppression>()
                        }
                    }
                },
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Accepted suppression",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.Accepted
                                }
                            }
                        }
                    }
                },
                ExpectedResult = true
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Suppression under review",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.UnderReview
                                }
                            }
                        }
                    }
                },
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Rejected suppression",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.Rejected
                                }
                            }
                        }
                    }
                },
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Suppressed result after result with rejected suppression",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.Rejected
                                }
                            }
                        },

                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.Accepted
                                }
                            }
                        }
                    }
                },
                ExpectedResult = true
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Suppressed result after result with null suppressions",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result(),

                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.Accepted
                                }
                            }
                        }
                    }
                },
                ExpectedResult = false
            },

            new HasSuppressedResultsTestCase
            {
                Name = "Suppression both rejected and accepted",
                Run = new Run
                {
                    Results = new List<Result>
                    {
                        new Result(),

                        new Result
                        {
                            Suppressions = new List<Suppression>
                            {
                                new Suppression
                                {
                                    Status = SuppressionStatus.Accepted
                                },
                                new Suppression
                                {
                                    Status = SuppressionStatus.Rejected
                                }
                            }
                        }
                    }
                },
                ExpectedResult = false
            }
        }.AsReadOnly();

        [Fact]
        public void Run_ColumnKindSerializesProperly()
        {
            // In our Windows-specific SDK, if no one has explicitly set ColumnKind, we
            // will set it to the windows-specific value of Utf16CodeUnits. Otherwise,
            // the SARIF file will pick up the ColumnKind default value of
            // UnicodeCodePoints, which is not appropriate for Windows frameworks.
            RoundTripColumnKind(persistedValue: ColumnKind.None, expectedRoundTrippedValue: ColumnKind.Utf16CodeUnits);

            // When explicitly set, we should always preserve that setting
            RoundTripColumnKind(persistedValue: ColumnKind.Utf16CodeUnits, expectedRoundTrippedValue: ColumnKind.Utf16CodeUnits);
            RoundTripColumnKind(persistedValue: ColumnKind.UnicodeCodePoints, expectedRoundTrippedValue: ColumnKind.UnicodeCodePoints);
        }

        [Fact]
        public void Run_RetrievesExistingFileDataObject()
        {
            Run run = BuildDefaultRunObject();

            ArtifactLocation fileLocation = BuildDefaultFileLocation();
            fileLocation.Index.Should().Be(-1);

            // Retrieve existing file location. Our input file location should have its
            // fileIndex property set as well.
            RetrieveFileIndexAndValidate(run, fileLocation, expectedFileIndex: 1);

            // Repeat look-up with bad file index value. This should succeed and reset
            // the fileIndex to the appropriate value.
            fileLocation = BuildDefaultFileLocation();
            fileLocation.Index = int.MaxValue;
            RetrieveFileIndexAndValidate(run, fileLocation, expectedFileIndex: 1);

            // Now set a unique property bag on the file location. The property bag
            // should not interfere with retrieving the file data object. The property bag should
            // not be modified as a result of retrieving the file data index.
            fileLocation = BuildDefaultFileLocation();
            fileLocation.SetProperty(Guid.NewGuid().ToString(), "");
            RetrieveFileIndexAndValidate(run, fileLocation, expectedFileIndex: 1);

            // Now use a file location that has no url and therefore relies strictly on
            // the index in run.artifacts (i.e., _fileToIndexMap will not be used).
            fileLocation = new ArtifactLocation
            {
                Index = 0
            };
            RetrieveFileIndexAndValidate(run, fileLocation, expectedFileIndex: 0);
        }

        [Fact]
        public void Run_ColumnKind_InitializedAsUtf16CodeUnits()
        {
            // This test ensures NotYetAutoGenerated field 'ColumnKind' is initialized as Utf16CodeUnits
            // see .src/sarif/NotYetAutoGenerated/Run.cs (columnKind property)
            var run = new Run();

            run.ColumnKind.Should().Be(ColumnKind.Utf16CodeUnits);
        }

        [Fact]
        public void Run_ComputePolicies_WhenCollectionIsNullOrEmpty()
        {
            Dictionary<string, FailureLevel> cache = Run.ComputePolicies(null);
            cache.Count.Should().Be(0);

            cache = Run.ComputePolicies(new ToolComponent[] { });
            cache.Count.Should().Be(0);
        }

        [Fact]
        public void Run_ComputePolicies_SingleComponent()
        {
            Dictionary<string, FailureLevel> cache = Run.ComputePolicies(new ToolComponent[] { CreateToolComponent("test", 1, FailureLevel.Error) });
            cache.Count.Should().Be(1);
        }

        [Fact]
        public void Run_ComputePolicies_MultipleComponents()
        {
            Dictionary<string, FailureLevel> cache = Run.ComputePolicies(new ToolComponent[]
            {
                CreateToolComponent("test1", 1, FailureLevel.Error),
                CreateToolComponent("test2", 2, FailureLevel.Warning)
            });
            cache.Count.Should().Be(2);
            cache.First().Value.Should().Be(FailureLevel.Warning);
            cache.Last().Value.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void Run_ApplyPolicies_WhenWeDontHavePolicies()
        {
            Run run = CreateRun(resultCount: 1);
            run.ApplyPolicies();
            run.Results[0].Level.Should().Be(FailureLevel.Note);
        }

        [Fact]
        public void Run_ApplyPolicies_WhenWeHavePolicy()
        {
            Run run = CreateRun(resultCount: 1);
            run.Policies = new ToolComponent[] { CreateToolComponent("test", 1, FailureLevel.Error) };
            run.ApplyPolicies();
            run.Results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void Run_ApplyPolicies_WhenWeHavePolicies()
        {
            Run run = CreateRun(resultCount: 1);
            run.Policies = new ToolComponent[]
            {
                CreateToolComponent("test1", rulesCount: 1, FailureLevel.Error),
                CreateToolComponent("test2", rulesCount: 2, FailureLevel.Warning)
            };
            run.ApplyPolicies();
            run.Results[0].Level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void Run_ApplyPoliies_WhenWeHaveCacheAvailable()
        {
            var cachedPolicies = new Dictionary<string, FailureLevel>() { { "TEST0", FailureLevel.None } };
            Run run = CreateRun(resultCount: 1);
            run.Policies = new ToolComponent[]
            {
                CreateToolComponent("test1", rulesCount: 1, FailureLevel.Error)
            };
            run.ApplyPolicies(cachedPolicies);

            run.Results[0].Level.Should().Be(FailureLevel.None);
        }

        [Fact]
        public void HasAbsentResults_ReturnsExpectedValue()
        {
            var sb = new StringBuilder();

            foreach (HasAbsentResultsTestCase testCase in s_hasAbsentResultsTestCases)
            {
                bool actualResult = testCase.Run.HasAbsentResults();
                if (actualResult != testCase.ExpectedResult)
                {
                    sb.AppendLine($"    {testCase.Name}: expected {testCase.ExpectedResult} but got {actualResult}");
                }
            }

            sb.Length.Should().Be(0, "failed test cases:\n" + sb.ToString());
        }

        [Fact]
        public void HasSuppressedResults_ReturnsExpectedValue()
        {
            var sb = new StringBuilder();

            foreach (HasSuppressedResultsTestCase testCase in s_hasSuppressedResultsTestCases)
            {
                bool actualResult = testCase.Run.HasSuppressedResults();
                if (actualResult != testCase.ExpectedResult)
                {
                    sb.AppendLine($"    {testCase.Name}: expected {testCase.ExpectedResult} but got {actualResult}");
                }
            }

            sb.Length.Should().Be(0, "failed test cases:\n" + sb.ToString());
        }

        [Fact]
        public void Run_ShouldSerializeAutomationDetails_WhenIdOrGuidIsNotNullOrWhiteSpace()
        {
            const string id = "automation-id";
            const string guid = "automation-guid";

            var sarifLog = new SarifLog
            {
                Runs = new[] { new Run() }
            };
            sarifLog.Runs[0].AutomationDetails = new RunAutomationDetails();

            sarifLog.Runs[0].AutomationDetails.Id = id;
            sarifLog.Runs[0].AutomationDetails.Guid = string.Empty;

            AsserAutomationDetailsValues(sarifLog, id, string.Empty);

            sarifLog.Runs[0].AutomationDetails.Id = string.Empty;
            sarifLog.Runs[0].AutomationDetails.Guid = guid;

            AsserAutomationDetailsValues(sarifLog, string.Empty, guid);

            sarifLog.Runs[0].AutomationDetails.Id = id;
            sarifLog.Runs[0].AutomationDetails.Guid = guid;

            AsserAutomationDetailsValues(sarifLog, id, guid);
        }

        [Fact]
        public void Run_ShouldNotSerializeAutomationDetails_WhenIdAndGuidAreNullOrWhiteSpace()
        {
            const string whiteSpace = " ";

            var sarifLog = new SarifLog
            {
                Runs = new[] { new Run() }
            };
            sarifLog.Runs[0].AutomationDetails = new RunAutomationDetails();

            sarifLog.Runs[0].AutomationDetails.Id = string.Empty;
            sarifLog.Runs[0].AutomationDetails.Guid = string.Empty;

            AsserAutomationDetailsValues(sarifLog, string.Empty, string.Empty);

            sarifLog.Runs[0].AutomationDetails.Id = null;
            sarifLog.Runs[0].AutomationDetails.Guid = null;

            AsserAutomationDetailsValues(sarifLog, string.Empty, string.Empty);

            sarifLog.Runs[0].AutomationDetails.Id = whiteSpace;
            sarifLog.Runs[0].AutomationDetails.Guid = whiteSpace;

            AsserAutomationDetailsValues(sarifLog, whiteSpace, whiteSpace);
        }

        private void AsserAutomationDetailsValues(SarifLog sarifLog, string id, string guid)
        {
            string sarifLogText = JsonConvert.SerializeObject(sarifLog);
            if (string.IsNullOrWhiteSpace(id))
            {
                // Checking if the property 'id' exists.
                sarifLogText.Should().NotContain($@"""{nameof(id)}""");
            }
            else
            {
                sarifLogText.Should().Contain(id);
            }

            if (string.IsNullOrWhiteSpace(guid))
            {
                // Checking if the property 'guid' exists.
                sarifLogText.Should().NotContain($@"""{nameof(guid)}""");
            }
            else
            {
                sarifLogText.Should().Contain(guid);
            }
        }

        private void RoundTripColumnKind(ColumnKind persistedValue, ColumnKind expectedRoundTrippedValue)
        {
            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool { Driver = new ToolComponent { Name = "Test tool"} },
                        ColumnKind = persistedValue
                    }
                }
            };

            string json = JsonConvert.SerializeObject(sarifLog);

            sarifLog = JsonConvert.DeserializeObject<SarifLog>(json);
            sarifLog.Runs[0].ColumnKind.Should().Be(expectedRoundTrippedValue);
        }

        private void RetrieveFileIndexAndValidate(Run run, ArtifactLocation fileLocation, int expectedFileIndex)
        {
            bool validateIndex = fileLocation.Uri != null;
            int fileIndex = run.GetFileIndex(fileLocation, addToFilesTableIfNotPresent: false);

            if (validateIndex)
            {
                fileLocation.Index.Should().Be(fileIndex);
            }

            fileIndex.Should().Be(expectedFileIndex);
        }

        private ArtifactLocation BuildDefaultFileLocation()
        {
            return new ArtifactLocation { Uri = s_Uri, UriBaseId = s_UriBaseId };
        }

        private Run BuildDefaultRunObject()
        {
            var run = new Run()
            {
                Artifacts = new[]
                {
                    new Artifact
                    {
                        // This unused fileLocation exists simply to move testing
                        // to the second array element. Tests that depend on a fileIndex
                        // of '0' are suspect because 0 is a value that might be set as
                        // a default in some code paths, due to a bug
                        Location = new ArtifactLocation{ Uri = new Uri("unused", UriKind.RelativeOrAbsolute)}
                    },
                    new Artifact
                    {
                        Location = BuildDefaultFileLocation(),
                        Properties = new Dictionary<string, SerializedPropertyInfo>
                        {
                            [Guid.NewGuid().ToString()] = null
                        }
                    }
                }
            };
            run.Artifacts[0].Location.Index = 0;

            return run;
        }

        private ToolComponent CreateToolComponent(string name, int rulesCount, FailureLevel failureLevel)
        {
            ToolComponent toolComponent = new ToolComponent();
            toolComponent.Name = name;
            toolComponent.Rules = new ReportingDescriptor[rulesCount];
            for (int i = 0; i < rulesCount; i++)
            {
                toolComponent.Rules[i] = new ReportingDescriptor
                {
                    Id = $"TEST{i}",
                    DefaultConfiguration = new ReportingConfiguration
                    {
                        Level = failureLevel
                    }
                };
            }

            return toolComponent;
        }

        private Run CreateRun(int resultCount)
        {
            var run = new Run
            {
                Results = new Result[resultCount]
            };
            for (int i = 0; i < resultCount; i++)
            {
                run.Results[i] = new Result
                {
                    RuleId = $"TEST{i}",
                    Level = FailureLevel.Note
                };
            }

            return run;
        }

        private struct HasAbsentResultsTestCase
        {
            internal string Name;
            internal Run Run;
            internal bool ExpectedResult;
        }

        private struct HasSuppressedResultsTestCase
        {
            internal string Name;
            internal Run Run;
            internal bool ExpectedResult;
        }
    }
}
