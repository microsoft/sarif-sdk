// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Moq;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintLoaderTests
    {
        [Fact]
        public void TSLintLoader_ReadLog_WhenInputIsNull_ThrowsArgumentNullException()
        {
            TSLintLoader loader = new TSLintLoader();

            Action action = () => loader.ReadLog(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void TSLintLoader_ReadLog_Passes()
        {
            Mock<XmlObjectSerializer> mockSerializer = new Mock<XmlObjectSerializer>();

            const string Input = @"
            [
                {
                    ""endPosition"":
                        {
                            ""character"":1,
                            ""line"":113,
                            ""position"":4429
                        },
                    ""failure"":""file should end with a newline"",
                    ""fix"":
                    {
                        ""innerStart"":4429,
                        ""innerLength"":0,
                        ""innerText"":""\r\n""
                    },
                    ""name"":""SecureApp/js/index.d.ts"",
                    ""ruleName"":""eofline"",
                    ""ruleSeverity"":""ERROR"",
                    ""startPosition"":
                    {
                        ""character"":1,
                        ""line"":113,
                        ""position"":4429
                    }
                }
            ]";

            const string ExpectedProcessedStream = @"
            [
                {
                    ""endPosition"":
                        {
                            ""character"":1,
                            ""line"":113,
                            ""position"":4429
                        },
                    ""failure"":""file should end with a newline"",
                    ""fix"":
                    [{
                        ""innerStart"":4429,
                        ""innerLength"":0,
                        ""innerText"":""\r\n""
                    }],
                    ""name"":""SecureApp/js/index.d.ts"",
                    ""ruleName"":""eofline"",
                    ""ruleSeverity"":""ERROR"",
                    ""startPosition"":
                    {
                        ""character"":1,
                        ""line"":113,
                        ""position"":4429
                    }
                }
            ]";


            byte[] buffer = new byte[ExpectedProcessedStream.Length];
            using (var ptStream = new MemoryStream(buffer))
            {
                mockSerializer.Setup(serializer => serializer.ReadObject(It.IsAny<Stream>())).Callback<Stream>((s) => s.CopyTo(ptStream));
                TSLintLoader loader = new TSLintLoader(mockSerializer.Object);

                loader.ReadLog(new MemoryStream(Encoding.UTF8.GetBytes(Input)));
                mockSerializer.Verify(serializer => serializer.ReadObject(It.IsAny<Stream>()), Times.Once);
            }

            string actualProcessedStream = Encoding.UTF8.GetString(buffer);
            Assert.Equal(ExpectedProcessedStream, actualProcessedStream);
        }

        [Fact]
        public void TSLintLoader_NormalizeTSLintFixFormat_Passes()
        {
            Mock<XmlObjectSerializer> mockSerializer = new Mock<XmlObjectSerializer>();
            const string Input = @"
            [
                {
                    ""endPosition"":
                        {
                            ""character"":1,
                            ""line"":113,
                            ""position"":4429
                        },
                    ""failure"":""file should end with a newline"",
                    ""fix"":
                    {
                        ""innerStart"":4429,
                        ""innerLength"":0,
                        ""innerText"":""\r\n""
                    },
                    ""name"":""SecureApp/js/index.d.ts"",
                    ""ruleName"":""eofline"",
                    ""ruleSeverity"":""ERROR"",
                    ""startPosition"":
                    {
                        ""character"":1,
                        ""line"":113,
                        ""position"":4429
                    }
                }
            ]";

            const string ExpectedOutput = @"
            [
                {
                    ""endPosition"":
                        {
                            ""character"":1,
                            ""line"":113,
                            ""position"":4429
                        },
                    ""failure"":""file should end with a newline"",
                    ""fix"":
                    [{
                        ""innerStart"":4429,
                        ""innerLength"":0,
                        ""innerText"":""\r\n""
                    }],
                    ""name"":""SecureApp/js/index.d.ts"",
                    ""ruleName"":""eofline"",
                    ""ruleSeverity"":""ERROR"",
                    ""startPosition"":
                    {
                        ""character"":1,
                        ""line"":113,
                        ""position"":4429
                    }
                }
            ]";

            byte[] buffer = new byte[ExpectedOutput.Length];
            using (var ptStream = new MemoryStream(buffer))
            {
                TSLintLoader loader = new TSLintLoader(mockSerializer.Object);

                loader.NormalizeTSLintFixFormat(new MemoryStream(Encoding.UTF8.GetBytes(Input))).CopyTo(ptStream);
            }

            string actualProcessedStream = Encoding.UTF8.GetString(buffer);
            Assert.Equal(ExpectedOutput, actualProcessedStream);
        }
    }
}
