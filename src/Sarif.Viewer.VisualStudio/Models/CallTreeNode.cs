// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;
using System;
using System.IO;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTreeNode : CodeLocationObject
    {
        private AnnotatedCodeLocation _location;

        public AnnotatedCodeLocation Location
        {
            get
            {
                return this._location;
            }
            set
            {
                this._location = value;

                if (value != null && value.PhysicalLocation != null)
                {
                    Uri uri = value.PhysicalLocation.Uri;
                    if (uri != null)
                    {
                        string path = uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString();
                        if (uri.IsAbsoluteUri && !Path.IsPathRooted(path))
                        {
                            path = uri.AbsoluteUri;
                        }

                        this.FilePath = path;
                    }

                    this.Region = value.PhysicalLocation.Region;
                }
                else
                {
                    this.FilePath = null;
                    this.Region = null;
                }
            }
        }

        public List<CallTreeNode> Children { get; set; }
    }
}
