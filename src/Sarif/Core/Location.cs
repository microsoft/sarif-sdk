// Copyright (c) Microsoft.  All Rights Reserved.
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
                if (locations != null && locations.Count > 0)
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
                IList<LogicalLocation> locations = LogicalLocations;
                locations.Clear();

                if (value != null)
                {
                    locations.Add(value);
                }
            }
        }
    }
}
