// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.CisCatObjectModel
{
    public class CisCatReport
    {
        [JsonProperty("benchmark-id")]
        public string BenchmarkId { get; set; }

        [JsonProperty("benchmark-title")]
        public string BenchmarkTitle { get; set; }

        [JsonProperty("benchmark-version")]
        public string BenchmarkVersion { get; set; }

        [JsonProperty("profile-id")]
        public string ProfileId { get; set; }

        [JsonProperty("profile-title")]
        public string ProfileTitle { get; set; }

        [JsonProperty("score")]
        public string Score { get; set; }

        [JsonProperty("rules")]
        public IEnumerable<CisCatRule> Rules { get; set; }
    }
}
