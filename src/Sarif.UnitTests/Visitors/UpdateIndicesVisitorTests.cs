// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class UpdateIndicesVisitorTests
    {
        private readonly string _remappedUriBaseId = Guid.NewGuid().ToString();
        private readonly string _remappedFullyQualifiedName = Guid.NewGuid().ToString();
        private readonly Uri _remappedUri = new Uri(Guid.NewGuid().ToString(), UriKind.Relative);
        private readonly string _remappedFullyQualifiedLogicalName = Guid.NewGuid().ToString();
        private readonly Result _result;

        public UpdateIndicesVisitorTests()
        {
            _result = new Result
            {
                Locations = new[]
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            FileLocation = new FileLocation { Uri = _remappedUri, UriBaseId = _remappedUriBaseId, FileIndex = Int32.MaxValue}
                        },
                        FullyQualifiedLogicalName = _remappedFullyQualifiedLogicalName,
                        LogicalLocationIndex = Int32.MaxValue
                    }
                }
            };
        }


        [Fact]
        public void UpdateIndicesVisitor_FunctionsWithNullMaps()
        {
            var result = _result.DeepClone();

            var visitor = new UpdateIndicesVisitor(null, null);
            visitor.VisitResult(result);

            result.Locations[0].LogicalLocationIndex.Should().Be(Int32.MaxValue);
            result.Locations[0].PhysicalLocation.FileLocation.FileIndex.Should().Be(Int32.MaxValue);
        }

        [Fact]
        public void UpdateIndicesVisitor_RemapsFullyQualifiedogicalLNames()
        {
            var result = _result.DeepClone();
            int remappedIndex = 42;

            var fullyQualifiedLogicalNameToIndexMap = new Dictionary<string, int>
            {
                [_remappedFullyQualifiedLogicalName] = remappedIndex
            };

            var visitor = new UpdateIndicesVisitor(fullyQualifiedLogicalNameToIndexMap, fileLocationToIndexMap: null);
            visitor.VisitResult(result);

            result.Locations[0].LogicalLocationIndex.Should().Be(remappedIndex);
            result.Locations[0].PhysicalLocation.FileLocation.FileIndex.Should().Be(Int32.MaxValue);
        }

        [Fact]
        public void UpdateIndicesVisitor_RemapsFileLocations()
        {
            var result = _result.DeepClone();
            int remappedIndex = 42 * 42;

            var fileLocationToIndexMap = new Dictionary<FileLocation, int>()
            {
                [result.Locations[0].PhysicalLocation.FileLocation] = remappedIndex
            };

            var visitor = new UpdateIndicesVisitor(fullyQualifiedLogicalNameToIndexMap: null, fileLocationToIndexMap: fileLocationToIndexMap);
            visitor.VisitResult(result);

            result.Locations[0].LogicalLocationIndex.Should().Be(Int32.MaxValue);
            result.Locations[0].PhysicalLocation.FileLocation.FileIndex.Should().Be(remappedIndex);
        }

        [Fact]
        public void UpdateIndicesVisitor_DoesNotMutateUnrecognizedLogicalLocation()
        {
            var result = ConstructNewResult();
            Result originalResult = result.DeepClone();

            int remappedIndex = 42 * 3;

            var fullyQualifiedLogicalNameToIndexMap = new Dictionary<string, int>
            {
                [_remappedFullyQualifiedLogicalName] = remappedIndex
            };

            var visitor = new UpdateIndicesVisitor(fullyQualifiedLogicalNameToIndexMap, null);
            visitor.VisitResult(result);

            result.ValueEquals(originalResult).Should().BeTrue();
        }

        [Fact]
        public void UpdateIndicesVisitor_DoesNotMutateUnrecognizedFileLocation()
        {
            var result = ConstructNewResult();
            Result originalResult = result.DeepClone();

            int remappedIndex = 42 * 2;

            var fileLocationToIndexMap = new Dictionary<FileLocation, int>(FileLocation.ValueComparer)
            {
                [new FileLocation { Uri = _remappedUri, UriBaseId = _remappedUriBaseId }] = remappedIndex
            };

            var visitor = new UpdateIndicesVisitor(fullyQualifiedLogicalNameToIndexMap: null, null);
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
                            FileLocation = new FileLocation { Uri = new Uri(Guid.NewGuid().ToString(), UriKind.Relative), UriBaseId = Guid.NewGuid().ToString(), FileIndex = random.Next()}
                        },
                        FullyQualifiedLogicalName = Guid.NewGuid().ToString(),
                        LogicalLocationIndex = random.Next()
                    }
                }
            };
        }
    }
}
