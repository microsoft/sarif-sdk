// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class ExecJsonControl
    {
        /// <summary>
        /// The raw source code of the control. Note that if this is an overlay control, it does not
        /// include the underlying source code
        /// </summary>
        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; set; }

        [JsonProperty("desc", Required = Required.Default)]
        public string Desc { get; set; }

        [JsonProperty("descriptions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<ControlDescription> Descriptions { get; set; }

        /// <summary>
        /// The ID of this control
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("impact", Required = Required.Always)]
        [JsonConverter(typeof(MinMaxValueCheckConverter))]
        public double Impact { get; set; }

        [JsonProperty("refs", Required = Required.Default)]
        public List<Reference> Refs { get; set; }

        /// <summary>
        /// A list of all results of the controls describe blocks.
        ///
        /// For instance, if in the controls code we had the following:
        /// describe sshd_config do
        /// its('Port') { should cmp 22 }
        /// end
        /// The result of this block as a ControlResult would be appended to the results list.
        /// </summary>
        [JsonProperty("results", Required = Required.Always)]
        public List<ControlResult> Results { get; set; }

        [JsonProperty("source_location", Required = Required.Always)]
        public SourceLocation SourceLocation { get; set; }

        [JsonProperty("tags", Required = Required.Always)]
        public Dictionary<string, object> Tags { get; set; }

        [JsonProperty("title", Required = Required.Default)]
        public string Title { get; set; }

        [JsonProperty("waiver_data", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public WaiverData WaiverData { get; set; }
    }
}
