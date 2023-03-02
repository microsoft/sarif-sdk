// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.PatternMatcher
{
    public class ArtifactProvider : IArtifactProvider
    {
        internal ArtifactProvider(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public ArtifactProvider(IEnumerable<IEnumeratedArtifact> artifacts)
        {
            Artifacts = new List<IEnumeratedArtifact>(artifacts);
        }

        public virtual IEnumerable<IEnumeratedArtifact> Artifacts { get; set; }

        public ICollection<IEnumeratedArtifact> Skipped { get; set; }

        public IFileSystem FileSystem { get; set; }
    }
    
    public class SinglethreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        public SinglethreadedZipArchiveArtifactProvider(ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            var artifacts = new List<IEnumeratedArtifact>();

            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                artifacts.Add(new EnumeratedArtifact(Sarif.FileSystem.Instance)
                {
                    Uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute),
                    Contents = new StreamReader(entry.Open()).ReadToEnd()
                });
            }

            Artifacts = artifacts;
        }
    }

    public class MultithreadedZipArchiveArtifactProvider : ArtifactProvider
    {
        private readonly ZipArchive zipArchive;

        public MultithreadedZipArchiveArtifactProvider(ZipArchive zipArchive, IFileSystem fileSystem) : base(fileSystem)
        {
            this.zipArchive = zipArchive;
        }

        public override IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                foreach (ZipArchiveEntry entry in this.zipArchive.Entries)
                {
                    yield return new ZipArchiveArtifact(this.zipArchive, entry);
                }
            }
        }
    }

    public class ZipArchiveArtifact : IEnumeratedArtifact
    {
        private ZipArchiveEntry entry;
        private readonly ZipArchive archive;
        private readonly Uri uri;
        private string contents;

        public ZipArchiveArtifact(ZipArchive archive, ZipArchiveEntry entry)
        {
            this.entry = entry;
            this.archive = archive;
            this.uri = new Uri(entry.FullName, UriKind.RelativeOrAbsolute);
        }

        public Uri Uri => this.uri;

        public Stream Stream { 
            get
            { 
                lock (this.archive)
                {
                    return entry != null
                        ? entry.Open()
                        : null;
                }
            }
            set => throw new NotImplementedException(); 
        }

        public Encoding Encoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Contents
        {
            get
            {
                lock (this.archive)
                {
                    if (this.contents != null) { return this.contents; }
                    this.contents = new StreamReader(Stream).ReadToEnd();
                    this.entry = null;
                    return this.contents;
                }
            }
            set => throw new NotImplementedException();
        }

        public ulong? SizeInBytes 
        { 
            get
            {
                lock (this.archive)
                {
                    if (Stream != null)
                    {
                        return (ulong)Stream.Length;
                    }
                    return (ulong)this.contents.Length;
                }
            }
            set => throw new NotImplementedException(); 
        }
    }
}
