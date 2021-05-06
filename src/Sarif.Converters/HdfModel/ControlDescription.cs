// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class ControlDescription
    {
        [JsonProperty("data", Required = Required.Default)]
        public string Data { get; set; }

        [JsonProperty("label", Required = Required.Default)]
        public string Label { get; set; }
    }
}
