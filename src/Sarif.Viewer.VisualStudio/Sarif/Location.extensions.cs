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
    static class LocationExtensions
    {
        public static Microsoft.Sarif.Viewer.Models.AnnotatedCodeLocationModel ToAnnotatedCodeLocationModel(this Location location)
        {
            AnnotatedCodeLocationModel model = new AnnotatedCodeLocationModel();
            PhysicalLocation physicalLocation = null;

            if (location.ResultFile != null)
            {
                physicalLocation = location.ResultFile;
            }
            else if (location.AnalysisTarget != null)
            {
                physicalLocation = location.AnalysisTarget;
            }

            if (physicalLocation != null)
            {
                model.Region = physicalLocation.Region;

                Uri uri = physicalLocation.Uri;

                if (uri != null)
                {
                    model.FilePath = uri.ToPath();
                    model.UriBaseId = physicalLocation.UriBaseId;
                }
            }

            model.LogicalLocation = location.FullyQualifiedLogicalName;

            return model;
        }
    }
}
