// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    public class JsonMapBuilderTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(JsonMapBuilderTests));

        [Fact]
        public void JsonMapBuilder_Basic_20x()
        {
            string sampleFilePath = "Map.Sample.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText("Map.Sample.json"));

            // Allow a map 20x the size of the original file
            JsonMapNode root = JsonMapBuilder.Build(sampleFilePath, new JsonMapSettings(20));

            Assert.NotNull(root);
            Assert.Equal(0, root.Start);         // Index of root '{'
            Assert.Equal(154, root.End);         // Index of root '}'

            Assert.Null(root.ArrayStarts);       // Not an Array
            Assert.Equal(0, root.Every);

            Assert.Equal(3, root.Count);         // Version, Schema, Results
            Assert.Equal(3, root.Nodes.Count);

            JsonMapNode version;
            Assert.True(root.Nodes.TryGetValue("version", out version));
            Assert.Equal(12, version.Start);
            Assert.Equal(19, version.End);

            JsonMapNode schema;
            Assert.True(root.Nodes.TryGetValue("schema", out schema));
            Assert.Equal(31, schema.Start);
            Assert.Equal(67, schema.End);

            JsonMapNode results;
            Assert.True(root.Nodes.TryGetValue("results", out results));
            Assert.Equal(10, results.Count);
            Assert.Equal(10, results.ArrayStarts.Count);
            Assert.Equal(1, results.Every);

            long[] absoluteStarts = new long[results.ArrayStarts.Count];
            long previous = 0;
            for (int i = 0; i < results.ArrayStarts.Count; ++i)
            {
                previous += results.ArrayStarts[i];
                absoluteStarts[i] = previous;
            }

            // Positions are two after the previous end for literals and exact starts ('[' or '{') for nested objects/arrays
            Assert.Equal(new long[] { 82, 85, 91, 97, 104, 113, 120, 134, 140, 146 }, results.ArrayStarts.ToArray());
        }

        [Fact]
        public void JsonMapBuilder_Basic_2x()
        {
            string sampleFilePath = "Map.Sample.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText("Map.Sample.json"));

            // Allow a map 2x the size of the original file
            JsonMapNode root = JsonMapBuilder.Build(sampleFilePath, new JsonMapSettings(2));

            // Verify three properties but only one big enough for a node
            Assert.Equal(3, root.Count);
            Assert.Equal(1, root.Nodes?.Count ?? 0);

            // Verify all array starts fit
            JsonMapNode results;
            Assert.True(root.Nodes.TryGetValue("results", out results));
            Assert.Equal(10, results.Count);
            Assert.Equal(10, results.ArrayStarts.Count);
            Assert.Equal(1, results.Every);
        }

        [Fact]
        public void JsonMapBuilder_EveryTwo()
        {
            string sampleFilePath = "Map.TinyArray.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText("Map.TinyArray.json"));

            // File: 300b.
            // Map: Root (90b) + Array (5b x 50 (Count 100 / Every 2)) = 340b
            JsonMapNode root = JsonMapBuilder.Build(sampleFilePath, new JsonMapSettings(350.0 / 300.0));

            // 100 array elements, only 50 starts, every = 2
            Assert.Equal(100, root.Count);
            Assert.Equal(50, root.ArrayStarts.Count);
            Assert.Equal(2, root.Every);

            // Verify we see a[0], a[2], a[4], ...
            Assert.Equal(1, root.ArrayStarts[0]);
            Assert.Equal(7, root.ArrayStarts[1]);
            Assert.Equal(13, root.ArrayStarts[2]);

            // Ask the lookup method to find the positions of every element (every 3 bytes for "#, "
            for (int i = 0; i < root.Count; ++i)
            {
                long position = root.FindArrayStart(i, () => File.OpenRead(sampleFilePath));
                Assert.Equal(1 + 3 * i, position);
            }
        }

        [Fact]
        public void JsonMapBuilder_EveryFour()
        {
            string sampleFilePath = "Map.TinyArray.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText("Map.TinyArray.json"));

            // File: 300b.
            // Map: Root (90b) + Array (5b x 25 (Count 100 / Every 4)) = 215b
            JsonMapNode root = JsonMapBuilder.Build(sampleFilePath, new JsonMapSettings(220.0 / 300.0));

            // 100 array elements, only 25 starts, every = 4
            Assert.Equal(100, root.Count);
            Assert.Equal(25, root.ArrayStarts.Count);
            Assert.Equal(4, root.Every);

            // Verify we see a[0], a[4], a[8], ...
            Assert.Equal(1, root.ArrayStarts[0]);
            Assert.Equal(13, root.ArrayStarts[1]);
            Assert.Equal(25, root.ArrayStarts[2]);

            // Ask the lookup method to find the positions of every element (every 3 bytes for "#, "
            for (int i = 0; i < root.Count; ++i)
            {
                long position = root.FindArrayStart(i, () => File.OpenRead(sampleFilePath));
                Assert.Equal(1 + 3 * i, position);
            }
        }
    }
}
