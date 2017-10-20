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
    static class FileChangeExtensions
    {
        public static FileChangeModel ToFileChangeModel(this FileChange fileChange)
        {
            if (fileChange == null)
            {
                return null;
            }

            FileChangeModel model = new FileChangeModel();

            if (fileChange.Replacements != null)
            {
                model.FilePath = fileChange.Uri.IsAbsoluteUri
                    ? fileChange.Uri.LocalPath
                    : fileChange.Uri.OriginalString;

                if (!Path.IsPathRooted(model.FilePath) && fileChange.Uri.IsAbsoluteUri)
                {
                    model.FilePath = fileChange.Uri.AbsoluteUri;
                }

                foreach (Replacement replacement in fileChange.Replacements)
                {
                    model.Replacements.Add(replacement.ToReplacementModel());
                }
            }

            return model;
        }
    }
}
