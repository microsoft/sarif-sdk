// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Information that describes a run's identity and role within an engineering system process.
    /// </summary>
    public partial class RunAutomationDetails
    {
        public bool ShouldSerializeDescription() => this.Description != null;

        public bool ShouldSerializeId() => !string.IsNullOrWhiteSpace(this.Id);

        public bool ShouldSerializeGuid() => !string.IsNullOrWhiteSpace(this.Guid);

        public bool ShouldSerializeCorrelationGuid() => !string.IsNullOrWhiteSpace(this.CorrelationGuid);
    }
}
