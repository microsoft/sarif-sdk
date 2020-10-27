// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class ReportingDescriptor
    {
        public string Moniker
        {
            get
            {
                string moniker = this.Id;
                if (!string.IsNullOrWhiteSpace(this.Name))
                {
                    moniker += $".{this.Name}";
                }

                return moniker;
            }
        }

        public string Format(string messageId, IEnumerable<string> arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, this.MessageStrings[messageId].Text, arguments.ToArray());
        }
    }
}
