// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Readers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class DelegatingStreamTests
    {
        [Fact]
        public void DelegatingStreamBasicRead()
        {
            const string testStr = "Hello";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            DelegatingStream delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
            using StreamReader streamReader = new StreamReader(delegatingStream);
            Assert.Equal(testStr, streamReader.ReadToEnd());
            Assert.Equal(testStr.Length, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStreamBasicSeek()
        {
            const string testStr = "Hello";
            const int newPosition = 3;
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            DelegatingStream delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);

            delegatingStream.Position = newPosition;

            Assert.Equal(newPosition, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStreamDontPerturbPositionOnCtor()
        {
            const string testStr = "Hello";
            const int startingPosition = 3;
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            memoryStream.Position = startingPosition;
            DelegatingStream delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(startingPosition, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStreamNonSeekable()
        {
            const string testStr = "Hello";
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testStr));
            NonSeekableStream nonSeekableStream = new NonSeekableStream(memoryStream);
            DelegatingStream delegatingStream = new DelegatingStream(nonSeekableStream);

            using StreamReader streamReader = new StreamReader(delegatingStream);
            Assert.Equal(testStr, streamReader.ReadToEnd());
        }
    }
}
