// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    /// <summary>
    ///  JsonMapNode is a node in a JSON Map. JSON Maps describe the partial
    ///  structure of another JSON document compactly to enable constructing
    ///  subsets of it quickly.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class JsonMapNode
    {
        /// <summary>
        ///  Start is the absolute file offset of the beginning of the value of
        ///  the mapped object (the index of the '[' or '{').
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        ///  End is the absolute file offset of the end of the value of the
        ///  mapped object (the index of the ']' or '}').
        /// </summary>
        public long End { get; set; }

        /// <summary>
        ///  Nodes contains JsonMapNodes for each child of this node which is
        ///  large enough to be included in the map. The key is the property name
        ///  of the object in objects or the array index in arrays.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, JsonMapNode> Nodes { get; set; }

        /// <summary>
        ///  Count is the number of array elements (for arrays) or properties 
        ///  (for objects) in the mapped object.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        ///  For Arrays only, 'Every' indicates which proportion of array element
        ///  start positions are included in ArrayStarts. (Every = 1 means every element,
        ///  Every = 2 means every other, etc).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Every { get; set; }

        /// <summary>
        ///  For Arrays only, 'ArrayStarts' contains the start positions of the value of
        ///  some array elements. Values are delta-encoded in JSON, but have been decoded
        ///  as absolute offsets in this array. 
        ///  ArrayStarts[i] is the absolute start position of array[i*every].
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(LongArrayDeltaConverter))]
        public List<long> ArrayStarts { get; set; }
    }
}
