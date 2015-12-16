using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class SarifExtensionMethods
    {
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
