// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class SarifExtensionMethods
    {
        private static Regex s_semVer200 = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<prerelease>[A-Za-z0-9\-\.]+))?(\+(?<build>[A-Za-z0-9\-\.]+))?$", RegexOptions.Compiled);
        public static bool IsSemanticVersioningCompatible(this string versionString)
        {
            return s_semVer200.IsMatch(versionString);
        }

        public static Dictionary<string, string> BuildFormatSpecifiers(IEnumerable<string> resourceNames, ResourceManager resourceManager)
        {
            // Note this dictionary provides for case-insensitive keys
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string resourceName in resourceNames)
            {
                string resourceValue = resourceManager.GetString(resourceName);
                dictionary[resourceName] = resourceValue;
            }

            return dictionary;
        }

        public static void InitializeFromAssembly(this ToolInfo toolInfo, Assembly assembly, string prereleaseInfo = null)
        {
            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            Version version = assembly.GetName().Version;

            toolInfo.Name = name;
            toolInfo.Version = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
            toolInfo.FullName = name + " " + toolInfo.Version + (prereleaseInfo ?? "");
        }
    }
}
