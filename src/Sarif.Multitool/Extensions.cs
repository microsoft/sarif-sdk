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
