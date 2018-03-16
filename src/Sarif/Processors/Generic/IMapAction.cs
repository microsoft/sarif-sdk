// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public interface IMapAction<T> : IActionWrapper<T>
    {
        /// <summary>
        /// Take an action on each value in a list, then return the result.
        /// </summary>
        IEnumerable<T> Map(IEnumerable<T> collection);
    }
}
