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
            var address = new Address();
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            address.AbsoluteAddress = long.MinValue;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);
            address.AbsoluteAddress--;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            address.AbsoluteAddress = int.MinValue;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);
            address.AbsoluteAddress--;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            address.AbsoluteAddress = -2;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            address.AbsoluteAddress = -1;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            address.AbsoluteAddress = 0;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);

            address.AbsoluteAddress = 1;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);

            address.AbsoluteAddress = 2;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);

            address.AbsoluteAddress = int.MaxValue;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);
            address.AbsoluteAddress++;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);

            address.AbsoluteAddress = long.MaxValue;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);
            address.AbsoluteAddress++;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);

            address.AbsoluteAddress = ulong.MaxValue;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);
            address.AbsoluteAddress++;
            Address_VerifyRoundTripFromObjectHelper(address,
                shouldSerialize: true, reconstructedAbsoluteAddress: address.AbsoluteAddress);
        }

        [Fact]
        public void Address_VerifyRoundTripFromJson()
        {
            string json = "{}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: -1,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:{long.MinValue}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: long.MinValue,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:{new BigInteger(long.MinValue) - 1}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: new BigInteger(long.MinValue) - 1,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:{int.MinValue}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: int.MinValue,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:{new BigInteger(int.MinValue) - 1}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: new BigInteger(int.MinValue) - 1,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:-2}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: -2,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:-1}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: -1,
                shouldSerialize: false, reconstructedAbsoluteAddress: -1);

            json = $"{{{AbsoluteAddress}:0}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: 0,
                shouldSerialize: true, reconstructedAbsoluteAddress: 0);

            json = $"{{{AbsoluteAddress}:1}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: 1,
                shouldSerialize: true, reconstructedAbsoluteAddress: 1);

            json = $"{{{AbsoluteAddress}:2}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: 2,
                shouldSerialize: true, reconstructedAbsoluteAddress: 2);

            json = $"{{{AbsoluteAddress}:{int.MaxValue}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: int.MaxValue,
                shouldSerialize: true, reconstructedAbsoluteAddress: int.MaxValue);

            json = $"{{{AbsoluteAddress}:{new BigInteger(int.MaxValue) + 1}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: new BigInteger(int.MaxValue) + 1,
                shouldSerialize: true, reconstructedAbsoluteAddress: new BigInteger(int.MaxValue) + 1);

            json = $"{{{AbsoluteAddress}:{long.MaxValue}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: long.MaxValue,
                shouldSerialize: true, reconstructedAbsoluteAddress: long.MaxValue);

            json = $"{{{AbsoluteAddress}:{new BigInteger(long.MaxValue) + 1}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: new BigInteger(long.MaxValue) + 1,
                shouldSerialize: true, reconstructedAbsoluteAddress: new BigInteger(long.MaxValue) + 1);

            json = $"{{{AbsoluteAddress}:{ulong.MaxValue}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: ulong.MaxValue,
                shouldSerialize: true, reconstructedAbsoluteAddress: ulong.MaxValue);

            json = $"{{{AbsoluteAddress}:{new BigInteger(ulong.MaxValue) + 1}}}";
            Address_VerifyRoundTripFromJsonHelper(json, deserializedId: new BigInteger(ulong.MaxValue) + 1,
                shouldSerialize: true, reconstructedAbsoluteAddress: new BigInteger(ulong.MaxValue) + 1);
        }

        [Fact]
        public void Address_VerifyAbleToDeserializeWithBigInteger()
        {
            string content = s_extractor.GetResourceText("Address_BigInteger.sarif");
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(content);
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
            bool shouldSerialize, BigInteger reconstructedAbsoluteAddress)
        {
            address.ShouldSerializeAbsoluteAddress().Should().Be(shouldSerialize);
            string jsonAddress = JsonConvert.SerializeObject(address);
            jsonAddress.Contains(AbsoluteAddress, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(shouldSerialize);

            Address reconstructedAddress = JsonConvert.DeserializeObject<Address>(jsonAddress);
            reconstructedAddress.AbsoluteAddress.Should().Be(reconstructedAbsoluteAddress);

            reconstructedAddress.ShouldSerializeAbsoluteAddress().Should().Be(shouldSerialize);
            string reconstructedJsonAddress = JsonConvert.SerializeObject(reconstructedAddress);
            reconstructedJsonAddress.Contains(AbsoluteAddress, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(shouldSerialize);
        }

        private static void Address_VerifyRoundTripFromJsonHelper(string jsonAddress,
            BigInteger deserializedId, bool shouldSerialize, BigInteger reconstructedAbsoluteAddress)
        {
            Address address = JsonConvert.DeserializeObject<Address>(jsonAddress);
            address.AbsoluteAddress.Should().Be(deserializedId);

            address.ShouldSerializeAbsoluteAddress().Should().Be(shouldSerialize);
            string reconstructedJsonAddress = JsonConvert.SerializeObject(address);
            reconstructedJsonAddress.Contains(AbsoluteAddress, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(shouldSerialize);

            Address reconstructedAddress = JsonConvert.DeserializeObject<Address>(reconstructedJsonAddress);
            reconstructedAddress.AbsoluteAddress.Should().Be(reconstructedAbsoluteAddress);
        }
    }
}
