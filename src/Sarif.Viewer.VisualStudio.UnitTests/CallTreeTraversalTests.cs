// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using Xunit;
using Moq;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class CallTreeTraversalTests
    {
        [Fact]
        public void SelectPreviousNextCommandsTest()
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
                        Kind = AnnotatedCodeLocationKind.CallReturn
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    },
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Declaration
                    }
                }
            };

            var mockToolWindow = new Mock<IToolWindow>();
            mockToolWindow.Setup(s => s.UpdateSelectionList(It.IsAny<object[]>()));

            CallTree callTree = new CallTree(CodeFlowToTreeConverter.Convert(codeFlow), mockToolWindow.Object);

            callTree.FindPrevious().Should().Be(null);
            callTree.FindNext().Should().Be(null);

            callTree.SelectedItem = callTree.TopLevelNodes[0];
            callTree.FindPrevious().Should().Be(callTree.TopLevelNodes[0]);
            callTree.FindNext().Should().Be(callTree.TopLevelNodes[0].Children[0]);

            callTree.SelectedItem = callTree.TopLevelNodes[0].Children[0];
            callTree.FindPrevious().Should().Be(callTree.TopLevelNodes[0]);
            callTree.FindNext().Should().Be(callTree.TopLevelNodes[0].Children[1]);

            callTree.SelectedItem = callTree.TopLevelNodes[0].Children[2];
            callTree.FindPrevious().Should().Be(callTree.TopLevelNodes[0].Children[1]);
            callTree.FindNext().Should().Be(callTree.TopLevelNodes[1]);

            callTree.SelectedItem = callTree.TopLevelNodes[1];
            callTree.FindPrevious().Should().Be(callTree.TopLevelNodes[0].Children[2]);
            callTree.FindNext().Should().Be(callTree.TopLevelNodes[2]);

            callTree.SelectedItem = callTree.TopLevelNodes[2];
            callTree.FindPrevious().Should().Be(callTree.TopLevelNodes[1]);
            callTree.FindNext().Should().Be(callTree.TopLevelNodes[2]);
        }

        [Fact]
        public void SelectPreviousNextCommandsCallNoChildrenTest()
        {
            var codeFlow = new CodeFlow
            {
                Locations = new List<AnnotatedCodeLocation>
                {
                    new AnnotatedCodeLocation
                    {
                        Kind = AnnotatedCodeLocationKind.Call
                    }
                }
            };

            var mockToolWindow = new Mock<IToolWindow>();
            mockToolWindow.Setup(s => s.UpdateSelectionList(It.IsAny<object[]>()));

            CallTree callTree = new CallTree(CodeFlowToTreeConverter.Convert(codeFlow), mockToolWindow.Object);

            callTree.SelectedItem = callTree.TopLevelNodes[0];
            callTree.FindPrevious().Should().Be(callTree.TopLevelNodes[0]);
            callTree.FindNext().Should().Be(callTree.TopLevelNodes[0]);
        }
    }
}
