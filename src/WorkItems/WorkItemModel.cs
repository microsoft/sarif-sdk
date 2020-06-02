// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.WorkItems
{
    public class WorkItemModel
    {
        // TODO: Provide a meaningful representation of work item state in the model.
        //       https://github.com/microsoft/sarif-sdk/issues/1758

        public Guid Guid { get; set; }

        public string OwnerOrAccount { get; set; }

        public string RepositoryOrProject { get; set; }

        public string Title { get; set; }

        public string BodyOrDescription { get; set; }

        public string CommentOrDiscussion { get; set; }

        public string Area { get; set; }

        public string Iteration { get; set; }

        public string State { get; set; }

        public IList<string> Assignees { get; set; }

        public string Milestone { get; set; }

        public IList<string> LabelsOrTags { get; set; }

        public Attachment Attachment { get; set; }

        public IDictionary<string, string> CustomFields { get; set; }

        /// <summary>
        /// URI to raw work item data.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// URI to work item as rendered in browser to users.
        /// </summary>
        public Uri HtmlUri { get; set; }

        /// <summary>
        /// Operation to perform with the model. Ex: Create work item, Update work item.
        /// </summary>
        public WorkItemOperation Operation { get; set; }
    }

    /// <summary>
    /// Describes a work item to be filed.
    /// </summary>
    public class WorkItemModel<T> : WorkItemModel
    {
        public T Context { get; set; }
    }
}
