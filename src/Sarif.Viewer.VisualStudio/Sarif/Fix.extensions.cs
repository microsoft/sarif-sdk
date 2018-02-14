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
    static class FixExtensions
    {
        public static FixModel ToFixModel(this Fix fix)
        {
            if (fix == null)
            {
                return null;
            }

            FixModel model = new FixModel(fix.Description, new FileSystem());

            if (fix.FileChanges != null)
            {
                foreach (FileChange fileChange in fix.FileChanges)
                {
                    model.FileChanges.Add(fileChange.ToFileChangeModel());
                }
            }

            return model;
        }
    }
}
