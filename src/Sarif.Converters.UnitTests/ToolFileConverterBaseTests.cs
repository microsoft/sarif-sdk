// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// The logic for populating the logicalLocations dictionary is encapsulated
    /// in the abstract base class <see cref="ToolFileConverterBase"/>, from which
    /// converters such as <see cref="FxCopConverter"/> and <see cref="AndroidStudioConverter"/>
    /// derive. These unit tests exercise that logic.
    /// </summary>
    [TestClass]
    public class ToolFileConverterBaseTests
    {
        // A do-nothing converter, to demonstrate that the base class logic to
        // populate the logicalLocations dictionary is independent of any
        // particular converter.
        private class LogicalLocationTestConverter : ToolFileConverterBase
        {
            public override void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ConverterBase_SingleLogicalLocation()
        {
            Location location = new Location
            {
                FullyQualifiedLogicalName = "a"
            };

            var logicalLocation = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Namespace
            };

            var converter = new LogicalLocationTestConverter();

            string logicalLocationKey = converter.AddLogicalLocation(logicalLocation);

            location.FullyQualifiedLogicalName.Should().Be(logicalLocationKey);

            converter.LogicalLocationsDictionary.Keys.Count.Should().Be(1);
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a");
            converter.LogicalLocationsDictionary["a"].ValueEquals(logicalLocation).Should().BeTrue();
        }

        [TestMethod]
        public void ConverterBase_TwoIdenticalLogicalLocations()
        {
            Location location1 = new Location
            {
                FullyQualifiedLogicalName = "a"
            };

            Location location2 = new Location
            {
                FullyQualifiedLogicalName = "a"
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

            string logicalLocationKey = converter.AddLogicalLocation(logicalLocation1);
            logicalLocationKey.Should().Be(location1.FullyQualifiedLogicalName);

            logicalLocationKey = converter.AddLogicalLocation(logicalLocation2);
            logicalLocationKey.Should().Be(location2.FullyQualifiedLogicalName);

            converter.LogicalLocationsDictionary.Keys.Count.Should().Be(1);
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a");
            converter.LogicalLocationsDictionary["a"].ValueEquals(logicalLocation1).Should().BeTrue();
            converter.LogicalLocationsDictionary["a"].ValueEquals(logicalLocation2).Should().BeTrue();
        }

        [TestMethod]
        public void ConverterBase_MultipleDistinctIdenticallyNamedLogicalLocations()
        {
            Location location1 = new Location
            {
                FullyQualifiedLogicalName = "a"
            };

            var logicalLocation1 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Namespace
            };

            Location location2 = new Location
            {
                FullyQualifiedLogicalName = "a"
            };

            var logicalLocation2 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Package
            };

            Location location3 = new Location
            {
                FullyQualifiedLogicalName = "a"
            };

            var logicalLocation3 = new LogicalLocation
            {
                Name = "a",
                Kind = LogicalLocationKind.Member
            };

            var converter = new LogicalLocationTestConverter();

            string logicalLocationKey = converter.AddLogicalLocation(logicalLocation1);
            logicalLocationKey.Should().Be("a");

            logicalLocationKey = converter.AddLogicalLocation(logicalLocation2);
            logicalLocationKey.Should().Be("a-0");

            logicalLocationKey = converter.AddLogicalLocation(logicalLocation3);
            logicalLocationKey.Should().Be("a-1");

            converter.LogicalLocationsDictionary.Keys.Count.Should().Be(3);
            converter.LogicalLocationsDictionary["a"].ValueEquals(logicalLocation1).Should().BeTrue();
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a-0");
            converter.LogicalLocationsDictionary["a-0"].ValueEquals(logicalLocation2).Should().BeTrue();
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a-1");
            converter.LogicalLocationsDictionary["a-1"].ValueEquals(logicalLocation3).Should().BeTrue();
        }
    }
}
