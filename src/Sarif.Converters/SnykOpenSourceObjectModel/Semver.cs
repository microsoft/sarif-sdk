// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.SnykOpenSourceObjectModel
{
    public class Semver
    {
        [JsonProperty("vulnerable")]
        public IEnumerable<string> Vulnerable { get; set; } = new List<string>();
    }
}
