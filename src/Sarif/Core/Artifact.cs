// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SarifWriters = Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A single artifact. In some cases, this artifact might be nested within another artifact.
    /// </summary>
    public partial class Artifact : ISarifNode
    {
        public static Artifact Create(
            Uri uri,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Encoding encoding = null,
            HashData hashData = null,
            IFileSystem fileSystem = null,
            HashAlgorithms hashAlgorithms = HashAlgorithms.Default)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            fileSystem ??= FileSystem.Instance;

            var artifact = new Artifact()
            {
                Encoding = encoding?.WebName,
                Hashes = NullIfEmpty(hashData?.ToDictionary()),
            };

            string mimeType = SarifWriters.MimeType.DetermineFromFileExtension(uri);

            // Attempt to persist file contents and/or compute file hash and persist
            // this information to the log file. In the event that there is some issue
            // accessing the file, for example, due to ACLs applied to a directory,
            // we currently swallow these exceptions without populating any requested
            // data or putting a notification in the log file that a problem
            // occurred. Something to discuss moving forward.
            try
            {
                bool workTodo = dataToInsert.HasFlag(OptionallyEmittedData.Hashes) ||
                                dataToInsert.HasFlag(OptionallyEmittedData.TextFiles) ||
                                dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles);

                if (!workTodo ||
                    !uri.IsAbsoluteUri ||
                    !uri.IsFile ||
                    !fileSystem.FileExists(uri.LocalPath))
                {
                    return artifact;
                }

                string filePath = uri.LocalPath;

                if (dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles) &&
                    SarifWriters.MimeType.IsBinaryMimeType(mimeType))
                {
                    artifact.Contents = GetEncodedFileContents(fileSystem, filePath, mimeType, encoding);
                }

                if (dataToInsert.HasFlag(OptionallyEmittedData.TextFiles) &&
                    SarifWriters.MimeType.IsTextualMimeType(mimeType))
                {
                    artifact.Contents = GetEncodedFileContents(fileSystem, filePath, mimeType, encoding);
                }

                if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes))
                {
                    HashData hashes = hashData
                        ?? HashUtilities.ComputeHashes(filePath, fileSystem, hashAlgorithms);

                    // The hash utilities will return null data in some test contexts.
                    if (hashes != null)
                    {
                        IDictionary<string, string> hashDictionary = hashes.ToDictionary();

                        // Only attach a Hashes dictionary if at least one algorithm produced
                        // a value; otherwise we would emit an empty `"hashes": {}` object.
                        if (hashDictionary.Count > 0)
                        {
                            artifact.Hashes = hashDictionary;
                        }
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { }

            return artifact;
        }

        private static ArtifactContent GetEncodedFileContents(IFileSystem fileSystem, string filePath, string mimeType, Encoding inputFileEncoding)
        {
            var fileContent = new ArtifactContent();
            byte[] fileContents = fileSystem.FileReadAllBytes(filePath);

            if (SarifWriters.MimeType.IsBinaryMimeType(mimeType))
            {
                fileContent.Binary = Convert.ToBase64String(fileContents);
            }
            else
            {
                inputFileEncoding ??= new UTF8Encoding();
                fileContent.Text = inputFileEncoding.GetString(fileContents);
            }

            return fileContent;
        }

#if DEBUG
        public override string ToString()
        {
            return this.Location?.ToString() ?? base.ToString();
        }
#endif

        // Avoid serializing an empty "hashes": {} object when no algorithm produced a value
        // (e.g., the caller selected HashAlgorithms.None or supplied a HashData with all-null
        // properties). SARIF readers should not see an empty hashes object on an artifact.
        private static IDictionary<string, string> NullIfEmpty(IDictionary<string, string> hashes)
        {
            return (hashes == null || hashes.Count == 0) ? null : hashes;
        }
    }
}
