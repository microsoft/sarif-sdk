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
        /// <param name="inputRegion">Input region to be based on.</param>
        /// <param name="uri">Uri that will be used to get NewLineIndex from cache.</param>
        /// <param name="populateSnippet">Boolean that indicates if the snipper will be populated.</param>
        /// <returns>An instance of a Region class.</returns>
        Region PopulateTextRegionProperties(Region inputRegion, Uri uri, bool populateSnippet);

        /// <summary>
        /// Construct new region based on input region and uri.
        /// </summary>
        /// <param name="inputRegion">Input region to be based on.</param>
        /// <param name="uri">Uri that will be used to get NewLineIndex from cache.</param>
        /// <returns>a new region instance</returns>
        Region ConstructMultilineContextSnippet(Region inputRegion, Uri uri);
    }
}
