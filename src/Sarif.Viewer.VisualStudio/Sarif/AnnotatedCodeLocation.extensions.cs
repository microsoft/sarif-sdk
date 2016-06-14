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
    static class AnnotatedCodeLocationExtensions
    {
        public static Microsoft.Sarif.Viewer.Models.AnnotatedCodeLocationModel ToAnnotatedCodeLocationModel(this AnnotatedCodeLocation location)
        {
            AnnotatedCodeLocationModel model = new AnnotatedCodeLocationModel();

            if (location.PhysicalLocation != null)
            {
                model.Region = location.PhysicalLocation.Region;

                Uri uri = location.PhysicalLocation.Uri;

                if (uri != null)
                {
                    string path = uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString();
                    if (uri.IsAbsoluteUri && !Path.IsPathRooted(path))
                    {
                        path = uri.AbsoluteUri;
                    }

                    model.FilePath = path;
                }
            }

            model.Message = location.Message;
            model.Kind = location.Kind.ToString();
            model.LogicalLocation = location.FullyQualifiedLogicalName;
            model.IsEssential = location.Essential;

            return model;
        }
    }
}
