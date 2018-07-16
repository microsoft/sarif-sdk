// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class LocationExtensions
    {
        public static ThreadFlowLocationModel ToThreadFlowLocationModel(this Location location)
        {
            var model = new ThreadFlowLocationModel();
            PhysicalLocation physicalLocation = location.PhysicalLocation;

            if (physicalLocation?.FileLocation != null)
            {
                model.Id = physicalLocation.Id;
                model.Region = physicalLocation.Region;

                Uri uri = physicalLocation.FileLocation.Uri;

                if (uri != null)
                {
                    model.FilePath = uri.ToPath();
                    model.UriBaseId = physicalLocation.FileLocation.UriBaseId;
                }
            }

            model.Message = location.Message?.Text;
            model.LogicalLocation = location.FullyQualifiedLogicalName;

            return model;
        }
    }
}
