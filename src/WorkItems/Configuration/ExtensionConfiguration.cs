using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems.Configuration
{
    public class ExtensionConfiguration
    {
        public string Name;
        public string AssemblyPath;
        public string FullyQualifiedTypeName;
        public Dictionary<string, string> Properties;
    }
}
