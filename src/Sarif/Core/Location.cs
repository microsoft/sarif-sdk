// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Location
    {
        public LogicalLocation LogicalLocation
        {
            get
            {
                IList<LogicalLocation> locations = LogicalLocations;
                if (locations?.Count > 0)
                {
                    return locations[0];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    LogicalLocations = new List<LogicalLocation> { value };
                }
                else
                {
                    LogicalLocations = null;
                }
            }
        }
    }
}
