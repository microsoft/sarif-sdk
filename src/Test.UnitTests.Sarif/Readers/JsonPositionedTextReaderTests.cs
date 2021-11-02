// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Readers
{
    public class JsonPositionedTextReaderTests
    {
        [Fact]
        public void ThrowsExceptionWithNonSeekable()
        {
            Assert.Throws<ArgumentException>(() => JsonPositionedTextReader.FromStream(new NonSeekableStream(new MemoryStream())));
        }
    }
}
