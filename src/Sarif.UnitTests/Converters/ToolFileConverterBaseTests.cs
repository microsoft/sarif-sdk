// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
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
            public override void Convert(Stream input, IResultLogWriter output)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ConverterBase_SingleLogicalLocation()
        {
            Location location = new Location
            {
                LogicalLocation = "a"
            };

            LogicalLocationComponent[] logicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "a",
                    Kind = LogicalLocationKind.ClrNamespace
                },
            };

            var converter = new LogicalLocationTestConverter();

            converter.AddLogicalLocation(location, logicalLocationComponents);

            location.LogicalLocationKey.Should().BeNull();

            converter.LogicalLocationsDictionary.Keys.Count.Should().Be(1);
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a");
            converter.LogicalLocationsDictionary["a"].SequenceEqual(logicalLocationComponents).Should().BeTrue();
        }

        [TestMethod]
        public void ConverterBase_TwoIdenticalLogicalLocations()
        {
            Location location1 = new Location
            {
                LogicalLocation = "a"
            };

            Location location2 = new Location
            {
                LogicalLocation = "a"
            };

            LogicalLocationComponent[] logicalLocationComponents = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "a",
                    Kind = LogicalLocationKind.ClrNamespace
                }
            };

            var converter = new LogicalLocationTestConverter();

            converter.AddLogicalLocation(location1, logicalLocationComponents);
            converter.AddLogicalLocation(location2, logicalLocationComponents);

            location1.LogicalLocationKey.Should().BeNull();
            location2.LogicalLocationKey.Should().BeNull();

            converter.LogicalLocationsDictionary.Keys.Count.Should().Be(1);
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a");
            converter.LogicalLocationsDictionary["a"].SequenceEqual(logicalLocationComponents).Should().BeTrue();
        }

        [TestMethod]
        public void ConverterBase_MultipleDistinctIdenticallyNamedLogicalLocations()
        {
            Location location1 = new Location
            {
                LogicalLocation = "a"
            };

            LogicalLocationComponent[] logicalLocationComponents1 = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "a",
                    Kind = LogicalLocationKind.ClrNamespace
                }
            };

            Location location2 = new Location
            {
                LogicalLocation = "a"
            };

            LogicalLocationComponent[] logicalLocationComponents2 = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "a",
                    Kind = LogicalLocationKind.JvmPackage
                }
            };

            Location location3 = new Location
            {
                LogicalLocation = "a"
            };

            LogicalLocationComponent[] logicalLocationComponents3 = new[]
            {
                new LogicalLocationComponent
                {
                    Name = "a",
                    Kind = LogicalLocationKind.AndroidModule
                }
            };

            var converter = new LogicalLocationTestConverter();

            converter.AddLogicalLocation(location1, logicalLocationComponents1);
            converter.AddLogicalLocation(location2, logicalLocationComponents2);
            converter.AddLogicalLocation(location3, logicalLocationComponents3);

            location1.LogicalLocationKey.Should().BeNull();
            location2.LogicalLocationKey.Should().Be("a-0");
            location3.LogicalLocationKey.Should().Be("a-1");

            converter.LogicalLocationsDictionary.Keys.Count.Should().Be(3);
            converter.LogicalLocationsDictionary["a"].SequenceEqual(logicalLocationComponents1).Should().BeTrue();
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a-0");
            converter.LogicalLocationsDictionary["a-0"].SequenceEqual(logicalLocationComponents2).Should().BeTrue();
            converter.LogicalLocationsDictionary.Keys.Should().Contain("a-1");
            converter.LogicalLocationsDictionary["a-1"].SequenceEqual(logicalLocationComponents3).Should().BeTrue();
        }
    }
}
