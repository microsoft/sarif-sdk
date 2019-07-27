// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Describes a work item to be filed.
    /// </summary>
    public class WorkItemMetadata
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AreaPath { get; set; }
        public List<string> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }

        /// <summary>
        /// The results that should be filed as a single work item.
        /// </summary>
        /// <remarks>
        /// TEMPORARY. WHAT WE REALLY WANT HERE IS AN ATTACHMENT, A SARIF FILE WITH THE RELEVANT RESULTS.
        /// </remarks>
        public IList<Result> Results { get; set; }
    }
}
