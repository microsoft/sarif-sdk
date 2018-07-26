// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class ThreadFlowToTreeConverterTests
    {
        [Fact]
        public void CanConvertCodeFlowToTree()
        {
            var codeFlow = SarifUtilities.CreateSingleThreadedCodeFlow(new[]
            {
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "first parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "second parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 2, // CallReturn
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "third parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 2, // CallReturn
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "fourth parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 2, // CallReturn
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // CallReturn
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "fifth parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // CallReturn,
                }
            });

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            topLevelNodes.Count.Should().Be(2);
            topLevelNodes[0].Children.Count.Should().Be(4);
            topLevelNodes[0].Children[2].Children.Count.Should().Be(1);

            // Check that we have the right nodes at the right places in the tree.
            topLevelNodes[0].Location.NestingLevel.Should().Be(0);                         // Call
            topLevelNodes[0].Children[0].Location.NestingLevel.Should().Be(1);             // Call
            topLevelNodes[0].Children[0].Children[0].Location.NestingLevel.Should().Be(2); // CallReturn
            topLevelNodes[0].Children[1].Location.NestingLevel.Should().Be(1);             // Call
            topLevelNodes[0].Children[1].Children[0].Location.NestingLevel.Should().Be(2); // CallReturn
            topLevelNodes[0].Children[2].Location.NestingLevel.Should().Be(1);             // Call
            topLevelNodes[0].Children[2].Children[0].Location.NestingLevel.Should().Be(2); // CallReturn
            topLevelNodes[0].Children[3].Location.NestingLevel.Should().Be(1);             // CallReturn
            topLevelNodes[1].Location.NestingLevel.Should().Be(0);                         // Call
            topLevelNodes[1].Children[0].Location.NestingLevel.Should().Be(1);             // CallReturn

            // Check parents
            topLevelNodes[0].Parent.Should().Be(null);
            topLevelNodes[0].Children[0].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[0].Children[0].Children[0].Parent.Location.Location.Message.Text.Should().Be("second parent");
            topLevelNodes[0].Children[1].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[0].Children[1].Children[0].Parent.Location.Location.Message.Text.Should().Be("third parent");
            topLevelNodes[0].Children[2].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[0].Children[2].Children[0].Parent.Location.Location.Message.Text.Should().Be("fourth parent");
            topLevelNodes[0].Children[3].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[1].Parent.Should().Be(null);
            topLevelNodes[1].Children[0].Parent.Location.Location.Message.Text.Should().Be("fifth parent");
        }

        [Fact]
        public void CanConvertCodeFlowToTreeNonCallOrReturn()
        {
            var codeFlow = SarifUtilities.CreateSingleThreadedCodeFlow(new[]
            {
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "first parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // CallReturn
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Call
                    Location = new Location
                    {
                        Message = new Message
                        {
                            Text = "second parent"
                        }
                    }
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1, // CallReturn
                },
            });

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            topLevelNodes.Count.Should().Be(2);
            topLevelNodes[0].Children.Count.Should().Be(4);
            topLevelNodes[1].Children.Count.Should().Be(3);

            // Spot-check that we have the right nodes at the right places in the tree.
            topLevelNodes[0].Location.NestingLevel.Should().Be(0);             // Call
            topLevelNodes[0].Children[0].Location.NestingLevel.Should().Be(1); // Declaration
            topLevelNodes[0].Children[3].Location.NestingLevel.Should().Be(1); // CallReturn
            topLevelNodes[1].Location.NestingLevel.Should().Be(0);             // Call
            topLevelNodes[1].Children[2].Location.NestingLevel.Should().Be(1); // CallReturn

            // Check parents
            topLevelNodes[0].Parent.Should().Be(null);
            topLevelNodes[0].Children[0].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[0].Children[1].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[0].Children[2].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[0].Children[3].Parent.Location.Location.Message.Text.Should().Be("first parent");
            topLevelNodes[1].Parent.Should().Be(null);
            topLevelNodes[1].Children[0].Parent.Location.Location.Message.Text.Should().Be("second parent");
            topLevelNodes[1].Children[1].Parent.Location.Location.Message.Text.Should().Be("second parent");
            topLevelNodes[1].Children[2].Parent.Location.Location.Message.Text.Should().Be("second parent");
        }

        [Fact]
        public void CanConvertCodeFlowToTreeOnlyDeclarations()
        {
            var codeFlow = SarifUtilities.CreateSingleThreadedCodeFlow(new[]
            {
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Declaration
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 0, // Declaration
                },
            });

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            topLevelNodes.Count.Should().Be(3);
            topLevelNodes[0].Children.Should().BeEmpty();
            topLevelNodes[1].Children.Should().BeEmpty();
            topLevelNodes[2].Children.Should().BeEmpty();

            topLevelNodes[1].Location.NestingLevel.Should().Be(0); // Declaration
            topLevelNodes[0].Location.NestingLevel.Should().Be(0); // Declaration
            topLevelNodes[2].Location.NestingLevel.Should().Be(0); // Declaration

            topLevelNodes[0].Parent.Should().Be(null);
            topLevelNodes[1].Parent.Should().Be(null);
            topLevelNodes[2].Parent.Should().Be(null);
        }
    }
}
