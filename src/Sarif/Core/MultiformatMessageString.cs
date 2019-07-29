// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class MultiformatMessageString
    {
        public bool ShouldSerializeMarkdown() { return !string.IsNullOrWhiteSpace(this.Markdown); }

        public bool ShouldSerializeText() { return !string.IsNullOrWhiteSpace(this.Text); }
    }
}
