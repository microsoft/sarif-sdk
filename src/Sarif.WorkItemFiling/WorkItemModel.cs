// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Describes a work item to be filed.
    /// </summary>
    public class WorkItemModel
    {
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

        public object AdditionalData { get; set; }
    }
}
