// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Values specifying the valid targets of the <see cref="ReportingDescriptorReference"/> object being
    /// visited.
    /// </summary>
    /// <remarks>
    /// This is a Flags enum because some ReportingDescriptorReference-valued properties can
    /// validly refer to more than one kind of <see cref="ReportingDescriptor"/>.
    /// </remarks>
    [Flags]
    public enum ReportingDescriptorReferenceKinds
    {
        /// <summary>
        /// No ReportingDescriptorReference object is being visited.
        /// </summary>
        None,

        /// <summary>
        /// The ReportingDescriptorReference being visited can refer to a rule.
        /// </summary>
        Rule,

        /// <summary>
        /// The ReportingDescriptorReference being visited can refer to a notification.
        /// </summary>
        Notification,

        /// <summary>
        /// The ReportingDescriptorReference being visited can refer to a taxon.
        /// </summary>
        Taxon
    }
}