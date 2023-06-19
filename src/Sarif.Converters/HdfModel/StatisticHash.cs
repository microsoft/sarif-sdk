// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    /// <summary>
    /// Breakdowns of control statistics by result
    /// </summary>
    public partial class StatisticHash
    {
        [JsonProperty("failed", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public StatisticBlock Failed { get; set; }

        [JsonProperty("passed", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public StatisticBlock Passed { get; set; }

        [JsonProperty("skipped", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public StatisticBlock Skipped { get; set; }
    }
}
