// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Contains a function to make it less verbose to make key value pairs. Similar to C++'s
    /// std::make_pair.</summary> 
    public static class Pair
    {
        /// <summary>Makes a pair, with the generic type parameters determined using generic argument deduction.</summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="key">The key to use in the new pair.</param>
        /// <param name="value">The value to use in the new pair.</param>
        /// <returns>A KeyValuePair{TKey,TValue} containing the supplied key and value.</returns>
        public static KeyValuePair<TKey, TValue> Make<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
