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
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42
                            }
                        }
                    }
                },
                Kind = CallTreeNodeKind.Call
            };
            callTreeNode.Location.SetProperty("target", "my_function");

            VerifyConversion(callTreeNode, "my_function");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesCallWithNoCallee()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42
                            }
                        }
                    }
                },
                Kind = CallTreeNodeKind.Call
            };

            VerifyConversion(callTreeNode, "<unknown callee>");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesReturn()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42
                            }
                        }
                    }
                },
                Kind = CallTreeNodeKind.Return
            };

            VerifyConversion(callTreeNode, "Return");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNonCallOrReturnNodes()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42
                            }
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, string.Empty);
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesMissingRegion()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation()
                    }
                },
                Kind = CallTreeNodeKind.Return
            };

            VerifyConversion(callTreeNode, "Return");
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesMessage()
        {
            string snippet = "    contentStores[0] = contentStores[index];";
            string message = "The error happened here.";

            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = message
                        },
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42,
                                Snippet = new FileContent
                                {
                                    Text = snippet
                                }
                            }
                        }
                    }
                }
            };
            callTreeNode.Location.SetProperty("target", "my_function");

            VerifyConversion(callTreeNode, message);
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNullMessage()
        {
            string snippet = "    contentStores[0] = contentStores[index];";
            string message = null;

            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = message
                        },
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42,
                                Snippet = new FileContent
                                {
                                    Text = snippet
                                }
                            }
                        }
                    }
                }
            };
            callTreeNode.Location.SetProperty("target", "my_function");

            VerifyConversion(callTreeNode, snippet.Trim());
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesSnippet()
        {
            string snippet = "    contentStores[0] = contentStores[index];";

            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42,
                                Snippet = new FileContent
                                {
                                    Text = snippet
                                }
                            }
                        }
                    }
                }
            };
            callTreeNode.Location.SetProperty("target", "my_function");

            VerifyConversion(callTreeNode, snippet.Trim());
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNullSnippet()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new CodeFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = 42
                            }
                        }
                    }
                },
                Kind = CallTreeNodeKind.Call
            };
            callTreeNode.Location.SetProperty("target", "my_function");

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