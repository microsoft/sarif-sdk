// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    public static class TestData
    {
        public const string TestRuleId = "TST0001";
        public const string TestToolName = nameof(TestToolName);
        public const string SecondTestToolName = nameof(SecondTestToolName);
        public const string TestMessageText = "This is a flattened (argument-free) test message.";
        public const string TestMessageStringId = "testMessageStringId";
        public const string TestAnalysisTarget = @"C:\dir\file";
        public const string NotActuallyASecret = nameof(NotActuallyASecret);
        public const string TestRootBaseId = "TEST_ROOT";

        public const string AutomationDetailsGuid = "D41BF9F2-225D-4254-984E-DFD659702E4D";
        public const string ConverterName = "TestConverter";
        public const string LanguageIdentifier = "xx-XX";
        public const string PolicyName = "TestPolicy";
        public const string TaxonomyName = "TestTaxonomy";
        public const string ToolName = "TestTool";
        public const string TranslationName = "TestTranslation";
        public const string TranslationMetadataName = "TestTranslationMetadata";

        public static class RuleIds
        {
            public const string Rule1 = "TST0001";
            public const string Rule2 = "TST0002";
            public const string Rule3 = "TST0003";
            public const string Rule4 = "TST0004";
            public const string Rule5 = "TST0005";
            public const string Rule6 = "TST0006";
            public const string Rule7 = "TST0007";
            public const string Rule8 = "TST0008";
            public const string Rule9 = "TST0009";
            public const string Rule10 = "TST0010";
        }

        // In general, static instances of SARIF logs are a bad idea, because
        // they are mutable. A test may munge these and leave them in a bad
        // state for another test. Instead, we should prefer factories that
        // reliably generate a fresh copy of a log for each test.

        public static SarifLog CreateOneIdThreeLocations()
        {
            return new SarifLog
            {
                Runs = new[]
            {
                    new Run
                    {
                        Tool = CreateSimpleLog().Runs[0].Tool,
                        VersionControlProvenance = new []
                        {
                            new VersionControlDetails
                            {
                                RepositoryUri = new Uri(@"https://bugfiler.example.com/")
                            }
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
                                                Uri = new Uri(TestData.FileLocations.Location1),
                                             }
                                        }
                                    },
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                             ArtifactLocation = new ArtifactLocation
                                             {
                                                Uri = new Uri(TestData.FileLocations.Location2),
                                             }
                                        }
                                    },
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
        }

        public static SarifLog CreateTwoRunThreeResultLog()
        { 
            return new SarifLog
            {
                Runs = new[]
            {
                    new Run
                    {
                        Tool = CreateSimpleLog().Runs[0].Tool,
                        Results = new[]
                        {
                            CreateResult(FailureLevel.Error, ResultKind.Fail, new Region(), TestData.FileLocations.Location1),
                            CreateResult(FailureLevel.Error, ResultKind.Fail, new Region(), TestData.FileLocations.Location2),
                        }
                    },
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = SecondTestToolName
                            }
                        },
                        Results = new[]
                        {
                            CreateResult(FailureLevel.Error, ResultKind.Fail, new Region(), TestData.FileLocations.Location1)
                        }
                    }
                }
            };
        }

        public static class FileLocations
        {
            public const string Location1 = @"C:\Test\Data\File1.sarif";
            public const string Location2 = @"C:\Test\Data\File2.sarif";
            public const string Location3 = @"C:\Test\Data\File3.sarif";
            public const string Location4 = @"C:\Test\Data\File4.sarif";
        }

        public static class TaxonIds
        {
            public const string Taxon1 = "TAX0001";
            public const string Taxon2 = "TAX0002";
        }

        public static readonly ReportingDescriptor TestRule = new ReportingDescriptor
        {
            Id = TestRuleId,
            Name = "ThisIsATest",
            ShortDescription = new MultiformatMessageString { Text = "short description" },
            FullDescription = new MultiformatMessageString { Text = "full description" },
            MessageStrings = new Dictionary<string, MultiformatMessageString>
            {
                [TestMessageStringId] = new MultiformatMessageString { Text = "First: {0}, Second: {1}" }
            }
        };

        internal static Result CreateResult(FailureLevel level, ResultKind kind, Region region, string path)
        {
            return new Result
            {
                RuleId = TestRuleId,
                Level = level,
                Kind = kind,
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(path, UriKind.RelativeOrAbsolute)
                            },
                            Region = region
                        }
                    }
                },
                Message = new Message
                {
                    Arguments = new List<string>
                    {
                        "42",
                        "54"
                    },
                    Id = TestMessageStringId
                }
            };
        }

        public static SarifLog CreateSimpleLog()
        {
            return new SarifLog
            {
                Runs = new Run[]
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = TestToolName
                            }
                        },
                        Results = new []
                        {
                            new Result
                            {
                                Rule = new ReportingDescriptorReference
                                {
                                    Id = RuleIds.Rule1
                                },
                                Message = new Message
                                {
                                    Text = TestMessageText
                                }
                            },
                            new Result
                            {
                                Rule = new ReportingDescriptorReference
                                {
                                    Id = RuleIds.Rule2
                                },
                                Message = new Message
                                {
                                    Text = TestMessageText
                                }
                            }
                        }
                    }
                }
            };
        }

        public static SarifLog CreateEmptyRun()
        {
            return new SarifLog
            {
                Runs = new Run[]
                {
                    new Run
                    {
                    }
                }
            };
        }
    }
}