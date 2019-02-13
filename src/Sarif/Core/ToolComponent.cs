// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A component of the analysis tool that was run, such as the primary driver or a plug-in.
    /// </summary>
    public partial class ToolComponent
    {        
        public bool ShouldSerializeRuleDescriptors()
        {
            return this.RuleDescriptors.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeNotificationsMetadata()
        {
            return this.NotificationDescriptors.HasAtLeastOneNonNullValue();
        }
    }
}
