// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Rule
    {
        public string Format(string messageId, IEnumerable<string> arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, this.MessageStrings[messageId], arguments.ToArray());
        }
    }
}
