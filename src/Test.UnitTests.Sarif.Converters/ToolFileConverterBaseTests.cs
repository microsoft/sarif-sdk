// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// The logic for populating the logicalLocations array is encapsulated
    /// in the abstract base class <see cref="ToolFileConverterBase"/>, from which
    /// converters such as <see cref="FxCopConverter"/> and <see cref="AndroidStudioConverter"/>
    /// derive. These unit tests exercise that logic.
    /// </summary>
    public class ToolFileConverterBaseTests
    {
        // A do-nothing converter, to demonstrate that the base class logic to
        // populate the logicalLocations array is independent of any
        // particular converter.
        private class LogicalLocationTestConverter : ToolFileConverterBase
        {
            public override string ToolName => throw new NotImplementedException();

            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ConverterBase_SingleLogicalLocation()
        {
            Location location = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "a"
                }
            };

            var logicalLocation = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Namespace
            };

            var converter = new LogicalLocationTestConverter();

            int index = converter.AddLogicalLocation(logicalLocation);
            index.Should().Be(0);

            converter.LogicalLocations.Count.Should().Be(1);
            converter.LogicalLocations[index].Name.Should().Be("a");
            converter.LogicalLocations[index].ValueEquals(logicalLocation).Should().BeTrue();
        }

        [Fact]
        public void ConverterBase_TwoIdenticalLogicalLocations()
        {
            Location location1 = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "a"
                }
            };

            Location location2 = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "a"
                }
            };

            var logicalLocation1 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Namespace
            };

            var logicalLocation2 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Namespace
            };

            var converter = new LogicalLocationTestConverter();

            int index = converter.AddLogicalLocation(logicalLocation1);
            index.Should().Be(0);

            // Second logical location is identical to previous. So 
            // we shouldn't add a new instance
            index = converter.AddLogicalLocation(logicalLocation2);
            index.Should().Be(0);

            converter.LogicalLocations.Count.Should().Be(1);
            converter.LogicalLocations[index].Name.Should().Be("a");
            converter.LogicalLocations[index].ValueEquals(logicalLocation1).Should().BeTrue();
        }

        [Fact]
        public void ConverterBase_MultipleDistinctIdenticallyNamedLogicalLocations()
        {
            Location location1 = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "a"
                }
            };

            var logicalLocation1 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Namespace
            };

            Location location2 = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "a"
                }
            };

            var logicalLocation2 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Package
            };

            Location location3 = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = "a"
                }
            };

            var logicalLocation3 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Member
            };

            var converter = new LogicalLocationTestConverter();

            int index = converter.AddLogicalLocation(logicalLocation1);
            converter.LogicalLocations[index].Name.Should().Be("a");
            converter.LogicalLocations[index].FullyQualifiedName.Should().BeNull();

            index = converter.AddLogicalLocation(logicalLocation2);
            index.Should().Be(1);
            converter.LogicalLocations[index].Name.Should().Be("a");
            converter.LogicalLocations.Count.Should().Be(2);

            index = converter.AddLogicalLocation(logicalLocation3);
            index.Should().Be(2);
            converter.LogicalLocations[index].Name.Should().Be("a");
            converter.LogicalLocations.Count.Should().Be(3);
        }
    }
}
