// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The analysis tool that was run.
    /// </summary>
    public partial class Tool 
    {
        public static Tool CreateFromAssemblyData(Assembly assembly = null, string prereleaseInfo = null)
        {
            assembly = assembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            Version version = assembly.GetName().Version;

            string dottedQuadFileVersion = null;

            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (fileVersion.FileVersion != version.ToString())
            {
                dottedQuadFileVersion = fileVersion.FileVersion;
            }

            Tool tool = new Tool
            {
                Language = CultureInfo.CurrentCulture.Name,
                Driver = new ToolComponent
                {
                    Name = name,
                    FullName = name + " " + version.ToString() + (prereleaseInfo ?? ""),
                    Version = version.ToString(),
                    DottedQuadFileVersion = dottedQuadFileVersion,
                    SemanticVersion = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString(),
                    Properties = CreatePropertiesFromFileVersionInfo(fileVersion)
                }
            };

            return tool;
        }

        private static IDictionary<string, SerializedPropertyInfo> CreatePropertiesFromFileVersionInfo(FileVersionInfo fileVersion)
        {
            var toolComponent = new ToolComponent();

            if (!string.IsNullOrEmpty(fileVersion.Comments)) { toolComponent.SetProperty("Comments", fileVersion.Comments); }
            if (!string.IsNullOrEmpty(fileVersion.CompanyName)) { toolComponent.SetProperty("CompanyName", fileVersion.CompanyName); }
            if (!string.IsNullOrEmpty(fileVersion.ProductName)) { toolComponent.SetProperty("ProductName", fileVersion.ProductName); }

            return toolComponent.Properties;
        }
    }
}
