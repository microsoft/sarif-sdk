// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Abstract base for classes that represents a system (for example, GitHub or Azure DevOps)
    /// to which work items can be filed.
    /// </summary>
    public abstract class FilingClient: IDisposable
    {

        public enum SourceControlProvider
        {
            Github,
            AzureDevOps
        }

        /// <summary>
        /// The current source control provider.
        /// </summary>
        public SourceControlProvider Provider { get; set; }

        /// <summary>
        ///  The Azure DevOps account name or GitHub organization name.
        /// </summary>
        public string AccountOrOrganization { get; set; }

        /// <summary>
        /// The Azure DevOps project or GitHub repository name.
        /// </summary>
        public string ProjectOrRepository { get; set; }

        /// <summary>
        /// Connect to the project in which work items will be filed.
        /// </summary>
        /// <param name="personalAccessToken">
        /// Specifes the personal access used to access the project.
        /// </param>
        public abstract Task Connect(string personalAccessToken = null);

        /// <summary>
        /// Asynchronously file work items for the specified results.
        /// </summary>
        /// <param name="workItemFilingMetadata">
        /// Describes the work items to be filed.
        /// </param>
        /// <returns>
        /// An object that can be awaited to see the result groups that were actually filed.
        /// </returns>
        public abstract Task<IEnumerable<WorkItemModel>> FileWorkItems(IEnumerable<WorkItemModel> workItemModels);

        public virtual void Dispose()
        {
            // This method isn't abstract because we don't required all derived classes to implement.
            // Specifically, the GitHubClient type isn't disposable, so its wrapper has no work here.
        }
    }
}
