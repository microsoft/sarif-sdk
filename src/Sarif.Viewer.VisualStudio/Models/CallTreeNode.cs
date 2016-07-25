// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTreeNode : CodeLocationObject
    {
        private AnnotatedCodeLocation _location;

        public AnnotatedCodeLocation Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;

                if (value?.PhysicalLocation != null)
                {
                    // If the backing AnnotatedCodeLocation has a PhysicalLocation, set the 
                    // FilePath and Region properties. The FilePath and Region properties
                    // are used to navigate to the source location and highlight the line.
                    Uri uri = value.PhysicalLocation.Uri;
                    if (uri != null)
                    {
                        string path = uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString();
                        if (uri.IsAbsoluteUri && !Path.IsPathRooted(path))
                        {
                            path = uri.AbsoluteUri;
                        }

                        FilePath = path;
                    }

                    Region = value.PhysicalLocation.Region;
                }
                else
                {
                    FilePath = null;
                    Region = null;
                }
            }
        }

        public List<CallTreeNode> Children { get; set; }
    }
}
