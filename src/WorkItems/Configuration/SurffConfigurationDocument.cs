using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems.Configuration
{
    public class SurffConfigurationDocument
    {
        public SurffConfiguration Default;
        public Dictionary<string, SurffConfiguration> Tools;
    }
}
