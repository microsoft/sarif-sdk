// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer.VisualStudio
{
    internal static class CodeFlowToTreeConverter
    {
        internal static List<CallTreeNode> Convert(CodeFlow codeFlow)
        {
            int currentCodeFlowIndex = -1;

            return GetChildren(codeFlow, ref currentCodeFlowIndex, null);
        }

        private static List<CallTreeNode> GetChildren(CodeFlow codeFlow, ref int currentCodeFlowIndex, CallTreeNode parent)
        {
            currentCodeFlowIndex++;
            List<CallTreeNode> children = new List<CallTreeNode>();
            bool foundCallReturn = false;

            while (currentCodeFlowIndex < codeFlow.Locations.Count && !foundCallReturn)
            {
                switch (codeFlow.Locations[currentCodeFlowIndex].Kind)
                {
                    case AnnotatedCodeLocationKind.Call:
                        var newNode = new CallTreeNode
                        {
                            Location = codeFlow.Locations[currentCodeFlowIndex],
                            Parent = parent
                        };
                        newNode.Children = GetChildren(codeFlow, ref currentCodeFlowIndex, newNode);
                        children.Add(newNode);
                        break;

                    case AnnotatedCodeLocationKind.CallReturn:
                        children.Add(new CallTreeNode
                        {
                            Location = codeFlow.Locations[currentCodeFlowIndex],
                            Children = new List<CallTreeNode>(),
                            Parent = parent
                        });
                        foundCallReturn = true;
                        break;

                    default:
                        children.Add(new CallTreeNode
                        {
                            Location = codeFlow.Locations[currentCodeFlowIndex],
                            Children = new List<CallTreeNode>(),
                            Parent = parent
                        });
                        currentCodeFlowIndex++;
                        break;
                }
            }
            currentCodeFlowIndex++;
            return children;
        }
    }
}
