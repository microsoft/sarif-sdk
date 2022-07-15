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
    public class AddressTests
    {
        private const string AbsoluteAddress = "\"AbsoluteAddress\"";
        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(AddressTests));

        [Fact]
        public void Address_VerifyRoundTripFromObject()
        {
            // Arrange
            var address = new Address();

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress = long.MinValue;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress--;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress = int.MinValue;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress--;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress = -2;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress = -1;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: false, expectedAbsoluteAddress: -1);

            // Arrange
            address.AbsoluteAddress = 0;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress = 1;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress = 2;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress = int.MaxValue;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress++;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress = long.MaxValue;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress++;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress = ulong.MaxValue;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);

            // Arrange
            address.AbsoluteAddress++;

            Address_VerifyRoundTripFromObjectHelper(address,
                expectedShouldSerializeAbsoluteAddress: true, expectedAbsoluteAddress: address.AbsoluteAddress);
        }

        [Fact]
        public void Address_VerifyRoundTripFromJson()
        {
            // Arrange
            string json = "{}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: -1,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:{long.MinValue}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: long.MinValue,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:{new BigInteger(long.MinValue) - 1}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: new BigInteger(long.MinValue) - 1,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:{int.MinValue}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: int.MinValue,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:{new BigInteger(int.MinValue) - 1}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: new BigInteger(int.MinValue) - 1,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:-2}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: -2,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:-1}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: -1,
                expectedShouldSerializeAbsoluteAddress: false, expectedReconstructedAbsoluteAddress: -1);

            // Arrange
            json = $"{{{AbsoluteAddress}:0}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: 0,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: 0);

            // Arrange
            json = $"{{{AbsoluteAddress}:1}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: 1,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: 1);

            // Arrange
            json = $"{{{AbsoluteAddress}:2}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: 2,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: 2);

            // Arrange
            json = $"{{{AbsoluteAddress}:{int.MaxValue}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: int.MaxValue,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: int.MaxValue);

            // Arrange
            json = $"{{{AbsoluteAddress}:{new BigInteger(int.MaxValue) + 1}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: new BigInteger(int.MaxValue) + 1,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: new BigInteger(int.MaxValue) + 1);

            // Arrange
            json = $"{{{AbsoluteAddress}:{long.MaxValue}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: long.MaxValue,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: long.MaxValue);

            // Arrange
            json = $"{{{AbsoluteAddress}:{new BigInteger(long.MaxValue) + 1}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: new BigInteger(long.MaxValue) + 1,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: new BigInteger(long.MaxValue) + 1);

            // Arrange
            json = $"{{{AbsoluteAddress}:{ulong.MaxValue}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: ulong.MaxValue,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: ulong.MaxValue);

            // Arrange
            json = $"{{{AbsoluteAddress}:{new BigInteger(ulong.MaxValue) + 1}}}";

            Address_VerifyRoundTripFromJsonHelper(json, expectedAbsoluteAddress: new BigInteger(ulong.MaxValue) + 1,
                expectedShouldSerializeAbsoluteAddress: true, expectedReconstructedAbsoluteAddress: new BigInteger(ulong.MaxValue) + 1);
        }

        [Fact]
        public void Address_VerifyAbleToDeserializeWithBigInteger()
        {
            // Arrange
            string content = s_extractor.GetResourceText("Address_BigInteger.sarif");

            // Act
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(content);

            // Assert
            sarifLog.Runs[0].Results[0].Locations[0].PhysicalLocation.Address.AbsoluteAddress
                .Should().Be(BigInteger.Parse("31197130097450771296369962162453149327732752356239421572342053257324632475324"));
            sarifLog.Runs[0].Results[0].Locations[1].PhysicalLocation.Address.AbsoluteAddress
                .Should().Be(new BigInteger(long.MaxValue) + 1);
            sarifLog.Runs[0].Results[0].Locations[2].PhysicalLocation.Address.AbsoluteAddress
                .Should().Be(new BigInteger(int.MaxValue) + 1);
            sarifLog.Runs[0].Results[0].Locations[3].PhysicalLocation.Address.AbsoluteAddress.Should().Be(2);
            sarifLog.Runs[0].Results[0].Locations[4].PhysicalLocation.Address.AbsoluteAddress.Should().Be(0);
            sarifLog.Runs[0].Results[0].Locations[5].PhysicalLocation.Address.AbsoluteAddress.Should().Be(-1);
        }

        private static void Address_VerifyRoundTripFromObjectHelper(Address address,
            bool expectedShouldSerializeAbsoluteAddress, BigInteger expectedAbsoluteAddress)
        {
            // Act
            bool shouldSerializeAbsoluteAddress = address.ShouldSerializeAbsoluteAddress();

            // Assert
            shouldSerializeAbsoluteAddress.Should().Be(expectedShouldSerializeAbsoluteAddress);

            // Act
            string jsonAddress = JsonConvert.SerializeObject(address);

            // Assert
            jsonAddress.Contains(AbsoluteAddress, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(expectedShouldSerializeAbsoluteAddress);

            // Act
            Address reconstructedAddress = JsonConvert.DeserializeObject<Address>(jsonAddress);

            // Assert
            reconstructedAddress.AbsoluteAddress.Should().Be(expectedAbsoluteAddress);
            reconstructedAddress.ShouldSerializeAbsoluteAddress().Should().Be(expectedShouldSerializeAbsoluteAddress);

            // Act
            string reconstructedJsonAddress = JsonConvert.SerializeObject(reconstructedAddress);

            // Assert
            reconstructedJsonAddress.Contains(AbsoluteAddress, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(expectedShouldSerializeAbsoluteAddress);
        }

        private static void Address_VerifyRoundTripFromJsonHelper(string jsonAddress,
            BigInteger expectedAbsoluteAddress, bool expectedShouldSerializeAbsoluteAddress, BigInteger expectedReconstructedAbsoluteAddress)
        {
            // Act
            Address address = JsonConvert.DeserializeObject<Address>(jsonAddress);

            // Assert
            address.AbsoluteAddress.Should().Be(expectedAbsoluteAddress);

            // Act
            bool shouldSerializeAbsoluteAddress = address.ShouldSerializeAbsoluteAddress();

            // Assert
            shouldSerializeAbsoluteAddress.Should().Be(expectedShouldSerializeAbsoluteAddress);

            // Act
            string reconstructedJsonAddress = JsonConvert.SerializeObject(address);

            // Assert
            reconstructedJsonAddress.Contains(AbsoluteAddress, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(expectedShouldSerializeAbsoluteAddress);

            // Act
            Address reconstructedAddress = JsonConvert.DeserializeObject<Address>(reconstructedJsonAddress);

            // Assert
            reconstructedAddress.AbsoluteAddress.Should().Be(expectedReconstructedAbsoluteAddress);
        }
    }
}
