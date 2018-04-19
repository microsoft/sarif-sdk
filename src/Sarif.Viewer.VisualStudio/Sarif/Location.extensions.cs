// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class LocationExtensions
    {
        public static CodeFlowLocationModel ToCodeFlowLocationModel(this Location location)
        {
            var model = new CodeFlowLocationModel();
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
