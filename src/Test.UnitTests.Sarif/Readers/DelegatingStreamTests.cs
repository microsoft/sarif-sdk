// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Readers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class DelegatingStreamTests
    {
        private const int position = 3;
        private const string text = "Hello";

        [Fact]
        public void DelegatingStream_BasicRead()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);

            using var streamReader = new StreamReader(delegatingStream);
            Assert.Equal(text, streamReader.ReadToEnd());
            Assert.Equal(text.Length, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStream_BasicSeek_ShouldGenerateException()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);

            Exception exception = Record.Exception(() => delegatingStream.Position = position);

            exception.Should().BeOfType(typeof(NotImplementedException));
        }

        [Fact]
        public void DelegatingStream_DontPerturbPositionOnCtor()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            memoryStream.Position = position;
            var delegatingStream = new DelegatingStream(memoryStream);

            Assert.Equal(position, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void DelegatingStream_NonSeekable()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var nonSeekableStream = new NonSeekableStream(memoryStream);
            var delegatingStream = new DelegatingStream(nonSeekableStream);

            using var streamReader = new StreamReader(delegatingStream);
            Assert.Equal(text, streamReader.ReadToEnd());
        }
    }
}
