// Copyright (c) Microsoft.  All Rights Reserved.
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
        public static IList<FileData> Create(IEnumerable<Uri> uris, bool computeHashes, out string fileDataKey)
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

                    if (computeHashes && uri.IsAbsoluteUri && uri.IsFile)
                    {
                        string md5, sha1, sha256;

                        HashUtilities.ComputeHashes(uri.LocalPath, out md5, out sha1, out sha256);
                        fileData.Hashes = new List<Hash>
                        {
                            new Hash()
                            {
                                Value = md5,
                                Algorithm = AlgorithmKind.MD5,
                            },
                            new Hash()
                            {
                                Value = sha1,
                                Algorithm = AlgorithmKind.Sha1,
                            },
                            new Hash()
                            {
                                Value = sha256,
                                Algorithm = AlgorithmKind.Sha256,
                            },
                        };
                    }
                }
                else if (files.Count == 1)
                {
                    fileData.Uri = uri;
                    fileDataKey = fileDataKey + "#" + fileData.Uri.ToString();
                }
                else
                {
                    Debug.Assert(!uri.IsAbsoluteUri);                    
                    fileData.Uri = uri;
                    fileDataKey = fileDataKey + fileData.Uri.ToString();
                }

                files.Add(fileData);
            }

            return files;
        }
    }
}
