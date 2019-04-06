// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class ReportingDescriptor
    {
        public string Format(string messageId, IEnumerable<string> arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, this.MessageStrings[messageId].Text, arguments.ToArray());
        }

        public bool ShouldSerializeDeprecatedIds()
        {
            return this.DeprecatedIds.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeDeprecatedGuids()
        {
            return this.DeprecatedGuids.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeDeprecatedNames()
        {
            return this.DeprecatedNames.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeRelationships()
        {
            return this.Relationships.HasAtLeastOneNonDefaultValue(ReportingDescriptorRelationship.ValueComparer);
        }

        public bool ShouldSerializeDefaultConfiguration()
        {
            return this.DefaultConfiguration != null && !this.DefaultConfiguration.ValueEquals(ReportingConfiguration.Empty);
        }
    }
}
