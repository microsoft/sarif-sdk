// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class SupportedPlatform
    {
        [JsonProperty("os-family", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string OsFamily { get; set; }

        [JsonProperty("os-name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string OsName { get; set; }

        [JsonProperty("platform", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Platform { get; set; }

        [JsonProperty("platform-family", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PlatformFamily { get; set; }

        [JsonProperty("platform-name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PlatformName { get; set; }

        [JsonProperty("release", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Release { get; set; }
    }
}
