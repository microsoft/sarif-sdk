// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Configuration
{
    /// <summary>
    /// This attribute disables the generated "minus" version of a given flag
    /// in ConfigurationParser.
    /// </summary>
    /// <seealso cref="System.Attribute"/>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NoMinusAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the NoMinusAttribute class.
        /// </summary>
        public NoMinusAttribute()
        {
        }
    }
}
