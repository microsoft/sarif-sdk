// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

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
                Uri uri = SarifUtilities.CreateUri(fileChange.FileLocation.Uri);

                if (uri != null)
                {
                    model.FilePath = uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString;

                    foreach (Replacement replacement in fileChange.Replacements)
                    {
                        model.Replacements.Add(replacement.ToReplacementModel());
                    }
                }
            }

            return model;
        }
    }
}
