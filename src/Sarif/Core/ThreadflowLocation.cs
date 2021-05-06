// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A thread flow location of a SARIF thread flow.
    /// </summary>
    public partial class ThreadFlowLocation
    {
        public bool ShouldSerializeKinds() { return this.Kinds.HasAtLeastOneNonNullValue(); }

        public bool ShouldSerializeTaxa() { return this.Taxa.HasAtLeastOneNonDefaultValue(ReportingDescriptorReference.ValueComparer); }
    }
}
