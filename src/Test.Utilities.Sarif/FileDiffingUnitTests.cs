// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class FileDiffingUnitTests
    {
        protected virtual bool RebaselineExpectedResults => false;

        public static string GetTestDirectory(string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine(@".\TestData", subdirectory));
        }

        // Retrieving the source path of the tests is only used in developer ad hoc
        // rebaselining scenarios. i.e., this path won't be consumed by AppVeyor.
        public static string GetProductTestDataDirectory(string testBinaryName, string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine($@"..\..\..\..\..\src\{testBinaryName}\TestData", subdirectory));
        }

        private readonly ITestOutputHelper _outputHelper;
        private readonly bool _testProducesSarifCurrentVersion;
        private ResourceExtractor _resourceExtractor;

        public FileDiffingUnitTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true)
        {
            _outputHelper = outputHelper;
            _testProducesSarifCurrentVersion = testProducesSarifCurrentVersion;
            _resourceExtractor = new ResourceExtractor(this.GetType());

            Directory.CreateDirectory(OutputFolderPath);
        }

        protected virtual Assembly ThisAssembly => this.GetType().Assembly;

        protected virtual string TestBinaryName => Path.GetFileNameWithoutExtension(ThisAssembly.Location);

        protected virtual string TypeUnderTest => this.GetType().Name.Substring(0, this.GetType().Name.Length - "Tests".Length);

        protected virtual string OutputFolderPath => Path.Combine(Path.GetDirectoryName(ThisAssembly.Location), "UnitTestOutput." + TypeUnderTest);

        protected virtual string ProductTestDataDirectory => GetProductTestDataDirectory(TypeUnderTest);

        protected virtual string IntermediateTestFolder { get { return String.Empty; } }

        protected virtual string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.TestData." + TypeUnderTest;

        protected virtual string ConstructTestOutputFromInputResource(string inputResourceName) { return string.Empty; }

        protected virtual void RunTest(string inputResourceName, string expectedOutputResourceName = null)
        {
            expectedOutputResourceName = expectedOutputResourceName ?? inputResourceName;
            // When retrieving constructed test content, we pass the resourceName as the test
            // specified it. When constructing actual and expected file names from this data, 
            // however, we will ensure that the name has the ".sarif" extension. We do this
            // for test classes such as the Fortify converter that operate again non-SARIF inputs.
            string actualSarifText = ConstructTestOutputFromInputResource("Inputs." + inputResourceName);

            expectedOutputResourceName = Path.GetFileNameWithoutExtension(expectedOutputResourceName) + ".sarif";

            var sb = new StringBuilder();

            string expectedSarifText = GetResourceText("ExpectedOutputs." + expectedOutputResourceName);

            bool passed = false;

            if (!RebaselineExpectedResults)
            {
                if (_testProducesSarifCurrentVersion)
                {
                    PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(expectedSarifText, Formatting.Indented, out expectedSarifText);
                    passed = AreEquivalent<SarifLog>(actualSarifText, expectedSarifText);
                }
                else
                {
                    passed = AreEquivalent<SarifLogVersionOne>(actualSarifText, expectedSarifText, SarifContractResolverVersionOne.Instance);
                }
            }

            if (!passed)
            {
                string errorMessage = string.Format(@"there should be no unexpected diffs detected comparing actual results to '{0}'.", inputResourceName);
                sb.AppendLine(errorMessage);

                if (!Utilities.RunningInAppVeyor)
                {
                    string expectedFilePath = GetOutputFilePath("ExpectedOutputs", expectedOutputResourceName);
                    string actualFilePath = GetOutputFilePath("ActualOutputs", expectedOutputResourceName);

                    string expectedRootDirectory = Path.GetDirectoryName(expectedFilePath);
                    string actualRootDirectory = Path.GetDirectoryName(actualFilePath);

                    Directory.CreateDirectory(expectedRootDirectory);
                    Directory.CreateDirectory(actualRootDirectory);

                    File.WriteAllText(expectedFilePath, expectedSarifText);
                    File.WriteAllText(actualFilePath, actualSarifText);

                    sb.AppendLine("To compare all difference for this test suite:");
                    sb.AppendLine(GenerateDiffCommand(TypeUnderTest, Path.GetDirectoryName(expectedFilePath), Path.GetDirectoryName(actualFilePath)) + Environment.NewLine);

                    if (RebaselineExpectedResults)
                    {
                        string intermediateFolder = !string.IsNullOrEmpty(IntermediateTestFolder) ? IntermediateTestFolder + @"\" : String.Empty;
                        string testDirectory = Path.Combine(GetProductTestDataDirectory(TestBinaryName, intermediateFolder + TypeUnderTest), "ExpectedOutputs");
                        Directory.CreateDirectory(testDirectory);

                        // We retrieve all test strings from embedded resources. To rebaseline, we need to
                        // compute the enlistment location from which these resources are compiled.

                        expectedFilePath = Path.Combine(testDirectory, expectedOutputResourceName);
                        File.WriteAllText(expectedFilePath, actualSarifText);
                    }
                }

                if (!RebaselineExpectedResults)
                {
                    ValidateResults(sb.ToString());
                }
            }

            RebaselineExpectedResults.Should().BeFalse();
        }

        protected string GetOutputFilePath(string directory, string resourceName)
        {
            string fileName = Path.GetFileNameWithoutExtension(resourceName) + ".sarif";
            return Path.Combine(OutputFolderPath, directory, fileName);
        }

        protected string GetResourceText(string resourceName)
        {
            return _resourceExtractor.GetResourceText(resourceName, TestLogResourceNameRoot);
        }

        protected byte[] GetResourceBytes(string resourceName)
        {
            return _resourceExtractor.GetResourceBytes(resourceName, TestLogResourceNameRoot);
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

        public static bool AreEquivalent<T>(string actualSarif, string expectedSarif, IContractResolver contractResolver = null)
        {
            if (actualSarif.Equals(expectedSarif)) { return true; }

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };

            expectedSarif = expectedSarif ?? "{}";

            // Make sure we can successfully roundtrip what was just generated
            T actualSarifObject = JsonConvert.DeserializeObject<T>(actualSarif, settings);
            actualSarif = JsonConvert.SerializeObject(actualSarifObject, settings);

            JToken generatedToken = JToken.Parse(actualSarif);
            JToken expectedToken = JToken.Parse(expectedSarif);

            return JToken.DeepEquals(generatedToken, expectedToken);
        }
    }
}
