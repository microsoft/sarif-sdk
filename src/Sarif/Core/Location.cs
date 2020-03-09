// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Location
    {
        public LogicalLocation LogicalLocation
        {
            get { return LogicalLocations?[0]; }
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
