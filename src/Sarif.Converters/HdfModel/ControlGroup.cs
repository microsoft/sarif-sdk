// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class ControlGroup
    {
        /// <summary>
        /// The control IDs in this group
        /// </summary>
        [JsonProperty("controls", Required = Required.Always)]
        public List<string> Controls { get; set; }

        /// <summary>
        /// The unique identifier of the group
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// The name of the group
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
