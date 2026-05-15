// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

using FluentAssertions;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Core
{
    /// <summary>
    /// Tests covering SDK-G: <see cref="SarifLog.Load(Stream, bool)"/> now throws a typed
    /// <see cref="JsonReaderException"/> when deserialization yields a null SarifLog,
    /// instead of silently returning null and pushing the NRE risk to every caller.
    /// </summary>
    public class SarifLogLoadContractTests
    {
        private static Stream StreamOf(string content)
            => new MemoryStream(Encoding.UTF8.GetBytes(content ?? string.Empty));

        [Fact]
        public void Load_ThrowsTypedError_OnJsonNullLiteral()
        {
            using Stream s = StreamOf("null");

            FluentActions.Invoking(() => SarifLog.Load(s))
                .Should().Throw<JsonReaderException>()
                .WithMessage("*not a valid 'sarifLog'*");
        }

        [Fact]
        public void Load_ThrowsTypedError_OnEmptyStream()
        {
            using Stream s = StreamOf(string.Empty);

            FluentActions.Invoking(() => SarifLog.Load(s))
                .Should().Throw<JsonReaderException>();
        }

        [Fact]
        public void Load_DoesNotReturnNull_ForValidLog()
        {
            using Stream s = StreamOf("{\"version\":\"2.1.0\",\"runs\":[]}");

            SarifLog log = SarifLog.Load(s);

            log.Should().NotBeNull();
            log.Runs.Should().NotBeNull();
        }

        [Fact]
        public void Load_ExceptionCitesSpecSection()
        {
            using Stream s = StreamOf("null");

            FluentActions.Invoking(() => SarifLog.Load(s))
                .Should().Throw<JsonReaderException>()
                .Which.Message.Should().Contain("§3.13");
        }
    }
}
