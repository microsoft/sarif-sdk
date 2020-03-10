// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A reference to a ToolComponent defined at the Run level.
    /// </summary>
    public partial class ToolComponentReference
    {
        /// <summary>
        ///  Look up the ToolComponent for this ToolComponentReference.
        /// </summary>
        /// <param name="containingRun">Run instance within which this ToolComponentReference occurs.</param>
        /// <returns>ToolComponent instance referenced by this reference.</returns>
        public ToolComponent GetToolComponent(Run containingRun)
        {
            return containingRun.GetToolComponentFromReference(this);
        }
    }
}
