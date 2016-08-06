// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.Converters.UnitTests
{
    public class CallTreeNodeToTextConverterTests
    {
        [Fact]
        public void CallTreeNodeToTextConverter_HandlesCall()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Target = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, "my_function");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesCallWithNoCallee()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, "<unknown callee>");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesReturn()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.CallReturn,
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, "Return");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNonCallOrReturnNodes()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Continuation,
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, "Continuation");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesMissingRegion()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.CallReturn,
                    PhysicalLocation = new PhysicalLocation()
                }
            };

            VerifyConversion(callTreeNode, "Return");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesMessage()
        {
            string snippet = "    contentStores[0] = contentStores[index];";
            string message = "The error happened here.";
            string sourceFile = @"file:///c:/dir1/dir%202\source%20file.cpp";

            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Snippet = snippet,
                    Message = message,
                    Target = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Uri = new System.Uri(sourceFile),
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, message);
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNullMessage()
        {
            string snippet = "    contentStores[0] = contentStores[index];";
            string message = null;
            string sourceFile = @"file:///c:/dir1/dir%202\source%20file.cpp";

            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Snippet = snippet,
                    Message = message,
                    Target = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Uri = new System.Uri(sourceFile),
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, snippet.Trim());
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesSnippet()
        {
            string snippet = "    contentStores[0] = contentStores[index];";
            string sourceFile = @"file:///c:/dir1/dir%202\source%20file.cpp";

            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Snippet = snippet,
                    Target = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Uri = new System.Uri(sourceFile),
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, snippet.Trim());
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNullSnippet()
        {
            string snippet = null;
            string sourceFile = @"file:///c:/dir1/dir%202\source%20file.cpp";

            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Snippet = snippet,
                    Target = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Uri = new System.Uri(sourceFile),
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, "my_function");
        }

        private static void VerifyConversion(CallTreeNode callTreeNode, string expectedText)
        {
            var converter = new CallTreeNodeToTextConverter();

            string text = (string)converter.Convert(callTreeNode, typeof(string), null, CultureInfo.CurrentCulture);

            text.Should().Be(expectedText);
        }

    }
}
