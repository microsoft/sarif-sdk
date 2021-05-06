// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class Platform
    {
        /// <summary>
        /// The name of the platform this was run on.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The version of the platform this was run on.
        /// </summary>
        [JsonProperty("release", Required = Required.Always)]
        public string Release { get; set; }

        [JsonProperty("target_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string TargetId { get; set; }
    }
}
