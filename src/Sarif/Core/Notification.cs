// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Notification
    {
        public bool ShouldSerializeLocations() { return this.Locations.HasAtLeastOneNonDefaultValue(Location.ValueComparer); }
    }
}
