// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IFileRegionsCache
    {
        Region PopulateTextRegionProperties(Region inputRegion, Uri uri, bool populateSnippet);

        Region ConstructMultilineContextSnippet(Region inputRegion, Uri uri);
    }
}
