// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Describes a work item to be filed.
    /// </summary>
    public class WorkItemModel<T>
    {
        public T Data { get; set; }

        public string AccountorOrganization { get; set; }
     
        public string ProjectOrRepository { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string Description { get; set; }
        
        public string Discussion { get; set; }

        public string Area { get; set; }

        public string Iteration { get; set; }

        public IList<string> Tags { get; set; }

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
    }
}
