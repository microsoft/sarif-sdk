// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    [TestClass]
    public class EnumConverterTests
    {
        [TestMethod]
        public void EnumConverter_ConvertToPascalCase()
        {
            Assert.AreEqual("M", EnumConverter.ConvertToPascalCase("m"));
            Assert.AreEqual("MD", EnumConverter.ConvertToPascalCase("md"));
            Assert.AreEqual("MD5", EnumConverter.ConvertToPascalCase("md5"));
            Assert.AreEqual("MDFoo", EnumConverter.ConvertToPascalCase("mdFoo"));
            Assert.AreEqual("Mfoo", EnumConverter.ConvertToPascalCase("mfoo"));

            // NOTE: our heuristics for identifying two letter terms that
            // require casing as a group are necessarily limited to our 
            // specific cases. Doing a reasonable job of this requires 
            // maintaining a dictionary of two letter words to distinguish
            // them from two letter acronyms (which are cased as a group).
            // Even with a dictionary, there is overlap, such as with 
            // Io, a moon of jupiter, and IO.

            Assert.AreEqual("METoo", EnumConverter.ConvertToPascalCase("meToo"));
        }
    
        [TestMethod]
        public void EnumConverter_ConvertToCamelCase()
        {
            Assert.AreEqual("m", EnumConverter.ConvertToCamelCase("M"));
            Assert.AreEqual("md", EnumConverter.ConvertToCamelCase("MD"));
            Assert.AreEqual("md5", EnumConverter.ConvertToCamelCase("MD5"));
            Assert.AreEqual("mdFoo", EnumConverter.ConvertToCamelCase("MDFoo"));
            Assert.AreEqual("mfoo", EnumConverter.ConvertToCamelCase("Mfoo"));

            // NOTE: our heuristics for identifying two letter terms that
            // require casing as a group are necessarily limited to our 
            // specific cases. Doing a reasonable job of this requires 
            // maintaining a dictionary of two letter words to distinguish
            // them from two letter acronyms (which are cased as a group).
            // Even with a dictionary, there is overlap, such as with 
            // Io, a moon of jupiter, and IO.

            Assert.AreEqual("meToo", EnumConverter.ConvertToCamelCase("METoo"));
            Assert.AreEqual("meToo", EnumConverter.ConvertToCamelCase("MeToo"));
        }
    }
}