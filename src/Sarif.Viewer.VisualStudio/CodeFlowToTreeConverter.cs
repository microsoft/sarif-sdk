// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer.VisualStudio
{
    internal static class CodeFlowToTreeConverter
    {
        internal static CallTreeNode Convert(CodeFlow codeFlow)
        {
           int currentCodeFlowIndex = -1;

            CallTreeNode root = new CallTreeNode
            {
                Children = GetChildren(codeFlow, ref currentCodeFlowIndex)
            };

            return root;
        }

        private static List<CallTreeNode> GetChildren(CodeFlow codeFlow, ref int currentCodeFlowIndex)
        {
            currentCodeFlowIndex++;
            List<CallTreeNode> children = new List<CallTreeNode>();
            bool foundCallReturn = false;

            while (currentCodeFlowIndex < codeFlow.Locations.Count && !foundCallReturn)
            {
                switch (codeFlow.Locations[currentCodeFlowIndex].Kind)
                {
                    case AnnotatedCodeLocationKind.Call:
                        children.Add(new CallTreeNode
                        {
                            Children = GetChildren(codeFlow, ref currentCodeFlowIndex)
                        });
                        break;

                    case AnnotatedCodeLocationKind.CallReturn:
                        children.Add(new CallTreeNode
                        {
                            Children = new List<CallTreeNode>()
                        });
                        foundCallReturn = true;
                        break;

                    default:
                        children.Add(new CallTreeNode
                        {
                            Children = new List<CallTreeNode>()
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
