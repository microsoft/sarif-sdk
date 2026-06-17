// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class AggregatingArtifactsProvider : IArtifactProvider
    {
        public AggregatingArtifactsProvider(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IEnumerable<IArtifactProvider> Providers
        {
            get; set;
        }

        public IEnumerable<IEnumeratedArtifact> Artifacts
        {
            get
            {
                if (Providers == null) { yield break; }

                foreach (IArtifactProvider provider in Providers)
                {
                    foreach (IEnumeratedArtifact artifact in provider.Artifacts)
                    {
                        yield return artifact;
                    }
                }
            }
            set => throw new NotImplementedException();
        }

        public ICollection<IEnumeratedArtifact> Skipped { get; set; }

        public IFileSystem FileSystem { get; set; }
    }
}
