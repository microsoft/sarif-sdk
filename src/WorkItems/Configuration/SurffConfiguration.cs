using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems.Configuration
{
    public class SurffConfiguration
    {
        public bool IsEnabled;
        public ExtensionConfiguration[] Extensions;
        public WorkItemConfiguration WorkItem;
    }
}
