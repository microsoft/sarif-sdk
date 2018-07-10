// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Windows;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using Moq;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.Converters.UnitTests
{
    public class CallTreeTests
    {
        const string Expected = "expected";

        [Fact]
        public void CallTree_TryGetIndexInCallTreeNodeList_NullList()
        {
            List<CallTreeNode> list = null;

            CallTreeNode node = new CallTreeNode();

            int index;
            bool result = CallTree.TryGetIndexInCallTreeNodeList(list, node, out index);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetIndexInCallTreeNodeList_NullNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());

            int index;
            bool result = CallTree.TryGetIndexInCallTreeNodeList(list, null, out index);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetIndexInCallTreeNodeList_FirstNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(target);
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());

            int index;
            bool result = CallTree.TryGetIndexInCallTreeNodeList(list, target, out index);

            result.Should().BeTrue();
            index.Should().Be(0);
        }

        [Fact]
        public void CallTree_TryGetIndexInCallTreeNodeList_LastNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());
            list.Add(target);

            int index;
            bool result = CallTree.TryGetIndexInCallTreeNodeList(list, target, out index);

            result.Should().BeTrue();
            index.Should().Be(2);
        }

        [Fact]
        public void CallTree_TryGetIndexInCallTreeNodeList_MiddleNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(target);
            list.Add(new CallTreeNode());

            int index;
            bool result = CallTree.TryGetIndexInCallTreeNodeList(list, target, out index);

            result.Should().BeTrue();
            index.Should().Be(1);
        }

        [Fact]
        public void CallTree_TryGetIndexInCallTreeNodeList_DoesNotExistNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());

            int index;
            bool result = CallTree.TryGetIndexInCallTreeNodeList(list, new CallTreeNode(), out index);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetNextSibling_NullList()
        {
            List<CallTreeNode> list = null;

            CallTreeNode node = new CallTreeNode();

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, node, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetNextSibling_NullNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, null, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetNextSibling_FirstNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(target);
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, target, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetNextSibling_LastNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());
            list.Add(target);

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, target, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetNextSibling_MiddleNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(target);
            list.Add(new CallTreeNode() { FilePath = Expected });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, target, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetNextSibling_SkipNonVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(target);
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });
            list.Add(new CallTreeNode() { FilePath = Expected });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, target, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetNextSibling_NoVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(target);
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, target, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetNextSibling_DoesNotExistNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetNextSibling(list, new CallTreeNode(), out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_NullList()
        {
            List<CallTreeNode> list = null;

            CallTreeNode node = new CallTreeNode();

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, node, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_NullNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, null, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_FirstNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(target);
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, target, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_LastNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(target);

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, target, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_MiddleNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(target);
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, target, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_SkipNonVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });
            list.Add(target);
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, target, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_NoVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });
            list.Add(target);
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, target, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetPreviousSibling_DoesNotExistNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetPreviousSibling(list, new CallTreeNode(), out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetFirstItem_NullList()
        {
            List<CallTreeNode> list = null;

            CallTreeNode resultNode;
            bool result = CallTree.TryGetFirstItem(list, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetFirstItem_FirstNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetFirstItem(list, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetFirstItem_SkipNonVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(new CallTreeNode());

            CallTreeNode resultNode;
            bool result = CallTree.TryGetFirstItem(list, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetFirstItem_NoVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetFirstItem(list, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetLastItem_NullList()
        {
            List<CallTreeNode> list = null;

            CallTreeNode resultNode;
            bool result = CallTree.TryGetLastItem(list, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_TryGetLastItem_LastNode()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode() { FilePath = Expected });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetLastItem(list, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetLastItem_SkipNonVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            list.Add(new CallTreeNode());
            list.Add(new CallTreeNode() { FilePath = Expected });
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetLastItem(list, out resultNode);

            result.Should().BeTrue();
            resultNode.FilePath.Should().Be(Expected);
        }

        [Fact]
        public void CallTree_TryGetLastItem_NoVisibleNodes()
        {
            List<CallTreeNode> list = new List<CallTreeNode>();
            CallTreeNode target = new CallTreeNode();
            list.Add(new CallTreeNode() { Visibility = Visibility.Collapsed });
            list.Add(new CallTreeNode() { Visibility = Visibility.Hidden });

            CallTreeNode resultNode;
            bool result = CallTree.TryGetLastItem(list, out resultNode);

            result.Should().BeFalse();
        }

        [Fact]
        public void CallTree_ExpandAll_NoNodes()
        {
            var mockToolWindow = new Mock<IToolWindow>();
            mockToolWindow.Setup(s => s.UpdateSelectionList(It.IsAny<object[]>()));

            CallTree tree = new CallTree(new List<CallTreeNode>(), mockToolWindow.Object);
            tree.ExpandAll();
        }

        [Fact]
        public void CallTree_ExpandAll()
        {
            CallTree tree = CreateCallTree();
            tree.ExpandAll();

            tree.TopLevelNodes[0].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[0].Children[0].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[0].Children[1].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[0].Children[2].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[1].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[2].IsExpanded.Should().BeTrue();
        }

        [Fact]
        public void CallTree_CollapseAll()
        {
            CallTree tree = CreateCallTree();
            tree.CollapseAll();

            tree.TopLevelNodes[0].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[0].Children[0].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[0].Children[1].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[0].Children[2].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[1].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[2].IsExpanded.Should().BeFalse();
        }

        [Fact]
        public void CallTree_IntelligentExpand()
        {
            CallTree tree = CreateCallTree();
            tree.IntelligentExpand();

            tree.TopLevelNodes[0].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[0].Children[0].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[0].Children[1].IsExpanded.Should().BeTrue();
            tree.TopLevelNodes[0].Children[2].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[1].IsExpanded.Should().BeFalse();
            tree.TopLevelNodes[2].IsExpanded.Should().BeTrue();
        }

        [Fact]
        public void CallTree_SetVerbosity_Essential()
        {
            CallTree tree = CreateCallTree();
            tree.SetVerbosity(ThreadFlowLocationImportance.Essential);

            tree.TopLevelNodes[0].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[0].Visibility.Should().Be(Visibility.Collapsed);
            tree.TopLevelNodes[0].Children[1].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[2].Visibility.Should().Be(Visibility.Collapsed);
            tree.TopLevelNodes[1].Visibility.Should().Be(Visibility.Collapsed);
            tree.TopLevelNodes[2].Visibility.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void CallTree_SetVerbosity_Important()
        {
            CallTree tree = CreateCallTree();
            tree.SetVerbosity(ThreadFlowLocationImportance.Important);

            tree.TopLevelNodes[0].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[0].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[1].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[2].Visibility.Should().Be(Visibility.Collapsed);
            tree.TopLevelNodes[1].Visibility.Should().Be(Visibility.Collapsed);
            tree.TopLevelNodes[2].Visibility.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void CallTree_SetVerbosity_Unimportant()
        {
            CallTree tree = CreateCallTree();
            tree.SetVerbosity(ThreadFlowLocationImportance.Unimportant);

            tree.TopLevelNodes[0].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[0].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[1].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[0].Children[2].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[1].Visibility.Should().Be(Visibility.Visible);
            tree.TopLevelNodes[2].Visibility.Should().Be(Visibility.Visible);
        }

        private CallTree CreateCallTree()
        {
            var codeFlow = SarifUtilities.CreateSingleThreadedCodeFlow(new[]
            {
                new ThreadFlowLocation
                {
                    NestingLevel = 0,
                    Importance = ThreadFlowLocationImportance.Unimportant,
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1,
                    Importance = ThreadFlowLocationImportance.Important,
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1,
                    Importance = ThreadFlowLocationImportance.Essential,
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 1,
                    Importance = ThreadFlowLocationImportance.Unimportant,
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 0,
                    Importance = ThreadFlowLocationImportance.Unimportant,
                },
                new ThreadFlowLocation
                {
                    NestingLevel = 0,
                    Importance = ThreadFlowLocationImportance.Essential,
                }
            });

            var mockToolWindow = new Mock<IToolWindow>();
            mockToolWindow.Setup(s => s.UpdateSelectionList(It.IsAny<object[]>()));

            CallTree callTree = new CallTree(CodeFlowToTreeConverter.Convert(codeFlow), mockToolWindow.Object);

            return callTree;
        }
    }
}
