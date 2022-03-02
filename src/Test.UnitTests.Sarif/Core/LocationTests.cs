// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Numerics;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class LocationTests
    {
        private const string Id = "\"Id\"";

        [Fact]
        public void Location_LogicalLocation_WhenLogicalLocationsIsAbsent_ReturnsNull()
        {
            var location = new Location();

            location.LogicalLocation.Should().BeNull();
        }

        [Fact]
        public void Location_LogicalLocation_WhenLogicalLocationsIsPresent_ReturnsFirstArrayElement()
        {
            var location = new Location
            {
                LogicalLocations = new LogicalLocation[]
                {
                    new LogicalLocation
                    {
                        FullyQualifiedName = "A.B"
                    },
                    new LogicalLocation
                    {
                        FullyQualifiedName = "C.D"
                    }
                }
            };

            location.LogicalLocation.FullyQualifiedName.Should().Be("A.B");
        }

        [Fact]
        public void Location_LogicalLocation_WhenSetToNonNull_ReplacesLogicalLocationsArray()
        {
            var location = new Location
            {
                LogicalLocations = new LogicalLocation[]
                {
                    new LogicalLocation
                    {
                        FullyQualifiedName = "A.B"
                    },
                    new LogicalLocation
                    {
                        FullyQualifiedName = "C.D"
                    }
                }
            };

            location.LogicalLocation = new LogicalLocation
            {
                FullyQualifiedName = "X.Y"
            };

            location.LogicalLocations.Count.Should().Be(1);
            location.LogicalLocations[0].FullyQualifiedName.Should().Be("X.Y");
        }

        [Fact]
        public void Location_LogicalLocation_WhenSetToNull_RemovesLogicalLocationsArray()
        {
            var location = new Location
            {
                LogicalLocations = new LogicalLocation[]
                {
                    new LogicalLocation
                    {
                        FullyQualifiedName = "A.B"
                    }
                }
            };

            location.LogicalLocation = null;

            location.LogicalLocations.Should().BeNull();
        }

        [Fact]
        public void Location_SerializeId()
        {
            var location = new Location();
            AssertShouldSerializeId(location, false);

            location.Id = -1;
            AssertShouldSerializeId(location, false);

            location.Id = 0;
            AssertShouldSerializeId(location, true);

            location.Id = 1;
            AssertShouldSerializeId(location, true);

            location.Id = int.MaxValue;
            AssertShouldSerializeId(location, true);

            location.Id = long.MaxValue;
            AssertShouldSerializeId(location, true);

            location.Id++;
            AssertShouldSerializeId(location, true);
        }

        [Fact]
        public void Location_DeserializeId()
        {
            string jsonLocation = "{}";
            AssertDeserializeId(jsonLocation, -1);

            jsonLocation = "{\"id\":-1}";
            AssertDeserializeId(jsonLocation, -1);

            jsonLocation = "{\"id\":0}";
            AssertDeserializeId(jsonLocation, 0);

            jsonLocation = "{\"id\":1}";
            AssertDeserializeId(jsonLocation, 1);

            jsonLocation = $"{{\"id\":{int.MaxValue}}}";
            AssertDeserializeId(jsonLocation, int.MaxValue);

            jsonLocation = $"{{\"id\":{long.MaxValue}}}";
            AssertDeserializeId(jsonLocation, long.MaxValue);

            jsonLocation = $"{{\"id\":{new BigInteger(long.MaxValue) + 1}}}";
            AssertDeserializeId(jsonLocation, new BigInteger(long.MaxValue) + 1);
        }

        private void AssertShouldSerializeId(Location location, bool should = true)
        {
            Assert.True(should == location.ShouldSerializeId());
            string testSerializedString = JsonConvert.SerializeObject(location);
            Assert.True(should == testSerializedString.Contains(Id, StringComparison.InvariantCultureIgnoreCase));
        }

        private void AssertDeserializeId(string jsonLocation, BigInteger id)
        {
            Location location = JsonConvert.DeserializeObject<Location>(jsonLocation);
            Assert.True(location.Id == id);
        }
    }
}
