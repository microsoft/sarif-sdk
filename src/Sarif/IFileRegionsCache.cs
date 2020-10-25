// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The interface to a cache that can be used to populate regions with comprehensive data,
    /// to retrieve artifact text associated with a SARIF log, and to construct snippets for
    /// <see cref="Region"/> instances.
    /// </summary>
    public interface IFileRegionsCache
    {
        /// <summary>
        /// Creates a <see cref="Region"/> object, based on an existing Region, in which all
        /// text-related properties have been populated.
        /// </summary>
        /// <remarks>
        /// For example, if the input Region specifies only the StartLine property, the returned
        /// Region instance will have computed and populated other text-related properties, such
        /// as properties, such as CharOffset, CharLength, etc.
        /// </remarks>
        /// <param name="inputRegion">
        /// Region object that forms the basis of the returned Region object.
        /// </param>
        /// <param name="uri">
        /// URI of the artifact in which <paramref name="inputRegion"/> lies, used to retrieve
        /// from the cache the location of each newline in the artifact.
        /// </param>
        /// <param name="populateSnippet">
        /// Boolean that indicates if the region's Snippet property will be populated.
        /// </param>
        /// <returns>
        /// A Region object whose text-related properties have been fully populated.
        /// </returns>
        Region PopulateTextRegionProperties(Region inputRegion, Uri uri, bool populateSnippet);

        /// <summary>
        /// Creates a <see cref="Region"/> object, including a snippet, that can serve as a context
        /// region for an existing Region object.
        /// </summary>
        /// <param name="inputRegion">
        /// Region object for which a context region is desired.
        /// </param>
        /// <param name="uri">
        /// URI of the artifact in which <paramref name="inputRegion"/> lies, used to retrieve
        /// from the cache the artifact's text.
        /// </param>
        /// <returns>
        /// A Region object representing a valid context region for <paramref name="inputRegion"/>.
        /// </returns>
        Region ConstructMultilineContextSnippet(Region inputRegion, Uri uri);
    }
}
