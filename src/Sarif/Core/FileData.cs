// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using SarifWriters = Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A single file. In some cases, this file might be nested within another file.
    /// </summary>
    public partial class FileData : ISarifNode, IEquatable<FileData>
    {
        public static IList<FileData> Create(IEnumerable<Uri> uris, out string fileDataKey)
        {
            if (uris == null) { throw new ArgumentNullException(nameof(uris)); }

            fileDataKey = null;

            List<FileData> files = new List<FileData>();

            foreach (Uri uri in uris)
            {
                var fileData = new FileData()
                {
                    MimeType = SarifWriters.MimeType.DetermineFromFileExtension(uri)
                };

                if (files.Count == 0)
                {
                    Debug.Assert(uri.IsAbsoluteUri);
                    fileDataKey = uri.ToString();
                }
                else if (files.Count == 1)
                {
                    fileData.PathFromParent = uri.ToString();
                    fileDataKey = fileDataKey + "#" + fileData.PathFromParent;
                }
                else
                {
                    Debug.Assert(!uri.IsAbsoluteUri);                    
                    fileData.PathFromParent = uri.ToString();
                    fileDataKey = fileDataKey + fileData.PathFromParent;
                }

                files.Add(fileData);
            }

            return files;
        }
    }
}
