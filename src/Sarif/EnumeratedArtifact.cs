// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    // TBD: this class should probably be a generic, with
    // EnumeratedArtifact<string> being a commonly utilized thing.
    public class EnumeratedArtifact : IEnumeratedArtifact
    {
        public EnumeratedArtifact(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        private string contents;

        public Uri Uri { get; set; }

        public Stream Stream { get; set; }

        public Encoding Encoding { get; set; }

        internal IFileSystem FileSystem { get; set; }

        public string Contents
        {
            get => GetContents();
            set => this.contents = value;
        }

        private string GetContents()
        {
            if (this.contents != null) { return this.contents; }

            DriverEventSource.Log.GetTargetStart(this.Uri.GetFilePath());

            if (Stream == null && this.contents == null)
            {
                // TBD we actually have no validation URI is non-null yet.
                contents = Uri!.IsFile
                    ? FileSystem.FileReadAllText(Uri.LocalPath)
                    : null;

                this.sizeInBytes = (ulong?)this.contents?.Length;
            }
            else 
            {
                if (Stream.CanSeek) { this.Stream.Seek(0, SeekOrigin.Begin); }
                using var contentReader = new StreamReader(Stream);
                this.contents = contentReader.ReadToEnd();
                Stream = null;
            }

            DriverEventSource.Log.GetTargetStop(this.Uri.GetFilePath());
            return this.contents;
        }

        public ulong? sizeInBytes;

        public ulong? SizeInBytes
        {
            get
            {
                if (this.sizeInBytes != null) 
                { 
                    return this.sizeInBytes.Value; 
                };

                if (this.Contents != null) 
                {  
                    return  (ulong?)this.Contents.Length; 
                }

                this.sizeInBytes = Uri!.IsFile
                    ? (ulong)FileSystem.FileInfoLength(Uri.LocalPath)
                    : (ulong?)null;

                return this.sizeInBytes;
            }
            set
            {
                this.sizeInBytes = value;
            }
        }
    }
}
