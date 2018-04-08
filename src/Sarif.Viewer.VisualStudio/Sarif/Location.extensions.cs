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
        public static Microsoft.Sarif.Viewer.Models.CodeFlowLocationModel ToAnnotatedCodeLocationModel(this Location location)
        {
            CodeFlowLocationModel model = new CodeFlowLocationModel();
            PhysicalLocation physicalLocation = location.PhysicalLocation;

            if (physicalLocation?.FileLocation != null)
            {
                model.Region = physicalLocation.Region;

                Uri uri = physicalLocation.FileLocation.Uri;

                if (uri != null)
                {
                    model.FilePath = uri.ToPath();
                    model.UriBaseId = physicalLocation.FileLocation.UriBaseId;
                }
            }

            model.LogicalLocation = location.FullyQualifiedLogicalName;

            return model;
        }
    }
}
