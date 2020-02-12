// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
/// <summary>
/// This class scrapes a SARIF log file to proactively acquire data that is 
/// relevant to filing work items from the log file. 
/// </summary>
    public class SarifWorkItemData
    {
        public SarifWorkItemData()
        {

        }

        public void InitializeFromLog(SarifLog sarifLog)
        {
            this.SourceControlledFiles = null;
            this.NonSourceControlledFiles = null;
        }

        /// <summary>
        /// URIs for source-controlled scan targets associated with the work item.
        /// </summary>
        public IEnumerable<Uri> SourceControlledFiles { get; set; }

        /// <summary>
        /// URIs for scan targets assocaited with the work item that are not
        /// source-controlled (i.e., dependencies or build outputs).
        /// </summary>
        public IEnumerable<Uri> NonSourceControlledFiles { get; set; }
    }
}
