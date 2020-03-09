// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class EnumConverterTests
    {
        [Fact]
        public void EnumConverter_ConvertToPascalCase()
        {
            Assert.Equal("M", EnumConverter.ConvertToPascalCase("m"));
            Assert.Equal("MD", EnumConverter.ConvertToPascalCase("md"));
            Assert.Equal("MD5", EnumConverter.ConvertToPascalCase("md5"));
            Assert.Equal("MDExample", EnumConverter.ConvertToPascalCase("mdExample"));
            Assert.Equal("Mexample", EnumConverter.ConvertToPascalCase("mexample"));

            // NOTE: our heuristics for identifying two letter terms that
            // require casing as a group are necessarily limited to our 
            // specific cases. Doing a reasonable job of this requires 
            // maintaining a dictionary of two letter words to distinguish
            // them from two letter acronyms (which are cased as a group).
            // Even with a dictionary, there is overlap, such as with 
            // Io, a moon of jupiter, and IO.

            Assert.Equal("METoo", EnumConverter.ConvertToPascalCase("meToo"));
        }

        [Fact]
        public void EnumConverter_ConvertToCamelCase()
        {
            Assert.Equal("m", EnumConverter.ConvertToCamelCase("M"));
            Assert.Equal("md", EnumConverter.ConvertToCamelCase("MD"));
            Assert.Equal("md5", EnumConverter.ConvertToCamelCase("MD5"));
            Assert.Equal("mdExample", EnumConverter.ConvertToCamelCase("MDExample"));
            Assert.Equal("mexample", EnumConverter.ConvertToCamelCase("Mexample"));

            // NOTE: our heuristics for identifying two letter terms that
            // require casing as a group are necessarily limited to our 
            // specific cases. Doing a reasonable job of this requires 
            // maintaining a dictionary of two letter words to distinguish
            // them from two letter acronyms (which are cased as a group).
            // Even with a dictionary, there is overlap, such as with 
            // Io, a moon of jupiter, and IO.

            Assert.Equal("meToo", EnumConverter.ConvertToCamelCase("METoo"));
            Assert.Equal("meToo", EnumConverter.ConvertToCamelCase("MeToo"));
        }
    }
}