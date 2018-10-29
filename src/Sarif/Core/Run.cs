// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        public bool ShouldSerializeInvocations() { return this.Invocations != null && this.Invocations.Where((s) => s != null).FirstOrDefault() != null; }
    }
}
