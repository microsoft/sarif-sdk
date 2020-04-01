// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers.SampleModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class LineMappingStreamReaderTests
    {
        [Fact]
        public void LineMappingStreamReader_Basics()
        {
            LogModelSampleBuilder.EnsureSamplesBuilt();
            string path = LogModelSampleBuilder.SampleLogPath;

            // Read it all and find all newline indices
            byte[] content = File.ReadAllBytes(path);
            List<long> newlines = new List<long>();
            newlines.Add(-1);
            newlines.Add(-1);

            for (long i = 0; i < content.Length; ++i)
            {
                if (content[i] == (byte)'\n')
                {
                    newlines.Add(i);
                }
            }

            char[] buffer = new char[1024];
            int nextLine = 1;
            long bytesRead = 0;
            using (LineMappingStreamReader reader = new LineMappingStreamReader(File.OpenRead(path)))
            {
                while (true)
                {
                    // Read a segment of the file
                    int lengthRead = reader.Read(buffer, 0, buffer.Length);
                    if (lengthRead == 0) break;

                    // Count the file bytes now in range
                    bytesRead += reader.CurrentEncoding.GetByteCount(buffer, 0, lengthRead);

                    // Ask the LineMappingStreamReader for the position of (N, 1) for each line in range
                    for (; nextLine < newlines.Count; nextLine++)
                    {
                        long nextNewlinePosition = newlines[nextLine];
                        if (nextNewlinePosition > bytesRead) break;

                        long reportedAtPosition = reader.LineAndCharToOffset(nextLine, 1) - 1;

                        // Verify (N, 1) is one byte after the newline we found
                        Assert.Equal(nextNewlinePosition, reportedAtPosition);
                    }
                }
            }
        }

        [Fact]
        public void LineMappingStreamReader_WithJsonReader()
        {
            LogModelSampleBuilder.EnsureSamplesBuilt();
            string filePath = LogModelSampleBuilder.SampleLogPath;
            JsonSerializer serializer = new JsonSerializer();

            // Open a stream to read objects individually
            using (Stream seekingStream = File.OpenRead(filePath))
            {
                // Read the Json with a LineMappingStreamReader
                using (LineMappingStreamReader streamReader = new LineMappingStreamReader(File.OpenRead(filePath)))
                using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                {
                    // Get into the top object
                    jsonReader.Read();

                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject)
                        {
                            // Map each object to a byte position
                            long position = streamReader.LineAndCharToOffset(jsonReader.LineNumber, jsonReader.LinePosition);

                            // Create an object from the original stream
                            JObject expected = (JObject)serializer.Deserialize(jsonReader);

                            // Compare to one we get by seeking to the calculated byte offset
                            JObject actual = ReadAtPosition(serializer, seekingStream, position);

                            // Confirm both objects are the same
                            Assert.Equal(expected.ToString(), actual.ToString());
                        }
                    }
                }
            }
        }

        private static JObject ReadAtPosition(JsonSerializer serializer, Stream stream, long position)
        {
            stream.Seek(position, SeekOrigin.Begin);
            using (JsonTextReader jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                jsonReader.CloseInput = false;
                return (JObject)serializer.Deserialize(jsonReader);
            }
        }

        [Fact]
        public void LineMappingStreamReader_BomHandling()
        {
            string sampleFilePath = "elfie-arriba-utf8-bom.sarif";
            ResourceExtractor extractor = new ResourceExtractor(typeof(LineMappingStreamReaderTests));
            File.WriteAllBytes(sampleFilePath, extractor.GetResourceBytes("elfie-arriba-utf8-bom.sarif"));

            // Read the Json with a LineMappingStreamReader
            using (LineMappingStreamReader streamReader = new LineMappingStreamReader(File.OpenRead(sampleFilePath)))
            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
            {
                // Get into the top object
                jsonReader.Read();

                // Root element - index 3 (due to three BOM bytes)
                long position = streamReader.LineAndCharToOffset(jsonReader.LineNumber, jsonReader.LinePosition);

                Assert.Equal(3, position);
            }
        }
    }
}
