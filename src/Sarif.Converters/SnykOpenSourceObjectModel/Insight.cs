// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.SnykOpenSourceObjectModel
{
    public class Insight
    {
        [JsonProperty("triageAdvice")]
        public string TriageAdvice { get; set; }
    }
}
