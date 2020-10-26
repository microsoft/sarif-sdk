// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public static class Extensions
    {
        public static bool RefersToDriver(this ToolComponentReference toolComponent, string driverGuid)
        {
            if (toolComponent.Index == -1)
            {
                if (toolComponent.Guid == null)
                {
                    return true;
                }
                else
                {
                    return toolComponent.Guid.Equals(driverGuid, StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }
    }
}
