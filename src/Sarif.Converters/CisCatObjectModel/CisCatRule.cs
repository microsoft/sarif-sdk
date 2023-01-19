// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.CisCatObjectModel
{
    public class CisCatRule
    {
        [JsonProperty("rule-id")]
        public string RuleId { get; set; }

        [JsonProperty("rule-title")]
        public string RuleTitle { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        public bool IsPass()
        {
            return this.Result == "pass";
        }
    }
}
