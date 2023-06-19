// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class WaiverData
    {
        [JsonProperty("expiration_date", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ExpirationDate { get; set; }

        [JsonProperty("justification", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Justification { get; set; }

        [JsonProperty("message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("run", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool? Run { get; set; }

        [JsonProperty("skipped_due_to_waiver", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SkippedDueToWaiver? SkippedDueToWaiver { get; set; }
    }
}
