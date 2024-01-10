// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class PeekableStreamTests
    {
        [Fact]
        public void PeekableStream_BoundaryRewinds()
        {
            var sb = new StringBuilder();
            byte[] ascendingData = new byte[256];
            for (int i = 0; i < ascendingData.Length; i++)
            {
                ascendingData[i] = (byte)i;
            }

            for (int rewindPoint = 16; rewindPoint < ascendingData.Length; rewindPoint++)
            {
                var testStream = new PeekableStream(new MemoryStream(ascendingData), rewindPoint);

                byte[] peekBuffer = new byte[rewindPoint];
                testStream.Read(peekBuffer, 0, peekBuffer.Length);
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

                for (int i = 0; i <  target.Length; i++)
                {
                    if (ascendingData[i] != target[i])
                    {
                        sb.AppendLine($"Unexpected data in target at position {i}: '{ascendingData[i]}' != '{target[i]}' ");
                    }
                }
            }

            sb.Length.Should().Be(0, sb.ToString());
        }
    }
}
