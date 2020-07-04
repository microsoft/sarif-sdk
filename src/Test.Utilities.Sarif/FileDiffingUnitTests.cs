// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
        public static string GetProductDirectory()
        {
            return Path.GetFullPath($@"..\..\..\..\..\src\");
        }

        public static string GetProductTestDataDirectory(string testBinaryName, string subdirectory = "")
        {
            return Path.GetFullPath(Path.Combine(GetProductDirectory(), $".\\{testBinaryName}\\TestData", subdirectory));
        }

        private readonly ITestOutputHelper _outputHelper;
        private readonly bool _testProducesSarifCurrentVersion;
        private readonly ResourceExtractor _resourceExtractor;

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

        protected virtual string OutputFolderPath => Path.Combine(GetTestDirectory(), "..", "UnitTestOutput." + TypeUnderTest);

        protected virtual string ProductTestDataDirectory => GetProductTestDataDirectory(TypeUnderTest);

        protected virtual string IntermediateTestFolder { get { return string.Empty; } }

        protected virtual string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.TestData." + TypeUnderTest;

        // Most tests convert a single input file to a single output file, but some have multiple
        // inputs and/or multiple outputs. Derived test classes must implement one or the other of
        // the methods ConstructTestOutputFromInputResource (which handles the one-to-one case) and
        // ConstructTestOutputsFromInputResources (which handles the many-to-many case). These
        // methods can't (or at least, shouldn't) both be abstract because derived test classes
        // shouldn't have to supply even an empty implementation of the one that they don't use.
        // Therefore this class implements both of those methods to throw NotImplementedException
        // so you don't accidentally call the wrong one.
        protected virtual string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
            => throw new NotImplementedException(nameof(ConstructTestOutputFromInputResource));

        protected virtual IDictionary<string, string> ConstructTestOutputsFromInputResources(IEnumerable<string> inputResourceNames, object parameter)
            => throw new NotImplementedException(nameof(ConstructTestOutputsFromInputResources));

        protected virtual void RunTest(string inputResourceName, string expectedOutputResourceName = null, object parameter = null)
        {
            // In the simple case of one input file and one output file, the output resource name
            // can be inferred from the input resource name. In the case of arbitrary numbers of
            // input and output files (the other overload of RunTest), the output resource names
            // must be specified explicitly.
            expectedOutputResourceName = expectedOutputResourceName ?? inputResourceName;

            // When we retrieve test input content, we use the resource name as the test specified it.
            // But when we construct the names of the actual and expected output files, we ensure that
            // the name has the ".sarif" extension. This is necessary for testing classes such as the
            // Fortify converter whose input is not SARIF. In the other overload of RunTest, this is
            // not necessary because, again, the output resource names are specified explicitly.
            expectedOutputResourceName = Path.GetFileNameWithoutExtension(expectedOutputResourceName) + SarifConstants.SarifFileExtension;
            string expectedSarifText = GetExpectedSarifTextFromResource(expectedOutputResourceName);

            string actualSarifText = ConstructTestOutputFromInputResource(ConstructFullInputResourceName(inputResourceName), parameter);

            // The comparison code is shared between this one-input-to-one-output method and the
            // overload that takes multiple inputs and multiple outputs. So set up the lists and
            // dictionaries that the shared comparison method expects.
            var inputResourceNames = new List<string> { inputResourceName };

            const string SingleOutputDictionaryKey = "single";

            var expectedOutputResourceNameDictionary = new Dictionary<string, string>
            {
                [SingleOutputDictionaryKey] = expectedOutputResourceName
            };

            var expectedSarifTexts = new Dictionary<string, string>
            {
                [SingleOutputDictionaryKey] = expectedSarifText
            };

            var actualSarifTexts = new Dictionary<string, string>
            {
                [SingleOutputDictionaryKey] = actualSarifText
            };

            CompareActualToExpected(inputResourceNames, expectedOutputResourceNameDictionary, expectedSarifTexts, actualSarifTexts);
        }

        protected virtual void RunTest(IList<string> inputResourceNames, IDictionary<string, string> expectedOutputResourceNames, object parameter = null)
        {
            var expectedSarifTexts = expectedOutputResourceNames.ToDictionary(
                pair => pair.Key,
                pair => GetExpectedSarifTextFromResource(pair.Value));

            IEnumerable<string> fullInputResourceNames = inputResourceNames.Select(resName => ConstructFullInputResourceName(resName));

            IDictionary<string, string> actualSarifTexts = ConstructTestOutputsFromInputResources(fullInputResourceNames, parameter);

            CompareActualToExpected(inputResourceNames, expectedOutputResourceNames, expectedSarifTexts, actualSarifTexts);
        }

        private void CompareActualToExpected(
            IList<string> inputResourceNames,
            IDictionary<string, string> expectedOutputResourceNameDictionary,
            IDictionary<string, string> expectedSarifTextDictionary,
            IDictionary<string, string> actualSarifTextDictionary)
        {
            if (inputResourceNames.Count == 0)
            {
                throw new ArgumentException("No input resources were specified", nameof(inputResourceNames));
            }

            if (expectedOutputResourceNameDictionary.Count == 0)
            {
                throw new ArgumentException("No expected output resources were specified", nameof(expectedOutputResourceNameDictionary));
            }

            if (expectedSarifTextDictionary.Count != expectedOutputResourceNameDictionary.Count)
            {
                throw new ArgumentException($"The number of expected output files ({expectedSarifTextDictionary.Count}) does not match the number of expected output resources {expectedOutputResourceNameDictionary.Count}");
            }

            if (expectedSarifTextDictionary.Count != actualSarifTextDictionary.Count)
            {
                throw new ArgumentException($"The number of actual output files ({actualSarifTextDictionary.Count}) does not match the number of expected output files {expectedSarifTextDictionary.Count}");
            }

            bool passed = true;
            if (RebaselineExpectedResults)
            {
                passed = false;
            }
            else
            {
                // Reify the list of keys because we're going to modify the dictionary in place.
                List<string> keys = expectedSarifTextDictionary.Keys.ToList();

                foreach (string key in keys)
                {
                    if (_testProducesSarifCurrentVersion)
                    {
                        PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(expectedSarifTextDictionary[key], Formatting.Indented, out string transformedSarifText);
                        expectedSarifTextDictionary[key] = transformedSarifText;

                        passed &= AreEquivalent<SarifLog>(actualSarifTextDictionary[key], expectedSarifTextDictionary[key]);
                    }
                    else
                    {
                        passed &= AreEquivalent<SarifLogVersionOne>(actualSarifTextDictionary[key], expectedSarifTextDictionary[key], SarifContractResolverVersionOne.Instance);
                    }
                }
            }

            if (!passed)
            {
                string errorMessage = string.Format(@"there should be no unexpected diffs detected comparing actual results to '{0}'.", string.Join(", ", inputResourceNames));
                var sb = new StringBuilder(errorMessage);

                if (!Utilities.RunningInAppVeyor)
                {
                    string expectedRootDirectory = null;
                    string actualRootDirectory = null;

                    bool firstKey = true;
                    foreach (string key in expectedOutputResourceNameDictionary.Keys)
                    {
                        string expectedFilePath = GetOutputFilePath("ExpectedOutputs", expectedOutputResourceNameDictionary[key]);
                        string actualFilePath = GetOutputFilePath("ActualOutputs", expectedOutputResourceNameDictionary[key]);

                        if (firstKey)
                        {
                            expectedRootDirectory = Path.GetDirectoryName(expectedFilePath);
                            actualRootDirectory = Path.GetDirectoryName(actualFilePath);

                            Directory.CreateDirectory(expectedRootDirectory);
                            Directory.CreateDirectory(actualRootDirectory);

                            firstKey = false;
                        }

                        File.WriteAllText(expectedFilePath, expectedSarifTextDictionary[key]);
                        File.WriteAllText(actualFilePath, actualSarifTextDictionary[key]);
                    }

                    sb.AppendLine("To compare all difference for this test suite:");
                    sb.AppendLine(GenerateDiffCommand(TypeUnderTest, expectedRootDirectory, actualRootDirectory) + Environment.NewLine);

                    if (RebaselineExpectedResults)
                    {
                        string intermediateFolder = !string.IsNullOrEmpty(IntermediateTestFolder) ? IntermediateTestFolder + @"\" : string.Empty;
                        string testDirectory = Path.Combine(GetProductTestDataDirectory(TestBinaryName, intermediateFolder + TypeUnderTest), "ExpectedOutputs");
                        Directory.CreateDirectory(testDirectory);

                        // We retrieve all test strings from embedded resources. To rebaseline, we need to
                        // compute the enlistment location from which these resources are compiled.
                        foreach (string key in expectedOutputResourceNameDictionary.Keys)
                        {
                            string expectedFilePath = Path.Combine(testDirectory, expectedOutputResourceNameDictionary[key]);
                            File.WriteAllText(expectedFilePath, actualSarifTextDictionary[key]);
                        }
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
            string fileName = Path.GetFileNameWithoutExtension(resourceName) + SarifConstants.SarifFileExtension;
            return Path.Combine(OutputFolderPath, directory, fileName);
        }

        protected virtual string GetResourceText(string resourceName)
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

        public static string GenerateDiffCommand(string suiteName, string expected, string actual)
        {
            actual = Path.GetFullPath(actual);
            expected = Path.GetFullPath(expected);

            string diffText = string.Format(CultureInfo.InvariantCulture, "%DIFF% \"{0}\" \"{1}\"", expected, actual);

            string qualifier = string.Empty;

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
            expectedSarif = expectedSarif ?? "{}";
            JToken expectedToken = JsonConvert.DeserializeObject<JToken>(expectedSarif);

            JToken actualToken = JsonConvert.DeserializeObject<JToken>(actualSarif);
            if (!JToken.DeepEquals(actualToken, expectedToken)) { return false; }

            // Make sure we can successfully roundtrip what was just generated.
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };

            T actualSarifObject = JsonConvert.DeserializeObject<T>(actualSarif, settings);
            string roundTrippedSarif = JsonConvert.SerializeObject(actualSarifObject, settings);

            JToken roundTrippedToken = JsonConvert.DeserializeObject<JToken>(roundTrippedSarif);
            if (!JToken.DeepEquals(actualToken, roundTrippedToken)) { return false; }

            return true;
        }

        private string GetExpectedSarifTextFromResource(string resourceName)
            => GetResourceText(ConstructFullExpectedOutputResourceName(resourceName));

        private string ConstructFullExpectedOutputResourceName(string resourceName)
            => "ExpectedOutputs." + resourceName;

        private string ConstructFullInputResourceName(string resourceName)
            => "Inputs." + resourceName;
    }
}
