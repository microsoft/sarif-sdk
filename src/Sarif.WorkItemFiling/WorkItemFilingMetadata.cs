// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Describes a work item to be filed.
    /// </summary>
    public class WorkItemFilingMetadata
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AreaPath { get; set; }
        public List<string> Tags { get; set; }
        public Attachment Attachment { get; set; }
        public IReadOnlyDictionary<string, string> CustomFields { get; set; }
        public IReadOnlyList<string> CustomTags { get; set; }
        public object Object { get; set; }

        public IEnumerable<string> GetAllTags()
        {
            List<string> allTags = new List<string>();
            if (Tags != null)
            {
                allTags.AddRange(Tags);
            }

            if (CustomTags != null)
            {
                allTags.AddRange(CustomTags);
            }

            return allTags;
        }
    }
}
