// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using Microsoft.Sarif.Viewer.VisualStudio;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class CodeFlowExtensions
    {
        public static ThreadFlowLocationCollection ToThreadFlowLocationCollection (this CodeFlow codeFlow)
        {
            if (codeFlow == null)
            {
                return null;
            }

            var model = new ThreadFlowLocationCollection(codeFlow.Message.Text);

            if (codeFlow.ThreadFlows?[0]?.Locations != null)
            {
                foreach (ThreadFlowLocation location in codeFlow.ThreadFlows[0].Locations)
                {
                    // TODO we are not yet properly hardened against locationless
                    // code locations (and what this means is also in flux as
                    // far as SARIF producers). For now we skip these.
                    if (location.Location?.PhysicalLocation == null) { continue; }

                    model.Add(location.ToThreadFlowLocationModel());
                }
            }

            return model;
        }

        public static CallTree ToCallTree(this CodeFlow codeFlow)
        {
            if (codeFlow.ThreadFlows?[0]?.Locations?.Count == 0)
            {
                return null;
            }

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            return new CallTree(topLevelNodes, SarifViewerPackage.SarifToolWindow);
        }
    }
}
