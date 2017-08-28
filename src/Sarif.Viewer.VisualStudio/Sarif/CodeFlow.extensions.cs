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
        public static AnnotatedCodeLocationCollection ToAnnotatedCodeLocationCollection(this CodeFlow codeFlow)
        {
            if (codeFlow == null)
            {
                return null;
            }

            AnnotatedCodeLocationCollection model = new AnnotatedCodeLocationCollection(codeFlow.Message);

            if (codeFlow.Locations != null)
            {
                foreach (AnnotatedCodeLocation location in codeFlow.Locations)
                {
                    // TODO we are not yet properly hardened against locationless
                    // code locations (and what this means is also in flux as
                    // far as SARIF producers). For now we skip these.
                    if (location.PhysicalLocation == null) { continue; }

                    model.Add(location.ToAnnotatedCodeLocationModel());
                }
            }

            return model;
        }

        public static CallTree ToCallTree(this CodeFlow codeFlow)
        {
            if (codeFlow.Locations?.Count == 0)
            {
                return null;
            }

            List<CallTreeNode> topLevelNodes = CodeFlowToTreeConverter.Convert(codeFlow);

            return new CallTree(topLevelNodes, SarifViewerPackage.SarifToolWindow);
        }
    }
}
