// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class PeekableStreamTests
    {
        private static readonly byte[] ascendingData = new byte[256];

        static PeekableStreamTests()
        {
            for (int i = 0; i < ascendingData.Length; i++)
            {
                ascendingData[i] = (byte)i;
            }
        }

        [Fact]
        public void PeekableStream_BoundaryRewinds()
        {
            var sb = new StringBuilder();

            for (int rewindPoint = 16; rewindPoint < ascendingData.Length; rewindPoint++)
            {
                var testStream = new PeekableStream(new MemoryStream(ascendingData), rewindPoint);

                byte[] peekBuffer = new byte[rewindPoint];
                int read = testStream.Read(peekBuffer, 0, peekBuffer.Length);
                for (int i = 0; i < peekBuffer.Length; i++)
                {
                    if (ascendingData[i] != peekBuffer[i])
                    {
                        sb.AppendLine($"Unexpected data in peekat position {i}: '{ascendingData[i]}' != '{peekBuffer[i]}' ");
                    }
                }

                testStream.Rewind();

                byte[] target = new byte[ascendingData.Length];
                var targetStream = new MemoryStream(target);

                const int ReadCount = 7;
                testStream.CopyTo(targetStream, ReadCount);

                for (int i = 0; i < target.Length; i++)
                {
                    if (ascendingData[i] != target[i])
                    {
                        sb.AppendLine($"Unexpected data in target at position {i}: '{ascendingData[i]}' != '{target[i]}' ");
                    }
                }
            }

            sb.Length.Should().Be(0, sb.ToString());
        }

        [Fact]
        public void PeekableStream_SerialReadsBeforeSwitchover()
        {
            var sb = new StringBuilder();

            var testStream = new PeekableStream(new MemoryStream(ascendingData), ascendingData.Length * 2);

            int rewindPoint = ascendingData.Length;
            for (int readCount = 1; readCount < ascendingData.Length; readCount++)
            {
                byte[] target = new byte[ascendingData.Length];
                var targetStream = new MemoryStream(target);

                testStream.CopyTo(targetStream, readCount);

                for (int i = 0; i < target.Length; i++)
                {
                    if (ascendingData[i] != target[i])
                    {
                        sb.AppendLine($"Unexpected data in target at position {i}: '{ascendingData[i]}' != '{target[i]}' ");
                    }
                }

                testStream.Rewind();
            }

            sb.Length.Should().Be(0, sb.ToString());
        }

        [Fact]
        public void PeekableStream_RewindBoundaryEnforced()
        {
            var sb = new StringBuilder();

            int maxRewind = ascendingData.Length / 2;
            var testStream = new PeekableStream(new MemoryStream(ascendingData), maxRewind);
            byte[] buffer = new byte[ascendingData.Length];

            int actualRead = testStream.Read(buffer, 0, maxRewind + 1);

            // Implementation-specific: we expect a partial read here.
            Assert.Equal(actualRead, maxRewind);

            actualRead = testStream.Read(buffer, 0, 1);
            Assert.Equal(1, actualRead);

            try
            {
                testStream.Rewind();
                Assert.Fail("Rewinding prohibited after max rewind point.");
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Fact]
        public void PeekableStream_Dispose()
        {
            var memStream = new MemoryStream(ascendingData);
            var peekable = new PeekableStream(memStream, 16);
            peekable.Dispose();

            try
            {
                memStream.Read(new byte[16], 0, 16);
                Assert.Fail("Expected ObjectDisposedException.");
            }
            catch (ObjectDisposedException) { }
        }
    }
}
