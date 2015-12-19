// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Configuration
{
    /// <summary>
    /// Attribute for specifying a configuration field. Allows the field to be made required, and allows override of the field name, short name, and validation method.
    /// </summary>
    /// <seealso cref="System.Attribute"/>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FieldAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the FieldAttribute class.</summary>
        public FieldAttribute()
        {
        }

        /// <summary>Gets or sets the name of the field.</summary>
        /// <value>The name of the field.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the short form of the name of the field.</summary>
        /// <value>The short form of the name of the field.</value>
        public string ShortName { get; set; }

        /// <summary>Gets or sets the usage, or help text.</summary>
        /// <value>The usage, or help text.</value>
        public string Usage { get; set; }

        /// <summary>Gets or sets the type, or option flags, on the field.</summary>
        /// <value>The type / option flags of the field.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "GetType() is inherited from System.Object and cannot be removed.")]
        public FieldTypes Type { get; set; }
    }
}
