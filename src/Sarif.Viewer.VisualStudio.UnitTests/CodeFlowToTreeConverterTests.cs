// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class CodeFlowToTreeConverterTests
    {
        [Fact]
        public void CanConvertCodeFlowToTree()
        {
            var codeFlow = new CodeFlow
            {
                Locations = new List<AnnotatedCodeLocation>
                {
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "first parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "second parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "third parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "fourth parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "fifth parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn,
                    },
                }
            };

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            topLevelNodes.Count.Should().Be(2);
            topLevelNodes[0].Children.Count.Should().Be(4);
            topLevelNodes[0].Children[2].Children.Count.Should().Be(1);

            // Check that we have the right nodes at the right places in the tree.
            topLevelNodes[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[0].Children[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[0].Children[0].Children[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);
            topLevelNodes[0].Children[1].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[0].Children[1].Children[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);
            topLevelNodes[0].Children[2].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[0].Children[2].Children[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);
            topLevelNodes[0].Children[3].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);
            topLevelNodes[1].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[1].Children[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);

            // Check parents
            topLevelNodes[0].Parent.Should().Be(null);
            topLevelNodes[0].Children[0].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[0].Children[0].Children[0].Parent.Location.Message.Should().Be("second parent");
            topLevelNodes[0].Children[1].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[0].Children[1].Children[0].Parent.Location.Message.Should().Be("third parent");
            topLevelNodes[0].Children[2].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[0].Children[2].Children[0].Parent.Location.Message.Should().Be("fourth parent");
            topLevelNodes[0].Children[3].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[1].Parent.Should().Be(null);
            topLevelNodes[1].Children[0].Parent.Location.Message.Should().Be("fifth parent");
        }

        [Fact]
        public void CanConvertCodeFlowToTreeNonCallOrReturn()
        {
            var codeFlow = new CodeFlow
            {
                Locations = new List<AnnotatedCodeLocation>
                {
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "first parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call,
                        Message = "second parent"
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                }
            };

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            topLevelNodes.Count.Should().Be(2);
            topLevelNodes[0].Children.Count.Should().Be(4);
            topLevelNodes[1].Children.Count.Should().Be(3);

            // Spot-check that we have the right nodes at the right places in the tree.
            topLevelNodes[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[0].Children[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Declaration);
            topLevelNodes[0].Children[3].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);
            topLevelNodes[1].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Call);
            topLevelNodes[1].Children[2].Location.Kind.Should().Be(AnnotatedCodeLocationKind.CallReturn);

            // Check parents
            topLevelNodes[0].Parent.Should().Be(null);
            topLevelNodes[0].Children[0].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[0].Children[1].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[0].Children[2].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[0].Children[3].Parent.Location.Message.Should().Be("first parent");
            topLevelNodes[1].Parent.Should().Be(null);
            topLevelNodes[1].Children[0].Parent.Location.Message.Should().Be("second parent");
            topLevelNodes[1].Children[1].Parent.Location.Message.Should().Be("second parent");
            topLevelNodes[1].Children[2].Parent.Location.Message.Should().Be("second parent");
        }

        [Fact]
        public void CanConvertCodeFlowToTreeOnlyDeclarations()
        {
            var codeFlow = new CodeFlow
            {
                Locations = new List<AnnotatedCodeLocation>
                {
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                }
            };

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            topLevelNodes.Count.Should().Be(3);
            topLevelNodes[0].Children.Should().BeEmpty();
            topLevelNodes[1].Children.Should().BeEmpty();
            topLevelNodes[2].Children.Should().BeEmpty();

            topLevelNodes[0].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Declaration);
            topLevelNodes[1].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Declaration);
            topLevelNodes[2].Location.Kind.Should().Be(AnnotatedCodeLocationKind.Declaration);

            topLevelNodes[0].Parent.Should().Be(null);
            topLevelNodes[1].Parent.Should().Be(null);
            topLevelNodes[2].Parent.Should().Be(null);
        }
    }
}
