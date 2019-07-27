// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class WorkItemFiler
    {
        private readonly FilingTarget _filingTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="filingTarget">
        /// An object that represents the system (for example, GitHub or Azure DevOps)
        /// to which the work items will be filed.
        /// </param>
        public WorkItemFiler(FilingTarget filingTarget)
        {
            _filingTarget = filingTarget ?? throw new ArgumentNullException(nameof(filingTarget));
        }

        /// <summary>
        /// Files work items from the results in a SARIF log file.
        /// </summary>
        /// <param name="projectUri">
        /// The URI of the project in which the work items are to be filed.
        /// </param>
        /// <param name="workItemMetadata">
        /// Describes the work items to be filed.
        /// </param>
        /// <param name="personalAccessToken">
        /// Specifes the personal access used to access the project. Default: null.
        /// </param>
        /// <returns>
        /// The set of results that were filed as work items.
        /// </returns>
        public async Task<IEnumerable<WorkItemFilingMetadata>> FileWorkItems(
            Uri projectUri,
            IList<WorkItemFilingMetadata> workItemMetadata,
            string personalAccessToken = null)
        {
            if (projectUri == null) { throw new ArgumentNullException(nameof(projectUri)); }
            if (workItemMetadata == null) { throw new ArgumentNullException(nameof(workItemMetadata)); }

            await _filingTarget.Connect(projectUri, personalAccessToken);

            IEnumerable<WorkItemFilingMetadata> filedResults = await _filingTarget.FileWorkItems(workItemMetadata);

            return filedResults;
        }
    }
}
