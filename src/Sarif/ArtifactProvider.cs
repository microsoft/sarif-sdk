// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
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

        public IFileSystem FileSystem { get; set; }
    }
}
