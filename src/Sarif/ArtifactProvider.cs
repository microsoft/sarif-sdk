// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ArtifactProvider : IArtifactProvider
    {
        private readonly Uri uri;

        internal ArtifactProvider(IFileSystem fileSystem, Uri uri = null)
        {
            FileSystem = fileSystem;
            this.uri = uri;

            /*
            this.ExtensionsDenyList =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".bmp",
                    ".cab",
                    ".cer",
                    ".der",
                    ".dll",
                    ".exe",
                    ".gif",
                    ".gz",
                    ".iso",
                    ".jpe",
                    ".jpeg",
                    ".lock",
                    ".p12",
                    ".pack",
                    ".pfx",
                    ".pkcs12",
                    ".png",
                    ".psd",
                    ".rar",
                    ".tar",
                    ".tif",
                    ".tiff",
                    ".xcf",
                    ".zip",
                };*/
        }

        public ArtifactProvider(IEnumerable<IEnumeratedArtifact> artifacts) : this(fileSystem: null, uri: null)
        {
            Artifacts = new List<IEnumeratedArtifact>(artifacts);
        }

        public Uri Uri => this.uri;

        public virtual ISet<string> ExtensionsDenyList { get; protected set; }

        public virtual ISet<string> ExtensionsAllowList { get; protected set; }


        public virtual bool IsExcluded(IEnumeratedArtifact artifact)
        {
            if (artifact == null) { throw new ArgumentNullException(nameof(artifact)); }

            string extension = Path.GetExtension(artifact.Uri.LocalPath);

            if (ExtensionsAllowList != null)
            {
                return !ExtensionsAllowList.Contains(extension);
            }

            if (ExtensionsDenyList != null)
            {
                return ExtensionsDenyList.Contains(extension);
            }

            return false;
        }

        public virtual IEnumerable<IEnumeratedArtifact> Artifacts { get; set; }

        public IFileSystem FileSystem { get; set; }

        public virtual bool Succeeded => UnhandledException == null;

        public virtual Exception UnhandledException { get; set; }
    }
}
