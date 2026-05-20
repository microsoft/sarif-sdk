// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// One line of the append-only event log that backs incremental SARIF authoring.
    /// </summary>
    /// <remarks>
    /// Wire shape: <c>{"v":1,"kind":"&lt;kind&gt;","payload":{ ... }}</c>.
    /// The payload is a SARIF object (Run header, Result, Notification, or Invocation) and is
    /// preserved as a <see cref="JToken"/> until consumers deserialize it into the appropriate
    /// strongly-typed SDK object.
    /// </remarks>
    public sealed class SarifEvent
    {
        [JsonProperty("v", Order = 0)]
        public int Version { get; set; } = SarifEventKinds.CurrentSchemaVersion;

        [JsonProperty("kind", Order = 1)]
        public string Kind { get; set; }

        [JsonProperty("payload", Order = 2)]
        public JToken Payload { get; set; }
    }
}
