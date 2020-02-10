// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WorkItemFiling;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class LogFileToWorkItemsFiler
    {
        private readonly FilingClient _filingTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileToWorkItemsFiler"> class.</see>
        /// </summary>
        /// <param name="filingTarget">
        /// An object that represents the system (for example, GitHub or Azure DevOps)
        /// to which the work items will be filed.
        /// </param>
        public LogFileToWorkItemsFiler(FilingClient filingTarget)
        {
            _filingTarget = filingTarget ?? throw new ArgumentNullException(nameof(filingTarget));
        }

        /// <summary>
        /// Files work items from the results in a SARIF log file.
        /// </summary>
        /// <param name="projectUri">
        /// The URI of the project in which the work items are to be filed.
        /// </param>
        /// <param name="workItemFilingModels">
        /// Describes the work items to be filed.
        /// </param>
        /// <param name="personalAccessToken">
        /// Specifes the personal access used to access the project. Default: null.
        /// </param>
        /// <returns>
        /// The set of results that were filed as work items.
        /// </returns>
        public async Task<IEnumerable<WorkItemModel>> FileWorkItems(
            Uri projectUri,
            IList<WorkItemModel> workItemFilingModels,
            string personalAccessToken = null)
        {
            if (projectUri == null) { throw new ArgumentNullException(nameof(projectUri)); }
            if (workItemFilingModels == null) { throw new ArgumentNullException(nameof(workItemFilingModels)); }

            await _filingTarget.Connect(personalAccessToken);

            IEnumerable<WorkItemModel> filedResults = await _filingTarget.FileWorkItems(workItemFilingModels);

            return filedResults;
        }
    }
}
