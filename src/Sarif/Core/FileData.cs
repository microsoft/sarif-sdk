﻿// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SarifWriters = Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A single file. In some cases, this file might be nested within another file.
    /// </summary>
    public partial class FileData : ISarifNode
    {
        public static FileData Create(Uri uri, bool computeHashes, out string fileDataKey)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            fileDataKey = null;

            var fileData = new FileData()
            {
                MimeType = SarifWriters.MimeType.DetermineFromFileExtension(uri)
            };

            fileDataKey = uri.ToString();

            if (computeHashes && uri.IsAbsoluteUri && uri.IsFile)
            {
                HashData hashes = HashUtilities.ComputeHashes(uri.LocalPath);
                fileData.Hashes = new List<Hash>
                        {
                            new Hash()
                            {
                                Value = hashes.MD5,
                                Algorithm = AlgorithmKind.MD5,
                            },
                            new Hash()
                            {
                                Value = hashes.Sha1,
                                Algorithm = AlgorithmKind.Sha1,
                            },
                            new Hash()
                            {
                                Value = hashes.Sha256,
                                Algorithm = AlgorithmKind.Sha256,
                            },
                        };
            }
            //else if (files.Count == 1)
            //{
            //    fileData.Uri = uri;
            //    fileDataKey = fileDataKey + "#" + fileData.Uri.ToString();
            //}
            //else
            //{
            //    Debug.Assert(!uri.IsAbsoluteUri);                    
            //    fileData.Uri = uri;
            //    fileDataKey = fileDataKey + fileData.Uri.ToString();
            //}

            //files.Add(fileData);

            return fileData;
        }
    }
}
