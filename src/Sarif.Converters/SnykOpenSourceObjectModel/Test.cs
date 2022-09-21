// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.SnykOpenSourceObjectModel
{
    public class Test
    {
        [JsonProperty("vulnerabilities")]
        public IEnumerable<Vulnerability> Vulnerabilities { get; set; } = new List<Vulnerability>();

        [JsonProperty("displayTargetFile")]
        public string DisplayTargetFile { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("uniqueCount")]
        public int UniqueCount { get; set; }

        [JsonProperty("ok")]
        public bool Ok { get; set; }
    }
}
