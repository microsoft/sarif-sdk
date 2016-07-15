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

            Stack<AnnotatedCodeLocation> unReturnedCalls = new Stack<AnnotatedCodeLocation>();

            return GetChildren(codeFlow, ref currentCodeFlowIndex, ref unReturnedCalls);
        }

        private static List<CallTreeNode> GetChildren(CodeFlow codeFlow, ref int currentCodeFlowIndex, ref Stack<AnnotatedCodeLocation> unReturnedCalls)
        {
            currentCodeFlowIndex++;
            List<CallTreeNode> children = new List<CallTreeNode>();
            bool foundCallReturn = false;

            while (currentCodeFlowIndex < codeFlow.Locations.Count && !foundCallReturn)
            {
                switch (codeFlow.Locations[currentCodeFlowIndex].Kind)
                {
                    case AnnotatedCodeLocationKind.Call:
                        unReturnedCalls.Push(codeFlow.Locations[currentCodeFlowIndex]);
                        children.Add(new CallTreeNode
                        {
                            Location = codeFlow.Locations[currentCodeFlowIndex],
                            Children = GetChildren(codeFlow, ref currentCodeFlowIndex, ref unReturnedCalls)
                        });
                        break;

                    case AnnotatedCodeLocationKind.CallReturn:
                        unReturnedCalls.Pop();
                        children.Add(new CallTreeNode
                        {
                            Location = codeFlow.Locations[currentCodeFlowIndex],
                            Children = new List<CallTreeNode>()
                        });
                        foundCallReturn = true;
                        break;

                    default:
                        children.Add(new CallTreeNode
                        {
                            Location = codeFlow.Locations[currentCodeFlowIndex],
                            Children = new List<CallTreeNode>()
                        });
                        currentCodeFlowIndex++;
                        break;
                }
            }
            currentCodeFlowIndex++;

            if (currentCodeFlowIndex == codeFlow.Locations.Count && unReturnedCalls.Count > 0)
            {
                throw new System.ArgumentException("At least one AnnotatedCodeLocation Call in this CodeFlow is not returned, causing an imbalanced tree.");
            }

            return children;
        }
    }
}
