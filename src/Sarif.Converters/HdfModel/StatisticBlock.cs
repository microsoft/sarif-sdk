// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class StatisticBlock
    {
        /// <summary>
        /// Total number of controls (in this category) for this inspec execution.
        /// </summary>
        [JsonProperty("total", Required = Required.Default)]
        public double Total { get; set; }
    }
}
