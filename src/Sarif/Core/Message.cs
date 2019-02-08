// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Message
    {
        public bool ShouldSerializeArguments() { return this.Arguments.HasAtLeastOneNonNullValue(); }
    }
}
