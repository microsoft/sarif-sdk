// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
