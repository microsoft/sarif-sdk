// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class Reference
    {
        [JsonProperty("ref", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Ref? Ref { get; set; }

        [JsonProperty("url", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("uri", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; }
    }
}
