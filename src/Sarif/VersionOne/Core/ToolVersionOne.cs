// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// The analysis tool that was run.
    /// </summary>
    public partial class ToolVersionOne 
    {
        public static ToolVersionOne CreateFromAssemblyData(string prereleaseInfo = null)
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string name = Path.GetFileNameWithoutExtension(assembly.Location);

            ToolVersionOne tool = new ToolVersionOne();

            // 'name'
            tool.Name = name;

            // 'version' : primary tool version.
            Version version = assembly.GetName().Version;
            tool.Version = version.ToString();

            // Synthesized semver 2.0 version required by spec
            tool.SemanticVersion = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();

            // Binary file version
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

            if (fileVersion.FileVersion != tool.Version)
            {
                tool.FileVersion = fileVersion.FileVersion;
            }

            tool.FullName = name + " " + tool.Version + (prereleaseInfo ?? "");

            tool.Language = CultureInfo.CurrentCulture.Name;

            if (!string.IsNullOrEmpty(fileVersion.Comments)) { tool.SetProperty("Comments", fileVersion.Comments); }
            if (!string.IsNullOrEmpty(fileVersion.CompanyName)) { tool.SetProperty("CompanyName", fileVersion.CompanyName); }
            if (!string.IsNullOrEmpty(fileVersion.ProductName)) { tool.SetProperty("ProductName", fileVersion.ProductName); }

            return tool;
        }
    }
}
