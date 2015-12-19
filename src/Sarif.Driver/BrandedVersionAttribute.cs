// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Attribute for branded version information.</summary>
    /// <seealso cref="T:System.Attribute"/>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class BrandedVersionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrandedVersionAttribute"/> class.
        /// </summary>
        /// <param name="brandedVersion">The branded version.</param>
        public BrandedVersionAttribute(string brandedVersion)
        {
            this.BrandedVersion = brandedVersion;
        }

        /// <summary>
        /// Gets the branded version for the assembly. For example, externally shipping assemblies are
        /// typically branded with a year instead of the SDL version.
        /// </summary>
        /// <value>The branded version.</value>
        public string BrandedVersion { get; private set; }
    }
}
