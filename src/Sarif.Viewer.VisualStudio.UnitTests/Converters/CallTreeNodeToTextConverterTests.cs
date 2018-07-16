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
        public void CallTreeNodeToTextConverter_HandlesLocationMessage()
        {
            string message = "my_function";

            var callTreeNode = new CallTreeNode
            {
                Location = new ThreadFlowLocation
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
                                StartLine = 42
                            }
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, message);
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesRegionSnippet()
        {
            string snippet = "    int x = 42;";

            var callTreeNode = new CallTreeNode
            {
                Location = new ThreadFlowLocation
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

            VerifyConversion(callTreeNode, snippet.Trim());
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNoMessageNorSnippet()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new ThreadFlowLocation
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

            VerifyConversion(callTreeNode, Microsoft.Sarif.Viewer.Resources.ContinuingCallTreeNodeMessage);
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesMessageAndSnippet()
        {
            string snippet = "    int x = 42;";
            string message = "my_function";

            var callTreeNode = new CallTreeNode
            {
                Location = new ThreadFlowLocation
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

            VerifyConversion(callTreeNode, message);
        }

        [Fact]
        public void CallTreeNodeToTextConverter_HandlesNullMessage()
        {
            string snippet = "    int x = 42;";
            string message = null;

            var callTreeNode = new CallTreeNode
            {
                Location = new ThreadFlowLocation
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

            VerifyConversion(callTreeNode, snippet.Trim());
        }

        private static void VerifyConversion(CallTreeNode callTreeNode, string expectedText)
        {
            var converter = new CallTreeNodeToTextConverter();

            string text = (string)converter.Convert(callTreeNode, typeof(string), null, CultureInfo.CurrentCulture);

            text.Should().Be(expectedText);
        }
    }
}