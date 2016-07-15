// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
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
                    Callee = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, "42: my_function");
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

            VerifyConversion(callTreeNode, "42: <unknown callee>");
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

            VerifyConversion(callTreeNode, "42: Return");
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

            VerifyConversion(callTreeNode, "42: Continuation");
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

        private static void VerifyConversion(CallTreeNode callTreeNode, string expectedText)
        {
            var converter = new CallTreeNodeToTextConverter();

            string text = (string)converter.Convert(callTreeNode, typeof(string), null, CultureInfo.CurrentCulture);

            text.Should().Be(expectedText);
        }

    }
}
