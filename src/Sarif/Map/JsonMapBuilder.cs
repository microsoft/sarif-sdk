// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    /// <summary>
    ///  JsonMapBuilder constructs a JsonMap for input Json files. It is passed
    ///  a size percentage target which controls how much detail it includes.
    /// </summary>
    public static class JsonMapBuilder
    {
        /// <summary>
        ///  Build the JsonMap for a given Json file, by path.
        ///  Returns null if the source file was too small for any map nodes to fit the size budget.
        /// </summary>
        /// <param name="filePath">File Path to the Json file to build a map from</param>
        /// <param name="settings">JsonMapSettings for map; null for defaults</param>
        /// <returns>JsonMap for file or null if file too small for map</returns>
        public static JsonMapNode Build(string filePath, JsonMapSettings settings = null)
        {
            return Build(() => File.OpenRead(filePath), settings);
        }

        /// <summary>
        ///  Build the JsonMap for a given Json file, given a stream provider.
        ///  Returns null if the source file was too small for any map nodes to fit the size budget.
        /// </summary>
        /// <param name="streamProvider">A function which will open the stream for the desired file</param>
        /// <param name="settings">JsonMapSettings for map; null for defaults</param>
        /// <returns>JsonMap for file or null if file too small for map</returns>
        public static JsonMapNode Build(Func<Stream> streamProvider, JsonMapSettings settings = null)
        {
            JsonMapRunSettings runSettings = null;

            using (Stream stream = streamProvider())
            {
                long length = stream.Length;

                // Compute JsonMapSettings for this specific file
                runSettings = new JsonMapRunSettings(length, settings ?? JsonMapRunSettings.DefaultSettings);

                // Don't build the map at all if the file is too small for anything to be mapped
                if (length <= runSettings.MinimumSizeForNode) { return null; }
            }

            // Parse file using JsonPositionedTextReader so map can get byte locations of elements
            using (JsonPositionedTextReader reader = new JsonPositionedTextReader(streamProvider))
            {
                if (!reader.Read()) { return null; }
                return Build(reader, runSettings, startPosition: 0, out long _);
            }
        }

        private static JsonMapNode Build(JsonPositionedTextReader reader, JsonMapRunSettings settings, long startPosition, out long endPosition)
        {
            // For tiny types, we know we won't create a node
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                case JsonToken.Boolean:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Date:
                case JsonToken.Comment:
                    endPosition = reader.TokenPosition;
                    reader.Read();
                    return null;

            }

            // For Strings, create a node only if the string is big enough
            if (reader.TokenType == JsonToken.String)
            {
                JsonMapNode result = null;
                endPosition = reader.TokenPosition;

                if (reader.Value.ToString().Length >= settings.MinimumSizeForNode)
                {
                    result = new JsonMapNode() { Start = startPosition, End = endPosition };
                }

                reader.Read();
                return result;
            }

            // For objects and arrays, capture the exact position, then build a node and look inside...
            JsonMapNode node = new JsonMapNode();
            node.Start = reader.TokenPosition;
            node.Count = 0;

            if (reader.TokenType == JsonToken.StartObject)
            {
                reader.Read();

                while (reader.TokenType != JsonToken.EndObject)
                {
                    // Value start is one after the ':' reader.TokenPosition is pointing to
                    long valueStartPosition = reader.TokenPosition + 1;

                    ExpectToken(JsonToken.PropertyName, reader);
                    string propertyName = reader.Value.ToString();
                    reader.Read();

                    JsonMapNode child = Build(reader, settings, valueStartPosition, out long unused);

                    if (child != null)
                    {
                        if (node.Nodes == null) { node.Nodes = new Dictionary<string, JsonMapNode>(); }
                        node.Nodes[propertyName] = child;
                    }

                    node.Count++;
                }

                ExpectToken(JsonToken.EndObject, reader);
                endPosition = reader.TokenPosition;
                node.End = endPosition;
                reader.Read();
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                node.ArrayStarts = new List<long>();

                long absoluteNextItemStart = reader.TokenPosition + 1;
                reader.Read();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    // Consider building children if nodes are large enough
                    JsonMapNode child = Build(reader, settings, absoluteNextItemStart, out long itemEnd);

                    if (child != null)
                    {
                        if (node.Nodes == null) { node.Nodes = new Dictionary<string, JsonMapNode>(); }
                        long itemIndex = node.Count;
                        node.Nodes[itemIndex.ToString()] = child;
                        absoluteNextItemStart = child.Start;
                    }

                    // Track the start of every array item
                    node.ArrayStarts.Add(absoluteNextItemStart);

                    // Next value start is two after the last value character itemEnd is pointing to (after last byte and comma)
                    absoluteNextItemStart = itemEnd + 2;

                    node.Count++;
                }

                endPosition = reader.TokenPosition;
                node.End = endPosition;
                reader.Read();

                FilterArrayStarts(node, settings);
            }
            else
            {
                throw new NotImplementedException($"Build not implemented for node type {reader.TokenType}.");
            }

            // Return the node if it was big enough
            if (node.Length >= settings.MinimumSizeForNode)
            {
                return node;
            }
            else
            {
                return null;
            }
        }

        private static void ExpectToken(JsonToken expected, JsonPositionedTextReader reader)
        {
            if (reader.TokenType != expected)
            {
                throw new JsonReaderException($"Expect: {expected} at {reader.TokenPosition:n0}, was {reader.TokenType}.");
            }
        }

        private static void FilterArrayStarts(JsonMapNode node, JsonMapRunSettings settings)
        {
            long arraySizeBytes = node.End - node.Start;
            double sizeBudget = arraySizeBytes * settings.CurrentSizeRatio;
            double countBudget = (double)((sizeBudget - JsonMapSettings.NodeSizeEstimateBytes) / JsonMapSettings.ArrayStartSizeEstimateBytes);

            if (arraySizeBytes < settings.MinimumSizeForNode || node.Count < 2 || countBudget < 2)
            {
                // Overall object too small: Keep nothing.
                node.ArrayStarts = null;
                return;
            }
            else
            {
                // Determine what proportion of items we'll keep
                int every = (int)Math.Ceiling(node.Count / countBudget);
                if (every < 1) { every = 1; }
                node.Every = every;

                // If not every item, build a new array with every Nth item
                if (every > 1)
                {
                    int newCount = (int)(node.Count / every);
                    List<long> newStarts = new List<long>(newCount);

                    for (int i = 0; i * every < node.Count; ++i)
                    {
                        newStarts.Add(node.ArrayStarts[i * every]);
                    }

                    node.ArrayStarts = newStarts;
                }
            }
        }
    }
}
