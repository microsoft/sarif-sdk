// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
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
        // This regex does not anchor to the end of the string ("$") because FileVersionInfo
        // can contain additional information, for example: "2.1.3.25 built by: MY-MACHINE".
        private const string DottedQuadFileVersionPattern = @"^\d+(\.\d+){3}";

        private static readonly Regex dottedQuadFileVersionRegex = new Regex(DottedQuadFileVersionPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static Tool CreateFromAssemblyData(Assembly assembly = null, string prereleaseInfo = null)
        {
            assembly = assembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            Version version = assembly.GetName().Version;

            string dottedQuadFileVersion = null;

            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (fileVersion.FileVersion != version.ToString())
            {
                dottedQuadFileVersion = ParseFileVersion(fileVersion.FileVersion);
            }

            Tool tool = new Tool
            {
                Driver = new ToolComponent
                {
                    Name = name,
                    FullName = name + " " + version.ToString() + (prereleaseInfo ?? ""),
                    Version = version.ToString(),
                    DottedQuadFileVersion = dottedQuadFileVersion,
                    SemanticVersion = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString(),
                    Organization = string.IsNullOrEmpty(fileVersion.CompanyName) ? null : fileVersion.CompanyName,
                    Product = string.IsNullOrEmpty(fileVersion.ProductName) ? null : fileVersion.ProductName,
                }
            };

            SetDriverPropertiesFromFileVersionInfo(tool.Driver, fileVersion);

            return tool;
        }

        internal static string ParseFileVersion(string fileVersion)
        {
            Match match = dottedQuadFileVersionRegex.Match(fileVersion);
            return (match.Success) ? match.Value : null;
        }

        private static void SetDriverPropertiesFromFileVersionInfo(ToolComponent driver, FileVersionInfo fileVersion)
        {
            if (!string.IsNullOrEmpty(fileVersion.Comments)) { driver.SetProperty("Comments", fileVersion.Comments); }
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
            if (!string.IsNullOrEmpty(reference.Guid))
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
    }
}
