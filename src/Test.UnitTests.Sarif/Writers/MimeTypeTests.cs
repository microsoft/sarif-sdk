// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class MimeTypeTests
    {
        [Fact]
        public void MimeType_GuessesFromFileName_DisallowsNullStringParameter()
        {
            Assert.Throws<ArgumentNullException>(() => MimeType.DetermineFromFileExtension((string)null));
        }

        [Fact]
        public void MimeType_GuessesFromFileName_DisallowsNullUriParameter()
        {
            Assert.Throws<ArgumentNullException>(() => MimeType.DetermineFromFileExtension((Uri)null));
        }

        [Fact]
        public void MimeType_GuessesFromFileName_Xml()
        {
            Assert.Equal("text/xml", MimeType.DetermineFromFileExtension("example.xml"));
        }

        [Fact]
        public void MimeType_GuessesFromFileName_Other()
        {
            Assert.Equal(MimeType.Binary, MimeType.DetermineFromFileExtension("example.exe"));
        }

        [Fact]
        public void MimeType_GuessesFromFileName_IgnoresCase()
        {
            Assert.Equal("text/xml", MimeType.DetermineFromFileExtension("example.XmL"));
        }

        [Fact]
        public void MimeType_GuessesFromFileName_RequiresPeriod()
        {
            Assert.Equal(MimeType.Binary, MimeType.DetermineFromFileExtension("examplexml"));
        }

        [Fact]
        public void MimeType_GuessesFromFileName_DealsWithTooShort()
        {
            Assert.Equal("text/xml", MimeType.DetermineFromFileExtension(".xml"));
        }

        [Fact]
        public void MimeType_Directory()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Assert.Equal("application/x-directory", MimeType.DetermineFromFileExtension(directory));
        }

        [Fact]
        public void MimeType_DirectoryUri()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Assert.Equal("application/x-directory", MimeType.DetermineFromFileExtension(new Uri(directory)));
        }
    }
}
