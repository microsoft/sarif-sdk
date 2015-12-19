// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Configuration
{
    /// <summary>
    /// Specify fields
    /// </summary>
    [Flags]
    public enum FieldTypes
    {
        /// <summary>The default, specifies that the given field is optional.</summary>
        None = 0x00,
        Optional = None,

        /// <summary>
        /// Indicates that this field is required. An error will be displayed
        /// if it is not present when parsing arguments.
        /// </summary>
        Required = 0x01,

        /// <summary>
        /// Default field--if values in command line are not preceded by an arg, they're parsed as this field
        /// </summary>
        Default = 0x02,

        /// <summary>
        /// This field will not be described in the usage summary. 
        /// </summary>
        Hidden = 0x04,

        /// <summary>
        /// Show the field on the CLI, but do not parse it from or save it to the XML, or place it in the schema. If CliOnly &amp; Required, an error will not be raised if the parameter is missing from an XML config.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cli", Justification = "This is spelled correctly.")]
        CliOnly = 0x08
    }
}
