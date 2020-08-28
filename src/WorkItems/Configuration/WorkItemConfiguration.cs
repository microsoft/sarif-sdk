using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems.Configuration
{
    public class WorkItemConfiguration
    {
        public string PersonalAccessToken;
        public bool SyncWorkItemMetadata;
        public bool ShouldFileUnchanged;
        public string SplittingStrategy;
        public string AzureDevOpsDescriptionFooter;
        public string GitHubDescriptionFooter;
        public string[] Tags;
        public string Organization;
        public string Project;
    }
}
