// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintLoaderTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void TSLintLoaderTest_ReadLog_NullInput()
        {
            TSLintLoader loader = new TSLintLoader();

            Assert.Throws<ArgumentNullException>(() => loader.ReadLog(null));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TSLintLoaderTest_ReadLog_ValidInput()
        {
            Mock<XmlObjectSerializer> mockSerializer = new Mock<XmlObjectSerializer>();

            string input = "[{\"endPosition\":{\"character\":1,\"line\":113,\"position\":4429},\"failure\":\"file should end with a newline\",\"fix\":{\"innerStart\":4429,\"innerLength\":0,\"innerText\":\"\r\n\"},\"name\":\"SecureApp/js/index.d.ts\",\"ruleName\":\"eofline\",\"ruleSeverity\":\"ERROR\",\"startPosition\":{\"character\":1,\"line\":113,\"position\":4429}}]";
            string expectedProcessedStream = "[{\"endPosition\":{\"character\":1,\"line\":113,\"position\":4429},\"failure\":\"file should end with a newline\",\"fix\":[{\"innerStart\":4429,\"innerLength\":0,\"innerText\":\"\r\n\"}],\"name\":\"SecureApp/js/index.d.ts\",\"ruleName\":\"eofline\",\"ruleSeverity\":\"ERROR\",\"startPosition\":{\"character\":1,\"line\":113,\"position\":4429}}]";


            byte[] buffer = new byte[expectedProcessedStream.Length];
            using (var ptStream = new MemoryStream(buffer))
            {
                mockSerializer.Setup(serializer => serializer.ReadObject(It.IsAny<Stream>())).Callback<Stream>((s) => s.CopyTo(ptStream));
                TSLintLoader loader = new TSLintLoader(mockSerializer.Object);

                loader.ReadLog(new MemoryStream(Encoding.UTF8.GetBytes(input)));
                mockSerializer.Verify(serializer => serializer.ReadObject(It.IsAny<Stream>()), Times.Once);
            }

            string actualProcessedStream = Encoding.UTF8.GetString(buffer);
            Assert.Equal(expectedProcessedStream, actualProcessedStream);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TSLintLoaderTest_NormalizeTSLintFixFormat_ValidInput()
        {
            Mock<XmlObjectSerializer> mockSerializer = new Mock<XmlObjectSerializer>();
            string input = "[{\"endPosition\":{\"character\":1,\"line\":113,\"position\":4429},\"failure\":\"file should end with a newline\",\"fix\":{\"innerStart\":4429,\"innerLength\":0,\"innerText\":\"\r\n\"},\"name\":\"SecureApp/js/index.d.ts\",\"ruleName\":\"eofline\",\"ruleSeverity\":\"ERROR\",\"startPosition\":{\"character\":1,\"line\":113,\"position\":4429}}]";
            string expectedOutput = "[{\"endPosition\":{\"character\":1,\"line\":113,\"position\":4429},\"failure\":\"file should end with a newline\",\"fix\":[{\"innerStart\":4429,\"innerLength\":0,\"innerText\":\"\r\n\"}],\"name\":\"SecureApp/js/index.d.ts\",\"ruleName\":\"eofline\",\"ruleSeverity\":\"ERROR\",\"startPosition\":{\"character\":1,\"line\":113,\"position\":4429}}]";

            byte[] buffer = new byte[expectedOutput.Length];
            using (var ptStream = new MemoryStream(buffer))
            {
                TSLintLoader loader = new TSLintLoader(mockSerializer.Object);

                loader.NormalizeTSLintFixFormat(new MemoryStream(Encoding.UTF8.GetBytes(input))).CopyTo(ptStream);
            }

            string actualProcessedStream = Encoding.UTF8.GetString(buffer);
            Assert.Equal(expectedOutput, actualProcessedStream);
        }
    }
}
