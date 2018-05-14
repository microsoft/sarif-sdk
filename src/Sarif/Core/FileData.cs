﻿// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SarifWriters = Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Represents a single file. In some cases, this file might be nested within another file.
    /// </summary>
    public partial class FileData : ISarifNode
    {
        public static FileData Create(
            Uri uri, 
            SarifWriters.LoggingOptions loggingOptions, 
            string mimeType = null, 
            Encoding encoding = null,
            IFileSystem fileSystem = null)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            mimeType = mimeType ?? SarifWriters.MimeType.DetermineFromFileExtension(uri);
            fileSystem = fileSystem ?? new FileSystem();

            var fileData = new FileData()
            {
                MimeType = mimeType
            };

            // Attempt to persist file contents and/or compute file hash and persist
            // this information to the log file. In the event that there is some issue
            // accessing the file, for example, due to ACLs applied to a directory,
            // we currently swallow these exceptions without populating any requested
            // data or putting a notification in the log file that a problem
            // occurred. Something to discuss moving forward.
            try
            {
                if (!uri.IsAbsoluteUri || !uri.IsFile || !fileSystem.FileExists(uri.LocalPath))
                {
                    return fileData;
                }

                string filePath = uri.LocalPath;

                if (loggingOptions.Includes(Writers.LoggingOptions.PersistFileContents))
                {
                    fileData.Contents = GetEncodedFileContents(fileSystem, filePath, mimeType, encoding);
                }

                if (loggingOptions.Includes(Writers.LoggingOptions.ComputeFileHashes))
                {
                    HashData hashes = HashUtilities.ComputeHashes(filePath);
                    fileData.Hashes = new List<Hash>
                    {
                        new Hash()
                        {
                            Value = hashes.MD5,
                            Algorithm = "md5",
                        },
                        new Hash()
                        {
                            Value = hashes.Sha1,
                            Algorithm = "sha-1",
                        },
                        new Hash()
                        {
                            Value = hashes.Sha256,
                            Algorithm = "sha-256",
                        },
                    };
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { }

            return fileData;
        }

        private static FileContent GetEncodedFileContents(IFileSystem fileSystem, string filePath, string mimeType, Encoding inputFileEncoding)
        {
            var fileContent = new FileContent();
            byte[] fileContents = fileSystem.ReadAllBytes(filePath);

            if (mimeType == SarifWriters.MimeType.Binary || inputFileEncoding == null)
            {
                fileContent.Binary = Convert.ToBase64String(fileContents);
            }
            else
            {
                fileContent.Text = inputFileEncoding.GetString(fileContents);
            }

            return fileContent;
        }
    }
}
