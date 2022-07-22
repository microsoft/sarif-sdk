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
            var testCases = new[] {
                new { absoluteAddress = (BigInteger?)null, expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)long.MinValue - 1, expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)long.MinValue, expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)int.MinValue - 1, expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)int.MinValue, expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)(-2), expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)(-1), expectedShouldSerializeAbsoluteAddress = false },
                new { absoluteAddress = (BigInteger?)0, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)1, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)2, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)int.MaxValue, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)int.MaxValue + 1, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)long.MaxValue, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)long.MaxValue + 1, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)ulong.MaxValue, expectedShouldSerializeAbsoluteAddress = true },
                new { absoluteAddress = (BigInteger?)ulong.MaxValue + 1, expectedShouldSerializeAbsoluteAddress = true },
            };

            var address = new Address();
            foreach (var testCase in testCases)
            {
                if (testCase.absoluteAddress.HasValue)
                {
                    address.AbsoluteAddress = testCase.absoluteAddress.Value;
                }

                BigInteger expectedAbsoluteAddress =
                    (!testCase.absoluteAddress.HasValue || testCase.absoluteAddress.Value < 0) ? -1 : testCase.absoluteAddress.Value;

                // Act & Assert
                Address_VerifyRoundTripFromObjectHelper(address,
                    expectedShouldSerializeAbsoluteAddress: testCase.expectedShouldSerializeAbsoluteAddress,
                    expectedAbsoluteAddress: expectedAbsoluteAddress);
            }

        }

        [Fact]
        public void Address_VerifyRoundTripFromJson()
        {
            // Arrange
            var testCases = new[] {
                new { json = "{}",
                    expectedAbsoluteAddress = (BigInteger)(-1),
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:{new BigInteger(long.MinValue) - 1}}}",
                    expectedAbsoluteAddress = (BigInteger)long.MinValue - 1,
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:{long.MinValue}}}",
                    expectedAbsoluteAddress = (BigInteger)long.MinValue,
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:{new BigInteger(int.MinValue) - 1}}}",
                    expectedAbsoluteAddress = (BigInteger)int.MinValue - 1,
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:{int.MinValue}}}",
                    expectedAbsoluteAddress = (BigInteger)int.MinValue,
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:-2}}",
                    expectedAbsoluteAddress = (BigInteger)(-2),
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:-1}}",
                    expectedAbsoluteAddress = (BigInteger)(-1),
                    expectedShouldSerializeAbsoluteAddress = false,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(-1) },
                new { json = $"{{{AbsoluteAddress}:0}}",
                    expectedAbsoluteAddress = (BigInteger)(0),
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(0) },
                new { json = $"{{{AbsoluteAddress}:1}}",
                    expectedAbsoluteAddress = (BigInteger)(1),
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(1) },
                new { json = $"{{{AbsoluteAddress}:2}}",
                    expectedAbsoluteAddress = (BigInteger)(2),
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)(2) },
                new { json = $"{{{AbsoluteAddress}:{int.MaxValue}}}",
                    expectedAbsoluteAddress = (BigInteger)int.MaxValue,
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)int.MaxValue },
                new { json = $"{{{AbsoluteAddress}:{new BigInteger(int.MaxValue) + 1}}}",
                    expectedAbsoluteAddress = (BigInteger)int.MaxValue + 1,
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)int.MaxValue + 1 },
                new { json = $"{{{AbsoluteAddress}:{long.MaxValue}}}",
                    expectedAbsoluteAddress = (BigInteger)long.MaxValue,
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)long.MaxValue },
                new { json = $"{{{AbsoluteAddress}:{new BigInteger(long.MaxValue) + 1}}}",
                    expectedAbsoluteAddress = (BigInteger)long.MaxValue + 1,
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)long.MaxValue + 1 },
                new { json = $"{{{AbsoluteAddress}:{ulong.MaxValue}}}",
                    expectedAbsoluteAddress = (BigInteger)ulong.MaxValue,
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)ulong.MaxValue },
                new { json = $"{{{AbsoluteAddress}:{new BigInteger(ulong.MaxValue) + 1}}}",
                    expectedAbsoluteAddress = (BigInteger)ulong.MaxValue + 1,
                    expectedShouldSerializeAbsoluteAddress = true,
                    expectedReconstructedAbsoluteAddress = (BigInteger)ulong.MaxValue + 1 },
            };

            foreach (var testCase in testCases)
            {
                // Act & Assert
                Address_VerifyRoundTripFromJsonHelper(
                    jsonAddress: testCase.json,
                    expectedAbsoluteAddress: testCase.expectedAbsoluteAddress,
                    expectedShouldSerializeAbsoluteAddress: testCase.expectedShouldSerializeAbsoluteAddress,
                    expectedReconstructedAbsoluteAddress: testCase.expectedReconstructedAbsoluteAddress);
            }
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
