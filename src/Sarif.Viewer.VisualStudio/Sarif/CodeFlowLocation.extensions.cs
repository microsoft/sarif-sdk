// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class CodeFlowLocationExtensions
    {
        public static CodeFlowLocationModel ToCodeFlowLocationModel(this CodeFlowLocation codeFlowLocation)
        {
            CodeFlowLocationModel model = new CodeFlowLocationModel();

            if (codeFlowLocation.Location != null)
            {
                if (codeFlowLocation.Location.PhysicalLocation != null)
                {
                    PhysicalLocation physicalLocation = codeFlowLocation.Location.PhysicalLocation;

                    model.Id = physicalLocation.Id;
                    model.Region = physicalLocation.Region;

                    Uri uri = physicalLocation.FileLocation?.Uri;
                    if (uri != null)
                    {
                        model.FilePath = uri.ToPath();
                        model.UriBaseId = physicalLocation.FileLocation.UriBaseId;
                    }
                }

                model.Message = codeFlowLocation.Location.Message.Text;
                model.LogicalLocation = codeFlowLocation.Location.FullyQualifiedLogicalName;
            }
            
            model.IsEssential = codeFlowLocation.Importance == CodeFlowLocationImportance.Essential;

            return model;
        }
    }
}
