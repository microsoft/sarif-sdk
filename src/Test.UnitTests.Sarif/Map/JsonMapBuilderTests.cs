// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    public class JsonMapBuilderTests
    {
        private static ResourceExtractor Extractor = new ResourceExtractor(typeof(JsonMapBuilderTests));

        [Fact]
        public void JsonMapBuilder_Basic_20x()
        {
            string sampleFilePath = @"Map.Sample.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"Map.Sample.json"));

            // Allow a map 20x the size of the original file
            JsonMapBuilder builder = new JsonMapBuilder(20);
            JsonMapNode root = builder.Build(sampleFilePath);

            Assert.NotNull(root);
            Assert.Equal(0, root.Start);         // Index of root '{'
            Assert.Equal(154, root.End);         // Index of root '}'

            Assert.Null(root.ArrayStarts);        // Not an Array
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
            Assert.Equal(10, results.Count);                                 // Array
            Assert.Equal(10, results.ArrayStarts.Count);
            Assert.Equal(1, results.Every);

            long[] absoluteStarts = new long[results.ArrayStarts.Count];
            long previous = 0;
            for(int i = 0; i < results.ArrayStarts.Count; ++i)
            {
                previous += results.ArrayStarts[i];
                absoluteStarts[i] = previous;
            }

            Assert.Equal(new long[] { 82, 85, 91, 97, 103, 113, 119, 134, 140, 146 }, results.ArrayStarts);
        }

        [Fact]
        public void JsonMapBuilder_Basic_1x()
        {
            string sampleFilePath = @"Map.Sample.json";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"Map.Sample.json"));

            // Allow a map 1x the size of the original file
            JsonMapBuilder builder = new JsonMapBuilder(1);
            JsonMapNode root = builder.Build(sampleFilePath);

            // Verify three properties but only one big enough for a node
            Assert.Equal(3, root.Count);
            Assert.Equal(1, root.Nodes?.Count ?? 0);

            // Verify all array starts fit
            JsonMapNode results;
            Assert.True(root.Nodes.TryGetValue("results", out results));
            Assert.Equal(10, results.Count);                                 // Array
            Assert.Equal(10, results.ArrayStarts.Count);
            Assert.Equal(1, results.Every);
        }
    }
}
