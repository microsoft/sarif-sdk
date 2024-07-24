﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Readers.SampleModel;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class DeferredCollectionsTests
    {
        [Fact]
        public void EndToEnd_NormalLog()
        {
            CompareReadNormalToReadDeferred(LogModelSampleBuilder.SampleLogPath);
        }

        [Fact]
        public void EndToEnd_EmptyLog()
        {
            CompareReadNormalToReadDeferred(LogModelSampleBuilder.SampleEmptyPath);
        }

        [Fact]
        public void EndToEnd_NoDictionary()
        {
            CompareReadNormalToReadDeferred(LogModelSampleBuilder.SampleNoCodeContextsPath);
        }

        [Fact]
        public void EndToEnd_SingleLineJson()
        {
            CompareReadNormalToReadDeferred(LogModelSampleBuilder.SampleOneLinePath);
        }

        [Fact]
        public void EndToEnd_NormalLog_WithStream()
        {
            CompareReadNormalToReadDeferredWithStreams(LogModelSampleBuilder.SampleLogPath);
        }

        [Fact]
        public void EndToEnd_EmptyLog_WithStream()
        {
            CompareReadNormalToReadDeferredWithStreams(LogModelSampleBuilder.SampleEmptyPath);
        }

        [Fact]
        public void EndToEnd_NoDictionary_WithStream()
        {
            CompareReadNormalToReadDeferredWithStreams(LogModelSampleBuilder.SampleNoCodeContextsPath);
        }

        [Fact]
        public void EndToEnd_SingleLineJson_WithStream()
        {
            CompareReadNormalToReadDeferredWithStreams(LogModelSampleBuilder.SampleOneLinePath);
        }

        private static void CompareReadNormalToReadDeferredWithStreams(string filePath)
        {
            LogModelSampleBuilder.EnsureSamplesBuilt();
            var serializer = new JsonSerializer();

            Log expected;
            Log actual;
            // Read normally (JsonSerializer -> JsonTextReader -> StreamReader)
            using (var reader = new JsonTextReader(new StreamReader(filePath)))
            {
                expected = serializer.Deserialize<Log>(reader);
                Assert.IsType<Dictionary<string, CodeContext>>(expected.CodeContexts);
                Assert.IsType<List<LogMessage>>(expected.Messages);
            }

            // Read with Deferred collections
            serializer.ContractResolver = new LogModelDeferredContractResolver();
            Stream contents = File.OpenRead(filePath);

            using (var reader = JsonPositionedTextReader.FromStream(contents))
            {
                actual = serializer.Deserialize<Log>(reader);
                Assert.IsType<DeferredDictionary<CodeContext>>(actual.CodeContexts);
                Assert.IsType<DeferredList<LogMessage>>(actual.Messages);
            }

            CompareReadNormalToReadDeferredLogs(expected, actual);
        }

        private static void CompareReadNormalToReadDeferredLogs(Log expected, Log actual)
        {
            // Deep compare objects which were returned
            AssertEqual(expected, actual);

            // DeferredList Code Coverage - CopyTo()
            var messages = new LogMessage[actual.Messages.Count + 1];
            actual.Messages.CopyTo(messages, 1);
            if (actual.Messages.Count > 0) { Assert.Equal<LogMessage>(actual.Messages[0], messages[1]); }

            // DeferredDictionary Code Coverage
            CodeContext context;

            // TryGetValue
            Assert.False(actual.CodeContexts.TryGetValue("missing", out context));
            if (actual.CodeContexts.Count > 0) { Assert.True(actual.CodeContexts.TryGetValue("load", out context)); }

            // ContainsKey
            Assert.False(actual.CodeContexts.ContainsKey("missing"));
            if (actual.CodeContexts.Count > 0) { Assert.True(actual.CodeContexts.ContainsKey("load")); }

            // Contains
            context = new CodeContext() { Name = "LoadRules()", Type = CodeContextType.Method, ParentContextID = "run" };
            Assert.False(actual.CodeContexts.Contains(new KeyValuePair<string, CodeContext>("missing", context)));        // Missing Key
            Assert.False(actual.CodeContexts.Contains(new KeyValuePair<string, CodeContext>("run", context)));            // Different Value

            if (actual.CodeContexts.Count > 0)
            {
                Assert.True(actual.CodeContexts.Contains(new KeyValuePair<string, CodeContext>("load", context)));        // Match
                Assert.False(actual.CodeContexts.Contains(new KeyValuePair<string, CodeContext>("load", null)));          // Match vs. Null
            }

            // CopyTo
            var contexts = new KeyValuePair<string, CodeContext>[actual.CodeContexts.Count + 1];
            actual.CodeContexts.CopyTo(contexts, 1);
            if (actual.CodeContexts.Count > 0) { Assert.Equal(actual.CodeContexts.First(), contexts[1]); }

            // Enumeration
            var contextsCopy = new Dictionary<string, CodeContext>();
            foreach (KeyValuePair<string, CodeContext> pair in actual.CodeContexts)
            {
                contextsCopy[pair.Key] = pair.Value;
            }
            Assert.Equal(actual.CodeContexts.Count, contextsCopy.Count);

            // Enumerate Keys
            int keyCount = 0;
            foreach (string key in actual.CodeContexts.Keys)
            {
                Assert.True(contextsCopy.ContainsKey(key));
                keyCount++;
            }
            Assert.Equal(contextsCopy.Count, keyCount);

            // Enumerate Values
            int valueCount = 0;
            foreach (CodeContext value in actual.CodeContexts.Values)
            {
                Assert.True(contextsCopy.ContainsValue(value));
                valueCount++;
            }
            Assert.Equal(contextsCopy.Count, valueCount);
        }

        private static void CompareReadNormalToReadDeferred(string filePath)
        {
            LogModelSampleBuilder.EnsureSamplesBuilt();
            var serializer = new JsonSerializer();

            Log actual;
            Log expected;

            // Read normally (JsonSerializer -> JsonTextReader -> StreamReader)
            using var normalReading = new JsonTextReader(new StreamReader(filePath));
            expected = serializer.Deserialize<Log>(normalReading);
            Assert.IsType<Dictionary<string, CodeContext>>(expected.CodeContexts);
            Assert.IsType<List<LogMessage>>(expected.Messages);

            // Read with Deferred collections
            serializer.ContractResolver = new LogModelDeferredContractResolver();
            using var deferredReading = new JsonPositionedTextReader(filePath);
            actual = serializer.Deserialize<Log>(deferredReading);
            Assert.IsType<DeferredDictionary<CodeContext>>(actual.CodeContexts);
            Assert.IsType<DeferredList<LogMessage>>(actual.Messages);

            CompareReadNormalToReadDeferredLogs(expected, actual);
        }

        private static void AssertEqual(Log expected, Log actual)
        {
            // Validate top level properties (which shouldn't have been read any differently)
            Assert.Equal(expected.ID, actual.ID);
            Assert.Equal(expected.StartTimeUtc, actual.StartTimeUtc);
            Assert.Equal(expected.ApplicationContext, actual.ApplicationContext);

            // Validate DeferredDictionary has the right keys and all equal values
            foreach (KeyValuePair<string, CodeContext> item in actual.CodeContexts)
            {
                Assert.Equal(expected.CodeContexts[item.Key], item.Value);
            }
            Assert.Equal(expected.CodeContexts.Count, actual.CodeContexts.Count);

            // Verify DeferredList has the right count and reconstructs identical messages
            int count = 0;
            foreach (LogMessage message in actual.Messages)
            {
                Assert.Equal(expected.Messages[count++], message);
            }

            // Enumerate list again via indexer
            for (int i = 0; i < actual.Messages.Count; ++i)
            {
                Assert.Equal(expected.Messages[i], actual.Messages[i]);
            }
        }
    }
}
