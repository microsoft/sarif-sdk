// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class NonDisposingDelegatingStreamTests
    {
        private const int position = 3;
        private const string text = "Hello";

        [Fact]
        public void NonDisposingDelegatingStream_NullStream_ShouldThrowArgumentNullException()
        {
            Exception exception = Record.Exception(() => new NonDisposingDelegatingStream(null));
            exception.Should().BeOfType(typeof(ArgumentNullException));
        }

        [Fact]
        public void NonDisposingDelegatingStream_BasicRead()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var delegatingStream = new NonDisposingDelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);

            using var streamReader = new StreamReader(delegatingStream);
            Assert.Equal(text, streamReader.ReadToEnd());
            Assert.Equal(text.Length, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void NonDisposingDelegatingStream_BasicSeek_ShouldGenerateException()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var delegatingStream = new NonDisposingDelegatingStream(memoryStream);

            Assert.Equal(0, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);

            Exception exception = Record.Exception(() => delegatingStream.Position = position);

            exception.Should().BeOfType(typeof(NotImplementedException));
        }

        [Fact]
        public void NonDisposingDelegatingStream_DontPerturbPositionOnCtor()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            memoryStream.Position = position;
            var delegatingStream = new NonDisposingDelegatingStream(memoryStream);

            Assert.Equal(position, delegatingStream.Position);
            Assert.Equal(memoryStream.Position, delegatingStream.Position);
        }

        [Fact]
        public void NonDisposingDelegatingStream_NonSeekable()
        {
            Stream stream = GenerateNonSeekableStream();
            stream.CanSeek.Should().BeFalse();

            var delegatingStream = new NonDisposingDelegatingStream(stream);
            delegatingStream.CanSeek.Should().BeFalse();

            Exception exception = Record.Exception(() => new StreamReader(delegatingStream));
            exception.Should().BeOfType(typeof(ArgumentException));
        }

        [Fact]
        public void NonDisposingDelegatingStream_VerifyingDipose()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var delegatingStream = new NonDisposingDelegatingStream(memoryStream);

            delegatingStream.Dispose();
            memoryStream.Length.Should().NotBe(0);

            delegatingStream.Dispose();
            memoryStream.Length.Should().NotBe(0);

            delegatingStream.DisposeUnderlyingStream();

            Exception exception = Record.Exception(() => memoryStream.Length.Should().NotBe(0));
            exception.Should().BeOfType(typeof(ObjectDisposedException));
        }

        internal static Stream GenerateNonSeekableStream()
        {
            var memoryStream = new MemoryStream();

            using var aes = Aes.Create();
            aes.Key = new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
            };

            byte[] iv = aes.IV;
            memoryStream.Write(iv, 0, iv.Length);

            var cryptoStream = new CryptoStream(
                memoryStream,
                aes.CreateEncryptor(),
                CryptoStreamMode.Write);

            using var encryptWriter = new StreamWriter(cryptoStream);
            encryptWriter.WriteLine("Hello World!");

            return cryptoStream;
        }
    }
}
