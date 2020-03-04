// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class SkimmerIdComparerTests
    {
        [Fact]
        public void SkimmerIdComparer_HandlesNullCasesProperly()
        {
            SkimmerIdComparer<TestAnalysisContext>.Instance.Compare(null, null).Should().Be(0);

            var skimmer = new TestRule();
            int leftIsNull = SkimmerIdComparer<TestAnalysisContext>.Instance.Compare(null, skimmer);
            int rightIsNull = SkimmerIdComparer<TestAnalysisContext>.Instance.Compare(skimmer, null);

            leftIsNull.Should().NotBe(0);
            rightIsNull.Should().NotBe(0);
            leftIsNull.Should().NotBe(rightIsNull);
        }

        [Fact]
        public void SkimmerIdComparer_ComparesIdAndName()
        {
            var failedTestCases = new List<string>();

            foreach (SkimmerIdComparerTestCase testCase in s_skimmerIdComparerTestCases)
            {
                TestRule leftRule = PopulateRuleProperties(id: testCase.LeftId, name: testCase.LeftName);
                TestRule rightRule = PopulateRuleProperties(id: testCase.RightId, name: testCase.RightName);

                int comparison = SkimmerIdComparer<TestAnalysisContext>.Instance.Compare(leftRule, rightRule);

                switch (testCase.ExpectedResult)
                {
                    case Equivalence.Equal:
                    {
                        if (comparison != 0)
                        {
                            failedTestCases.Add(testCase.Title);
                        }
                        break;
                    }
                    case Equivalence.LessThan:
                    {
                        if (comparison >= 0)
                        {
                            failedTestCases.Add(testCase.Title);
                        }
                        break;
                    }
                    case Equivalence.GreaterThan:
                    {
                        if (comparison <= 0)
                        {
                            failedTestCases.Add(testCase.Title);
                        }
                        break;
                    }
                }
            }

            failedTestCases.Should().BeEmpty();
        }

        private TestRule PopulateRuleProperties(string id, string name)
        {
            // This helper exhaustively sets an arbitrary guid string value to every 
            // rule property (as well as setting the provided id and name). This ensures
            // that every rule instance has unique data associated with it outside the 
            // id and name (and therefore ensures these unique values are never considered
            // when comparing rule instances via the SkimmerIdComparer).
            var rule = new TestRule()
            {
                Id = id,
                Name = name
            };

            rule.DefaultConfiguration = new ReportingConfiguration();
            rule.DefaultConfiguration.SetProperty<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            rule.DeprecatedGuids = new string[] { Guid.NewGuid().ToString() };
            rule.DeprecatedIds = new string[] { Guid.NewGuid().ToString() };
            rule.DeprecatedNames = new string[] { Guid.NewGuid().ToString() };
            rule.FullDescription = new MultiformatMessageString { Text = Guid.NewGuid().ToString() };

            rule.Guid = Guid.NewGuid().ToString();
            rule.Help = new MultiformatMessageString { Text = Guid.NewGuid().ToString() };
            rule.HelpUri = new Uri(Guid.NewGuid().ToString(), UriKind.RelativeOrAbsolute);

            rule.MessageStrings = new Dictionary<string, MultiformatMessageString>
            {
                [Guid.NewGuid().ToString()] = new MultiformatMessageString { Text = Guid.NewGuid().ToString() }
            };

            rule.SetProperty<string>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            rule.Relationships = new[]
            {
                new ReportingDescriptorRelationship { Description = new Message{ Text = Guid.NewGuid().ToString() } }
            };

            rule.ShortDescription = new MultiformatMessageString { Text = Guid.NewGuid().ToString() };

            return rule;
        }

        internal enum Equivalence
        {
            Equal,
            GreaterThan,
            LessThan
        }

        internal struct SkimmerIdComparerTestCase
        {
            public string Title;
            public string LeftId;
            public string LeftName;
            public string RightId;
            public string RightName;
            public Equivalence ExpectedResult;
        }

        private static readonly SkimmerIdComparerTestCase[] s_skimmerIdComparerTestCases = new SkimmerIdComparerTestCase[]
        {
            new SkimmerIdComparerTestCase
            {
                Title = "Left and right have equivalent id and name.",
                LeftId = "aaa",
                LeftName = "zzz",
                RightId = "aaa",
                RightName = "zzz",
                ExpectedResult = Equivalence.Equal
            },
            new SkimmerIdComparerTestCase
            {
                Title = "Left id only is lesser.",
                LeftId = "aaa",
                LeftName = null,
                RightId = "zzz",
                RightName = null,
                ExpectedResult = Equivalence.LessThan
            },
            new SkimmerIdComparerTestCase
            {
                Title = "Left id only is greater.",
                LeftId = "zzz",
                LeftName = null,
                RightId = "aaa",
                RightName = null,
                ExpectedResult = Equivalence.GreaterThan
            },            new SkimmerIdComparerTestCase
            {
                Title = "Left id is lesser due to name.",
                LeftId = "aaa",
                LeftName = "aaa",
                RightId = "aaa",
                RightName = "zzz",
                ExpectedResult = Equivalence.LessThan
            },
            new SkimmerIdComparerTestCase
            {
                Title = "Left id is greater due to name.",
                LeftId = "aaa",
                LeftName = "zzz",
                RightId = "aaa",
                RightName = "aaa",
                ExpectedResult = Equivalence.GreaterThan
            },
        };
    }
}
