// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class UpdateIndicesFromLegacyDataVisitorTests
    {
        private readonly string _remappedUriBaseId = Guid.NewGuid().ToString();
        private readonly string _remappedFullyQualifiedName = Guid.NewGuid().ToString();
        private readonly Uri _remappedUri = new Uri(Guid.NewGuid().ToString(), UriKind.Relative);
        private readonly string _remappedFullyQualifiedLogicalName = Guid.NewGuid().ToString();
        private readonly Result _result;

        public UpdateIndicesFromLegacyDataVisitorTests()
        {
            _result = new Result
            {
                Locations = new[]
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = _remappedUri, UriBaseId = _remappedUriBaseId, Index = int.MaxValue}
                        },
                        LogicalLocation = new LogicalLocation
                        {
                            FullyQualifiedName = _remappedFullyQualifiedLogicalName,
                            Index = int.MaxValue
                        },
                    }
                }
            };
        }


        [Fact]
        public void UpdateIndicesFromLegacyDataVisitor_FunctionsWithNullMaps()
        {
            Result result = _result.DeepClone();

            var visitor = new UpdateIndicesFromLegacyDataVisitor(null, null, null);
            visitor.VisitResult(result);

            result.Locations[0].LogicalLocation.Index.Should().Be(int.MaxValue);
            result.Locations[0].PhysicalLocation.ArtifactLocation.Index.Should().Be(int.MaxValue);
        }

        [Fact]
        public void UpdateIndicesFromLegacyDataVisitor_RemapsFullyQualifiedogicalLNames()
        {
            Result result = _result.DeepClone();
            int remappedIndex = 42;

            var fullyQualifiedLogicalNameToIndexMap = new Dictionary<string, int>
            {
                [_remappedFullyQualifiedLogicalName] = remappedIndex
            };

            var visitor = new UpdateIndicesFromLegacyDataVisitor(fullyQualifiedLogicalNameToIndexMap, fileLocationKeyToIndexMap: null, ruleKeyToIndexMap: null);
            visitor.VisitResult(result);

            result.Locations[0].LogicalLocation.Index.Should().Be(remappedIndex);
            result.Locations[0].PhysicalLocation.ArtifactLocation.Index.Should().Be(int.MaxValue);
        }

        [Fact]
        public void UpdateIndicesFromLegacyDataVisitor_RemapsFileLocations()
        {
            Result result = _result.DeepClone();
            int remappedIndex = 42 * 42;

            ArtifactLocation fileLocation = result.Locations[0].PhysicalLocation.ArtifactLocation;

            var fileLocationKeyToIndexMap = new Dictionary<string, int>()
            {
                ["#" + fileLocation.UriBaseId + "#" + fileLocation.Uri.OriginalString] = remappedIndex
            };

            var visitor = new UpdateIndicesFromLegacyDataVisitor(fullyQualifiedLogicalNameToIndexMap: null, fileLocationKeyToIndexMap: fileLocationKeyToIndexMap, ruleKeyToIndexMap: null);
            visitor.VisitResult(result);

            result.Locations[0].LogicalLocation.Index.Should().Be(int.MaxValue);
            result.Locations[0].PhysicalLocation.ArtifactLocation.Index.Should().Be(remappedIndex);
        }

        [Fact]
        public void UpdateIndicesFromLegacyDataVisitor_RemapsRuleIds()
        {
            int remappedIndex = 0;
            string actualRuleId = "ActualId";

            var ruleKeyToIndexMap = new Dictionary<string, int>()
            {
                ["COLLISION"] = remappedIndex
            };

            Result result = _result.DeepClone();
            result.RuleId = "COLLISION";
            result.RuleIndex = -1;

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Rules = new List<ReportingDescriptor>
                        {
                            new ReportingDescriptor { Id = actualRuleId }
                        }
                    }
                },
                Results = new List<Result>
                {
                    result
                }
            };

            var visitor = new UpdateIndicesFromLegacyDataVisitor(fullyQualifiedLogicalNameToIndexMap: null, fileLocationKeyToIndexMap: null, ruleKeyToIndexMap: ruleKeyToIndexMap);
            visitor.VisitRun(run);

            result.RuleId.Should().Be(actualRuleId);
            result.RuleIndex.Should().Be(remappedIndex);
        }

        [Fact]
        public void UpdateIndicesFromLegacyDataVisitor_DoesNotMutateUnrecognizedLogicalLocation()
        {
            Result result = ConstructNewResult();
            Result originalResult = result.DeepClone();

            int remappedIndex = 42 * 3;

            var fullyQualifiedLogicalNameToIndexMap = new Dictionary<string, int>
            {
                [_remappedFullyQualifiedLogicalName] = remappedIndex
            };

            var visitor = new UpdateIndicesFromLegacyDataVisitor(fullyQualifiedLogicalNameToIndexMap, null, null);
            visitor.VisitResult(result);

            result.ValueEquals(originalResult).Should().BeTrue();
        }

        [Fact]
        public void UpdateIndicesFromLegacyDataVisitor_DoesNotMutateUnrecognizedFileLocation()
        {
            Result result = ConstructNewResult();
            Result originalResult = result.DeepClone();

            int remappedIndex = 42 * 2;

            var fileLocationKeyToIndexMap = new Dictionary<string, int>()
            {
                ["#" + _remappedUriBaseId + "#" + _remappedUri] = remappedIndex
            };

            var visitor = new UpdateIndicesFromLegacyDataVisitor(fullyQualifiedLogicalNameToIndexMap: null, fileLocationKeyToIndexMap: fileLocationKeyToIndexMap, null);
            visitor.VisitResult(result);

            result.ValueEquals(originalResult).Should().BeTrue();
        }

        private Result ConstructNewResult()
        {
            var random = new Random();

            return new Result
            {
                Locations = new[]
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = new Uri(Guid.NewGuid().ToString(), UriKind.Relative), UriBaseId = Guid.NewGuid().ToString(), Index = random.Next()}
                        },
                        LogicalLocation = new LogicalLocation
                        {
                            FullyQualifiedName = Guid.NewGuid().ToString(),
                            Index = random.Next()
                        }
                    }
                }
            };
        }
    }
}
