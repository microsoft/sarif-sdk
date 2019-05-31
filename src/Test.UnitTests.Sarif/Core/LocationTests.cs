using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class LocationTests
    {
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
    }
}
