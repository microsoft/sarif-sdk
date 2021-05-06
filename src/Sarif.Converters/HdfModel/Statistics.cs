// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class Statistics
    {
        /// <summary>
        /// Breakdowns of control statistics by result
        /// </summary>
        [JsonProperty("controls", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public StatisticHash Controls { get; set; }

        /// <summary>
        /// How long (in seconds) this inspec exec ran for.
        /// </summary>
        [JsonProperty("duration", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double? Duration { get; set; }
    }
}
