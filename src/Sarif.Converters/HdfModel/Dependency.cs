// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class Dependency
    {
        [JsonProperty("branch", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Branch { get; set; }

        [JsonProperty("compliance", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Compliance { get; set; }

        [JsonProperty("git", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Git { get; set; }

        [JsonProperty("name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("path", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("skip_message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SkipMessage { get; set; }

        [JsonProperty("status", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("supermarket", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Supermarket { get; set; }

        [JsonProperty("url", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}
