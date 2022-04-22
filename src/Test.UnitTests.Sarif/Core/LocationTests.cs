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
        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(LocationTests));

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
        public void Location_VerifyIdRoundTripFromObject()
        {
            var location = new Location();
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);

            location.Id = long.MinValue;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);
            location.Id--;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);

            location.Id = int.MinValue;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);
            location.Id--;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);

            location.Id = -2;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);

            location.Id = -1;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: false, reconstructedLocationId: -1);

            location.Id = 0;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);

            location.Id = 1;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);

            location.Id = 2;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);

            location.Id = int.MaxValue;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);
            location.Id++;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);

            location.Id = long.MaxValue;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);
            location.Id++;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);

            location.Id = ulong.MaxValue;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);
            location.Id++;
            VerifyIdRoundTripFromObjectHelper(location, shouldSerialize: true, reconstructedLocationId: location.Id);
        }

        [Fact]
        public void Location_VerifyIdRoundTripFromJson()
        {
            string jsonLocation = "{}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: -1,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = $"{{\"id\":{long.MinValue}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: long.MinValue,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = $"{{\"id\":{new BigInteger(long.MinValue) - 1}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: new BigInteger(long.MinValue) - 1,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = $"{{\"id\":{int.MinValue}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: int.MinValue,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = $"{{\"id\":{new BigInteger(int.MinValue) - 1}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: new BigInteger(int.MinValue) - 1,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = "{\"id\":-2}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: -2,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = "{\"id\":-1}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: -1,
                shouldSerialize: false, reconstructedLocationId: -1);

            jsonLocation = "{\"id\":0}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: 0,
                shouldSerialize: true, reconstructedLocationId: 0);

            jsonLocation = "{\"id\":1}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: 1,
                shouldSerialize: true, reconstructedLocationId: 1);

            jsonLocation = "{\"id\":2}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: 2,
                shouldSerialize: true, reconstructedLocationId: 2);

            jsonLocation = $"{{\"id\":{int.MaxValue}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: int.MaxValue,
                shouldSerialize: true, reconstructedLocationId: int.MaxValue);

            jsonLocation = $"{{\"id\":{new BigInteger(int.MaxValue) + 1}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: new BigInteger(int.MaxValue) + 1,
                shouldSerialize: true, reconstructedLocationId: new BigInteger(int.MaxValue) + 1);

            jsonLocation = $"{{\"id\":{long.MaxValue}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: long.MaxValue,
                shouldSerialize: true, reconstructedLocationId: long.MaxValue);

            jsonLocation = $"{{\"id\":{new BigInteger(long.MaxValue) + 1}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: new BigInteger(long.MaxValue) + 1,
                shouldSerialize: true, reconstructedLocationId: new BigInteger(long.MaxValue) + 1);

            jsonLocation = $"{{\"id\":{ulong.MaxValue}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: ulong.MaxValue,
                shouldSerialize: true, reconstructedLocationId: ulong.MaxValue);

            jsonLocation = $"{{\"id\":{new BigInteger(ulong.MaxValue) + 1}}}";
            VerifyIdRoundTripFromJsonHelper(jsonLocation, deserializedId: new BigInteger(ulong.MaxValue) + 1,
                shouldSerialize: true, reconstructedLocationId: new BigInteger(ulong.MaxValue) + 1);
        }

        [Fact]
        public void Location_VerifyAbleToDeserializeWithBigIntegerId()
        {
            string content = s_extractor.GetResourceText("LocationId_BigInteger.sarif");
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(content);
            sarifLog.Runs[0].Results[0].Locations[0].Id.Should().Be(BigInteger.Parse("31197130097450771296369962162453149327732752356239421572342053257324632475324"));
            sarifLog.Runs[0].Results[0].Locations[1].Id.Should().Be(new BigInteger(long.MaxValue) + 1);
            sarifLog.Runs[0].Results[0].Locations[2].Id.Should().Be(new BigInteger(int.MaxValue) + 1);
            sarifLog.Runs[0].Results[0].Locations[3].Id.Should().Be(2);
            sarifLog.Runs[0].Results[0].Locations[4].Id.Should().Be(0);
            sarifLog.Runs[0].Results[0].Locations[5].Id.Should().Be(-1);
        }

        private void VerifyIdRoundTripFromObjectHelper(Location location, bool shouldSerialize, BigInteger reconstructedLocationId)
        {
            location.ShouldSerializeId().Should().Be(shouldSerialize);
            string jsonLocation = JsonConvert.SerializeObject(location);
            jsonLocation.Contains(Id, StringComparison.InvariantCultureIgnoreCase).Should().Be(shouldSerialize);

            Location reconstructedLocation = JsonConvert.DeserializeObject<Location>(jsonLocation);
            reconstructedLocation.Id.Should().Be(reconstructedLocationId);

            reconstructedLocation.ShouldSerializeId().Should().Be(shouldSerialize);
            string reconstructedJsonLocation = JsonConvert.SerializeObject(reconstructedLocation);
            reconstructedJsonLocation.Contains(Id, StringComparison.InvariantCultureIgnoreCase).Should().Be(shouldSerialize);
        }

        private void VerifyIdRoundTripFromJsonHelper(string jsonLocation, BigInteger deserializedId, bool shouldSerialize, BigInteger reconstructedLocationId)
        {
            Location location = JsonConvert.DeserializeObject<Location>(jsonLocation);
            location.Id.Should().Be(deserializedId);

            location.ShouldSerializeId().Should().Be(shouldSerialize);
            string reconstructedJsonLocation = JsonConvert.SerializeObject(location);
            reconstructedJsonLocation.Contains(Id, StringComparison.InvariantCultureIgnoreCase).Should().Be(shouldSerialize);

            Location reconstructedLocation = JsonConvert.DeserializeObject<Location>(reconstructedJsonLocation);
            reconstructedLocation.Id.Should().Be(reconstructedLocationId);
        }
    }
}
