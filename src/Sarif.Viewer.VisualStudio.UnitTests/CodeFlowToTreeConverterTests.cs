// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System.Collections.Generic;
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
                        Kind = AnnotatedCodeLocationKind.Call
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call
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
                        Kind = AnnotatedCodeLocationKind.Call
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
        }

        public void CanConvertCodeFlowToTreeNonCallOrReturn()
        {
            var codeFlow = new CodeFlow
            {
                Locations = new List<AnnotatedCodeLocation>
                {
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call
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
                        Kind = AnnotatedCodeLocationKind.Call
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
        }

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
        }
    }
}
