// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class FileDiffingTests
    {
        public static string GetTestDirectory(string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine(@".\TestData", subdirectory));
        }

        // Retrieving the source path of the tests is only used in developer ad hoc
        // rebaselining scenarios. i.e., this path won't be consumed by AppVeyor.
        public static string GetProductTestDataDirectory(string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine(@"..\..\..\..\..\src\Sarif.UnitTests\TestData", subdirectory));
        }

        private readonly ITestOutputHelper _outputHelper;

        public FileDiffingTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            if (Directory.Exists(OutputFolderPath))
            {
                Directory.Delete(OutputFolderPath, recursive: true);
            }

            Directory.CreateDirectory(OutputFolderPath);
        }

        protected virtual Assembly ThisAssembly => this.GetType().Assembly; 

        protected virtual string TypeUnderTest => this.GetType().Name.Substring(0, this.GetType().Name.Length - "Tests".Length);

        protected virtual string OutputFolderPath => Path.Combine(Path.GetDirectoryName(ThisAssembly.Location), "UnitTestOutput." + TypeUnderTest);

        protected virtual string ProductTestDataDirectory => GetProductTestDataDirectory(TypeUnderTest);

        protected virtual string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Sarif.UnitTests.TestData." + TypeUnderTest;

        protected string GetOutputFilePath(string directory, string resourceName)
        {
            string fileName = Path.GetFileNameWithoutExtension(resourceName) + ".sarif";
            return Path.Combine(OutputFolderPath, directory, fileName);
        }

        protected string GetResourceText(string resourceName)
        {
            string text = null;

            using (Stream stream = ThisAssembly.GetManifestResourceStream($"{TestLogResourceNameRoot}.{resourceName}"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            return text;
        }

        protected byte[] GetResourceBytes(string resourceName)
        {
            byte[] bytes = null;

            using (Stream stream = ThisAssembly.GetManifestResourceStream($"{TestLogResourceNameRoot}.{resourceName}"))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }
            return bytes;
        }

        protected void ValidateResults(string output)
        {
            if (!string.IsNullOrEmpty(output))
            {
                _outputHelper.WriteLine(output);
            }

            output.Length.Should().Be(0, because: output);
        }

        protected static string GenerateDiffCommand(string suiteName, string expected, string actual)
        {
            actual = Path.GetFullPath(actual);
            expected = Path.GetFullPath(expected);

            string diffText =  String.Format(CultureInfo.InvariantCulture, "%DIFF% \"{0}\" \"{1}\"", expected, actual);

            string qualifier = String.Empty;

            if (File.Exists(expected))
            {
                qualifier = Path.GetFileNameWithoutExtension(expected);
            }

            string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            fullPath = Path.Combine(fullPath, "Diff" + suiteName + qualifier + ".cmd");

            File.WriteAllText(fullPath, diffText);
            return fullPath;
        }

        protected static bool AreEquivalentSarifLogs<T>(string actualSarif, string expectedSarif, IContractResolver contractResolver = null)
        {
            expectedSarif = expectedSarif ?? "{}";

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore
            };

            // Make sure we can successfully deserialize what was just generated
            T actualLog = JsonConvert.DeserializeObject<T>(actualSarif, settings);

            JToken generatedToken = JToken.Parse(actualSarif);
            JToken expectedToken = JToken.Parse(expectedSarif);

            return JToken.DeepEquals(generatedToken, expectedToken);
        }
    }
}
