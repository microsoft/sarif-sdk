// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class MultithreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        private static readonly byte[] ZipSignature = { 0x50, 0x4B, 0x03, 0x04 };
        private readonly ZipArchive zipArchive;
        private static ISet<string> archiveExtensions;
        private readonly Uri uri;

        public ISet<string> BinaryExtensions { get; set; } = new HashSet<string>();

        public static ISet<string> ArchiveExtensions
        {
            get
            {
                archiveExtensions ??= CreateDefaultArchiveExtensionsSet();
                return archiveExtensions;
            }

            set { archiveExtensions = value; }
        }

        public MultithreadedZipArchiveArtifactProvider(Uri uri, ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
            this.zipArchive = zipArchive ?? throw new ArgumentNullException(nameof(zipArchive));
        }

        public static ISet<string> CreateDefaultArchiveExtensionsSet()
        {
            ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            result.Add(".apk"); // Android Package
            result.Add(".appx"); // Microsoft Application Package
            result.Add(".appxbundle"); // Microsoft Application Bundle
            result.Add(".docx"); // Microsoft Word document
            result.Add(".epub"); // Electronic Publication (eBook)
            result.Add(".jar"); // Java Archive
            result.Add(".msix"); // Microsoft Installer Package
            result.Add(".msixbundle"); // Microsoft Installer Bundle
            result.Add(".odp"); // OpenDocument Presentation
            result.Add(".ods"); // OpenDocument Spreadsheet
            result.Add(".odt"); // OpenDocument Text document
            result.Add(".onepkg"); // Microsoft OneNote Package
            result.Add(".oxps"); // Open XML Paper Specification document
            result.Add(".pkg"); // Apple Installer Package (when used in certain contexts)
            result.Add(".pptx"); // Microsoft PowerPoint presentation
            result.Add(".unitypackage"); // Unity Package
            result.Add(".vsdx"); // Microsoft Visio drawing
            result.Add(".xps"); // XML Paper Specification document
            result.Add(".xlsx"); // Microsoft Excel spreadsheet
            result.Add(".zip"); // ZIP archive
            return result;
        }

        internal static bool IsOpenPackagingConventionsFile(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[ZipSignature.Length];
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                if (bytesRead < ZipSignature.Length)
                {
                    return false;
                }

                for (int i = 0; i < ZipSignature.Length; i++)
                {
                    if (buffer[i] != ZipSignature[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                foreach (ZipArchiveEntry entry in this.zipArchive.Entries)
                {
                    if (entry.FullName.EndsWith("/")) { continue; }
                    yield return new ZipArchiveArtifact(this.uri, this.zipArchive, entry, BinaryExtensions);
                }
            }
        }
    }

}
