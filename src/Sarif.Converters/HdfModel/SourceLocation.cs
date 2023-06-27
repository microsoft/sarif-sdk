// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class SourceLocation
    {
        /// <summary>
        /// The line at which this statement is located in the file
        /// </summary>
        [JsonProperty("line", Required = Required.Default)]
        public double Line { get; set; }

        /// <summary>
        /// Path to the file that this statement originates from
        /// </summary>
        [JsonProperty("ref", Required = Required.Default)]
        public string Ref { get; set; }
    }
}
