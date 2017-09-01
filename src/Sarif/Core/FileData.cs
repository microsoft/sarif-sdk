// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        internal static IFileSystem _fileSystem = new FileSystem();

        public static FileData Create(Uri uri, SarifWriters.LoggingOptions loggingOptions, string mimeType = null, Encoding encoding = null)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            mimeType = mimeType ?? SarifWriters.MimeType.DetermineFromFileExtension(uri);
            encoding = encoding ?? Encoding.UTF8;

            var fileData = new FileData()
            {
                MimeType = mimeType,
            };

            // Attempt to persist file contents and/or compute file hash and persist
            // this information to the log file. In the event that there is some issue
            // accessing the file, for example, due to ACLs applied to a directory,
            // we currently swallow these exceptions without populating any requested
            // data or putting a notification in the log file that a problem
            // occurred. Something to discuss moving forward.
            try
            {
                if (!uri.IsAbsoluteUri || !uri.IsFile || !_fileSystem.FileExists(uri.LocalPath))
                {
                    return fileData;
                }

                string filePath = uri.LocalPath;
                bool encodeAsUtf8 = (fileData.MimeType != SarifWriters.MimeType.Binary);

                if (loggingOptions.Includes(Writers.LoggingOptions.PersistFileContents))
                {
                    fileData.Contents = EncodeFileContents(filePath, mimeType, encoding);
                }

                if (loggingOptions.Includes(Writers.LoggingOptions.ComputeFileHashes))
                {
                    HashData hashes = HashUtilities.ComputeHashes(filePath);
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

            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { }

            return fileData;
        }

        private static string EncodeFileContents(string filePath, string mimeType, Encoding inputFileEncoding)
        {
            byte[] fileContents;

            if (mimeType != SarifWriters.MimeType.Binary)
            {
                fileContents = Encoding.UTF8.GetBytes(_fileSystem.ReadAllText(filePath, inputFileEncoding));
            }
            else
            {
                fileContents = _fileSystem.ReadAllBytes(filePath);
            }

            return Convert.ToBase64String(fileContents);
        }
    }
}
