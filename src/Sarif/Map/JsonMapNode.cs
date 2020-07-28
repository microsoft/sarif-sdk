// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Readers;

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
        ///  Count is the number of array elements (for arrays) or properties 
        ///  (for objects) in the mapped object.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///  Return the byte length of this node.
        ///  Since End and Start are inclusive, it's one more than the difference.
        /// </summary>
        [JsonIgnore]
        public long Length => (1 + End - Start);

        /// <summary>
        ///  Nodes contains JsonMapNodes for each child of this node which is
        ///  large enough to be included in the map. The key is the property name
        ///  of the object in objects or the array index in arrays.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, JsonMapNode> Nodes { get; set; }

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

        /// <summary>
        ///  Find the start position of the desired item index.
        ///  If the node knows it, the known value is returned.
        ///  If not, the code seeks to the nearest node and reads enough of the document to find it.
        /// </summary>
        /// <param name="index">Index of array element to find</param>
        /// <param name="inputStreamProvider">Function which can open original file, if needed</param>
        /// <returns>Absolute byte offset of the start array[index] within this array</returns>
        public long FindArrayStart(int index, Func<Stream> inputStreamProvider)
        {
            if (index < 0 || index > this.Count) { throw new ArgumentOutOfRangeException("index"); }

            if (index == this.Count)
            {
                // If item after last is requested, return array end
                return this.End;
            }
            else if (this.ArrayStarts != null && this.Every == 1)
            {
                // If we know every element position, just return it
                return this.ArrayStarts[index];
            }
            else if (index % this.Every == 0)
            {
                // If we know this element position, just return it
                return this.ArrayStarts[index / this.Every];
            }

            // Otherwise, find the closest span of the file we must read
            long readFromPosition;
            long readToPosition;
            int startIndex;

            if (this.ArrayStarts == null)
            {
                // If there are no array positions, we must read the whole array (it should be small)
                readFromPosition = this.Start + 1;
                readToPosition = this.End;
                startIndex = 0;
            }
            else
            {
                // If there are array positions, read from the nearest previous element available
                int startToRead = index / this.Every;
                readFromPosition = this.ArrayStarts[startToRead];
                readToPosition = (this.ArrayStarts.Count > startToRead + 1 ? this.ArrayStarts[startToRead + 1] : this.End);
                startIndex = startToRead * this.Every;
            }

            using (Stream source = inputStreamProvider())
            {
                int lengthToRead = (int)(1 + readToPosition - readFromPosition);
                byte[] buffer = new byte[lengthToRead + 1];

                // Read the array slice
                source.Seek(readFromPosition, SeekOrigin.Begin);
                source.Read(buffer, 1, lengthToRead);

                // Make it a valid array prefix (it must start with '[', which will look like the root of the Json document
                buffer[0] = (byte)'[';

                using (JsonPositionedTextReader reader = new JsonPositionedTextReader(() => new MemoryStream(buffer)))
                {
                    // Find the desired array item index in the buffer
                    long relativePosition = reader.ReadToArrayIndex(index - startIndex);

                    // Convert back to an absolute position (buffer[0] was (readFromPosition - 1)
                    return (readFromPosition - 1) + relativePosition;
                }
            }
        }

        /// <summary>
        ///  Copy a given inclusive range from the source stream to the destination stream.
        ///  Used to copy slices of Json identified by the map nodes to an output file.
        /// </summary>
        /// <param name="source">Stream to copy from</param>
        /// <param name="destination">Stream to copy to</param>
        /// <param name="startInclusive">Source byte offset to copy from (inclusive)</param>
        /// <param name="endInclusive">Source byte offset to copy up to (inclusive)</param>
        /// <param name="buffer"></param>
        /// <param name="omitFromLast"></param>
        public static void CopyStreamBytes(Stream source, Stream destination, long startInclusive, long endInclusive, byte[] buffer, byte? omitFromLast = null)
        {
            source.Seek(startInclusive, SeekOrigin.Begin);

            // Copying from [0, 1] is length two.
            long length = endInclusive + 1 - startInclusive;

            // Copy up to the last block
            long lengthLeft = length - buffer.Length;
            while (lengthLeft > 0)
            {
                int lengthToRead = buffer.Length;
                if (lengthLeft < lengthToRead) { lengthToRead = (int)lengthLeft; }

                int lengthRead = source.Read(buffer, 0, lengthToRead);
                destination.Write(buffer, 0, lengthRead);

                lengthLeft -= lengthRead;
            }

            // Copy the last block
            lengthLeft += buffer.Length;
            if (lengthLeft > 0)
            {
                int lengthRead = source.Read(buffer, 0, (int)lengthLeft);

                // If 'omitFromLast', exclude that byte and after *if all whitespace*
                if (omitFromLast.HasValue)
                {
                    for (int i = lengthRead - 1; i >= 0; --i)
                    {
                        if (buffer[i] == omitFromLast.Value)
                        {
                            lengthRead = i;
                            break;
                        }
                        else if (!IsWhitespace(buffer[i]))
                        {
                            break;
                        }
                    }
                }

                destination.Write(buffer, 0, lengthRead);
            }
        }

        private static bool IsWhitespace(byte b)
        {
            return b == (byte)' ' || b == (byte)'\t' || b == (byte)'\r' || b == (byte)'\n';
        }
    }
}
