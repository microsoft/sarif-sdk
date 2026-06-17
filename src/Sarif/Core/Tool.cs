// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The analysis tool that was run.
    /// </summary>
    public partial class Tool
    {
        // This regex is not anchored to the end of the string ("$") so that a version string
        // carrying trailing build metadata (for example "2.1.3.25 built by: MY-MACHINE") still
        // yields its leading dotted-quad.
        private const string DottedQuadFileVersionPattern = @"^\d+(\.\d+){3}";

        private static readonly Regex dottedQuadFileVersionRegex = new Regex(DottedQuadFileVersionPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static Tool CreateFromAssemblyData(Assembly assembly = null,
                                                  bool omitSemanticVersion = false,
                                                  IFileSystem fileSystem = null)
        {
            assembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string location = assembly.Location;
            Version version = assembly.GetName().Version;

            // Tool identity is sourced from the assembly's own version attributes rather than the
            // on-disk Win32 version resource. Those attributes are embedded in the assembly and
            // reachable without a file path, so this also works under single-file publish, where
            // Assembly.Location returns an empty string (documented .NET behavior) and a file-based
            // read would throw.
            string name = string.IsNullOrEmpty(location)
                ? assembly.GetName().Name
                : Path.GetFileNameWithoutExtension(location);

            string fileVersionString = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                ?? version?.ToString();
            string semanticVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? fileVersionString;
            string companyName = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            string productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            string comments = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            string dottedQuadFileVersion = null;
            if (fileVersionString != version?.ToString())
            {
                dottedQuadFileVersion = ParseFileVersion(version?.ToString());
            }

            var tool = new Tool
            {
                Driver = new ToolComponent
                {
                    Name = name,
                    FullName = name + " " + (omitSemanticVersion ? null : version?.ToString()),
                    Version = omitSemanticVersion ? null : fileVersionString,
                    DottedQuadFileVersion = omitSemanticVersion ? null : dottedQuadFileVersion,
                    SemanticVersion = omitSemanticVersion ? null : semanticVersion,
                    Organization = string.IsNullOrEmpty(companyName) ? null : companyName,
                    Product = string.IsNullOrEmpty(productName) ? null : productName,
                }
            };

            if (!string.IsNullOrEmpty(comments)) { tool.Driver.SetProperty("comments", comments); }

            return tool;
        }

        internal static string ParseFileVersion(string fileVersion)
        {
            Match match = dottedQuadFileVersionRegex.Match(fileVersion);
            return match.Success ? match.Value : null;
        }

        /// <summary>
        ///  Find the ToolComponent corresponding to a ToolComponentReference.
        /// </summary>
        /// <param name="reference">ToolComponentReference to resolve</param>
        /// <returns>ToolComponent for reference</returns>
        public ToolComponent GetToolComponentFromReference(ToolComponentReference reference)
        {
            // Follows SARIF Spec 3.54.2 (toolComponent lookup)

            // No Reference: Driver
            if (reference == null) { return this.Driver; }

            // Lookup by Index if present
            if (reference.Index != -1)
            {
                if (reference.Index >= this.Extensions?.Count)
                {
                    throw new ArgumentOutOfRangeException($"ToolComponentReference referred to index {reference.Index}, but tool only has {this.Extensions?.Count ?? 0} components.");
                }

                return this.Extensions[reference.Index];
            }

            // Lookup by GUID if present
            if (reference.Guid != null)
            {
                if (this.Extensions != null)
                {
                    foreach (ToolComponent component in this.Extensions)
                    {
                        if (component.Guid == reference.Guid) { return component; }
                    }
                }

                throw new ArgumentException($"ToolComponentReference referred to Guid {reference.Guid}, which was not found in this.Extensions.");
            }

            // Neither specified? Driver.
            return this.Driver;
        }

        public bool ShouldSerializeExtensions()
        {
            return this.Extensions?.HasAtLeastOneNonNullValue() == true;
        }
    }
}
