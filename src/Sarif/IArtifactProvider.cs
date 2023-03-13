﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IArtifactProvider
    {
        IEnumerable<IEnumeratedArtifact> Artifacts { get; set; }

        ICollection<IEnumeratedArtifact> Skipped { get; set; }

        IFileSystem FileSystem { get; set; }
    }
}
