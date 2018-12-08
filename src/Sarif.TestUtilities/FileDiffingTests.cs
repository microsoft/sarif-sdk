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

            // We can't use the 'because' argument here because someone along
            // the line is stripping \n from output strings. This compromises
            // our file paths. e.g., 'c:\build\netcore2.0\etc' is rendered
            // as 'c:\build\etcore2.0'. 
            output.Length.Should().Be(0);
        }

        protected static string GenerateDiffCommand(string expected, string actual)
        {
            // This helper works to generate both a file or directory compare
            expected = Path.GetFullPath(expected).Replace(@"\", @"\\");
            actual = Path.GetFullPath(actual).Replace(@"\", @"\\");

            string beyondCompare = TryFindBeyondCompare();
            if (beyondCompare != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\" \"{2}\" /title1=Expected /title2=Actual", beyondCompare, expected, actual);
            }

            return String.Format(CultureInfo.InvariantCulture, "windiff \"{0}\" \"{1}\"", expected, actual);
        }

        protected static bool AreEquivalentSarifLogs<T>(string actualSarif, string expectedSarif, IContractResolver contractResolver = null)
        {
            expectedSarif = expectedSarif ?? "{}";

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            };

            // Make sure we can successfully deserialize what was just generated
            T actualLog = JsonConvert.DeserializeObject<T>(actualSarif, settings);

            actualSarif = JsonConvert.SerializeObject(actualLog, settings);

            JToken generatedToken = JToken.Parse(actualSarif);
            JToken expectedToken = JToken.Parse(expectedSarif);

            return JToken.DeepEquals(generatedToken, expectedToken);
        }

        protected static string TryFindBeyondCompare()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            for (int idx = 4; idx >= 3; --idx)
            {
                string beyondComparePath = String.Format(CultureInfo.InvariantCulture, "{0}\\Beyond Compare {1}\\BComp.exe", programFiles, idx);
                if (File.Exists(beyondComparePath))
                {
                    return beyondComparePath;
                }
            }

            programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            for (int idx = 4; idx >= 3; --idx)
            {
                string beyondComparePath = String.Format(CultureInfo.InvariantCulture, "{0}\\Beyond Compare {1}\\BComp.exe", programFiles, idx);
                if (File.Exists(beyondComparePath))
                {
                    return beyondComparePath;
                }
            }

            string beyondCompare2Path = programFiles + "\\Beyond Compare 2\\BC2.exe";
            if (File.Exists(beyondCompare2Path))
            {
                return beyondCompare2Path;
            }

            return null;
        }
    }
}
