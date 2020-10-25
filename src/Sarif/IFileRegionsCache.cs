// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IFileRegionsCache
    {
        /// <summary>
        /// Accepts a region, uri, and boolean and returns a Region object, based on the input
        /// region property, that has all its properties populated. If an
        /// input text region, for example, only specifies the startLine property, the returned
        /// Region instance will have computed and populated other properties, such as charOffset,
        /// charLength, etc.
        /// </summary>
        /// <param name="inputRegion"></param>
        /// <param name="uri"></param>
        /// <param name="populateSnippet"></param>
        /// <returns></returns>
        Region PopulateTextRegionProperties(Region inputRegion, Uri uri, bool populateSnippet);

        Region ConstructMultilineContextSnippet(Region inputRegion, Uri uri);
    }
}
