// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.WorkItems
{
    /// <summary>
    /// Represents a file attached to a work item.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// The textual contents of the attachment.
        /// </summary>
        /// <remarks>
        /// Binary attachments are not yet supported.
        /// </remarks>
        public string Text { get; set; }


        /// <summary>
        /// The name of the attachment.
        /// </summary>
        public string Name { get; set; }
    }
}
