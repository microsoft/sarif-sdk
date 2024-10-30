// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ArtifactProvider : IArtifactProvider
    {
        private readonly Uri uri;

        internal ArtifactProvider(IFileSystem fileSystem, Uri uri = null)
        {
            FileSystem = fileSystem;
            this.uri = uri;
        }

        public ArtifactProvider(IEnumerable<IEnumeratedArtifact> artifacts) : this(fileSystem: null, uri: null)
        {
            Artifacts = new List<IEnumeratedArtifact>(artifacts);
        }

        public Uri Uri => this.uri;
       
        public virtual IEnumerable<IEnumeratedArtifact> Artifacts { get; set; }

        public IFileSystem FileSystem { get; set; }

        public virtual bool Succeeded => UnhandledException == null;

        public virtual Exception UnhandledException { get; set; }
    }
}
