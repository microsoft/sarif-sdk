// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PylintObjectModel
{
    public class PylintLogEntry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("module")]
        public string ModuleName { get; set; }

        [JsonProperty("obj")]
        public string Object { get; set; }

        [JsonProperty("line")]
        public string Line { get; set; }

        [JsonProperty("column")]
        public string Column { get; set; }

        [JsonProperty("path")]
        public string FilePath { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("message-id")]
        public string MessageId { get; set; }
    }
}
