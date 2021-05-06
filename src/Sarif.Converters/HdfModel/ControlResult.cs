// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class ControlResult
    {
        [JsonProperty("backtrace")]
        public List<string> Backtrace { get; set; }

        [JsonProperty("code_desc", Required = Required.Always)]
        public string CodeDesc { get; set; }

        [JsonProperty("exception", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Exception { get; set; }

        [JsonProperty("message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("resource", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Resource { get; set; }

        [JsonProperty("run_time", Required = Required.Default)]
        public double RunTime { get; set; }

        [JsonProperty("skip_message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SkipMessage { get; set; }

        [JsonProperty("start_time", Required = Required.Default)]
        public string StartTime { get; set; }

        [JsonProperty("status", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ControlResultStatus? Status { get; set; }
    }
}
