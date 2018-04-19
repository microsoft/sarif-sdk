// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.VisualStudio
{
    internal static class CodeFlowToTreeConverter
    {
        internal static List<CallTreeNode> Convert(CodeFlow codeFlow)
        {
            var root = new CallTreeNode { Children = new List<CallTreeNode>() };
            ThreadFlow threadFlow = codeFlow.ThreadFlows?[0];

            if (threadFlow != null)
            {
                int lastNestingLevel = 0;
                CallTreeNode lastParent = root;

                foreach (CodeFlowLocation location in threadFlow.Locations)
                {
                    var newNode = new CallTreeNode
                    {
                        Location = location,
                        Children = new List<CallTreeNode>()
                    };

                    if (location.NestingLevel > lastNestingLevel)
                    {
                        // Previous node was a call
                        lastParent = lastParent.Children.Last();
                        lastParent.Kind = CallTreeNodeKind.Call;
                    }
                    else if (location.NestingLevel < lastNestingLevel)
                    {
                        // Previous node was a return
                        CallTreeNode node = lastParent.Children.Last(); // Get the last node we created
                        node.Kind = CallTreeNodeKind.Call;
                        lastParent = node.Parent.Parent;
                    }

                    newNode.Parent = lastParent;
                    lastParent.Children.Add(newNode);
                    lastNestingLevel = location.NestingLevel;
                }

                root.Children.ForEach(n => n.Parent = null);
            }

            return root.Children;
        }
    }
}
