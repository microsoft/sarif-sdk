// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Values that represent data model type kinds.</summary>
    internal enum DataModelTypeKind
    {
        /// <summary>An enum constant representing an uninitialized kind.</summary>
        Default,

        /// <summary>The <see cref="System.String"/> type.</summary>
        BuiltInString,

        BuiltInBoolean,

        /// <summary>The <c>double</c> or <c>bool</c> type.</summary> 
        BuiltInNumber,

        /// <summary>The <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> type (where
        /// TKey = string and TValue = string).</summary> 
        BuiltInDictionary,

        BuiltInUri,

        BuiltInVersion,

        /// <summary>
        /// A type that resolves to a constrained set of value
        /// </summary>
        Enum,

        /// <summary>Types which are lists of members provided in the G4 file. These are typically formed
        /// from productions containing a single group.</summary> 
        Leaf,

        /// <summary>Types which are lists of subtypes provided in the G4 file. These are typically formed
        /// from productions consisting of an alternation of other types.</summary> 
        Base
    }
}
