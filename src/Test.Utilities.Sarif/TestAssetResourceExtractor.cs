// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    ///  ResourceExtractor provides simple helpers for extracting resource stream content for tests.
    /// </summary>
    public class TestAssetResourceExtractor
    {
        private readonly Type testClass;
        private readonly Assembly testAssembly;
        private readonly string testAssetDirectory;

        public Assembly TestAssembly => testAssembly ?? testClass.Assembly;

        public string TestBinaryNameWithoutExtension =>
            Path.GetFileNameWithoutExtension(TestAssembly.Location);

        public string DefaultResourceNamespaceRoot => @$"{TestBinaryNameWithoutExtension}.TestData.{this.testAssetDirectory}";

        public string DefaultResourceInputsPath => @$"{DefaultResourceNamespaceRoot}.Inputs";

        public string DefaultResourceExpectedOutputsPath => @$"{DefaultResourceNamespaceRoot}.ExpectedOutputs";

        /// <summary>
        /// This constructor is used to retrieve the default location for test assets associated with 
        /// the provided test class. The path for these assets exists at the root of the test binary
        /// source code directory, in a directory named 'TestData'. The name of the type under test
        /// is inferred from the test class name and added to the path. 
        /// 
        /// E.g., a test class named RoundTrippingTests in Test.UnitTests.Sarif would store its test
        /// assets in a directory named:
        /// 
        ///     c:\src\sarif-sdk\src\Test.UnitTests.Sarif\TestData\RoundTripping\
        /// </summary>
        /// <param name="testClass"></param>
        public TestAssetResourceExtractor(Type testClass)
        {
            this.testClass = testClass;
            this.testAssetDirectory = testClass.Name.Substring(0, testClass.Name.Length - "Tests".Length);
        }

        public TestAssetResourceExtractor(Assembly testAssembly, string testAssetDirectory)
        {
            this.testAssembly = testAssembly;
            this.testAssetDirectory = testAssetDirectory;
        }

        public string GetResourceInputText(string resourcePath)
        {
            return GetResourceText($"{DefaultResourceInputsPath}.{resourcePath}");
        }

        public string GetResourceExpectedOutputsText(string resourcePath)
        {
            return GetResourceText($"{DefaultResourceExpectedOutputsPath}.{resourcePath}");
        }

        public string GetResourceText(string resourcePath)
        {
            using Stream stream = GetManifestResourceStream(resourcePath, out string fallbackResourcePath);

            ValidateStream(resourcePath, fallbackResourcePath, stream);

            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public byte[] GetResourceBytes(string resourcePath)
        {
            using Stream stream = GetManifestResourceStream(resourcePath, out string fallbackResourcePath);

            ValidateStream(resourcePath, fallbackResourcePath, stream);

            if (stream == null)
            {
                return GetBytesFromStream(stream);
            }

            return GetBytesFromStream(stream);
        }

        private Stream GetManifestResourceStream(string resourcePath, out string fallbackResourcePath)
        {
            fallbackResourcePath = null;

            // First look to see whether we've been provided a fully-qualified resource path.
            Stream stream = TestAssembly.GetManifestResourceStream(resourcePath);

            bool shortPath = !resourcePath.StartsWith(TestBinaryNameWithoutExtension);

            if (shortPath && stream == null)
            {
                // If this succeeds, we were provided a short resource name which 
                // needs to be appended to the default resource namespace root in 
                // order to construct the fully-qualified name.
                fallbackResourcePath = $"{DefaultResourceNamespaceRoot}.{resourcePath}";
                stream = TestAssembly.GetManifestResourceStream(fallbackResourcePath);
            }

            return stream;
        }

        private byte[] GetBytesFromStream(Stream stream)
        {
            using MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        private void ValidateStream(string resourcePath, string fallbackResourcePath, Stream stream)
        {
            if (stream != null) { return; }

            string[] resourceNames = TestAssembly.GetManifestResourceNames();
            Array.Sort(resourceNames);

            string fallbackMessage = fallbackResourcePath == null ? null :
                $"Also considered:{Environment.NewLine}{fallbackResourcePath}{Environment.NewLine}{Environment.NewLine}";

            // TODO: we should convert the resource names here to file paths and see if we can find them.
            // If we can, then the user has just neglected to embed the resource in the assembly, and we
            // could warn on this.

            // This code produces a message such as what follows. The report includes the original argument passed to
            // GetResourceText and also shows the fallback path, if one was considered, built from the namespace info.
            //
            //------------------------------------------------------------------------------------------------------
            //  System.ArgumentException : Could not find:
            //  Broken.csv
            //
            //  Also considered:
            //  Test.UnitTests.Sarif.Converters.TestData.HdfConverter.Broken.json
            //
            //  Valid Names:
            //  Test.UnitTests.Sarif.Converters.TestData.HdfConverter.ValidResults.json
            //  Test.UnitTests.Sarif.Converters.TestData.HdfConverter.InvalidResults.json
            //
            throw new ArgumentException(
                $"Could not find:{Environment.NewLine}{resourcePath}{Environment.NewLine}{Environment.NewLine}{fallbackMessage}" +
                $"Valid Names:{Environment.NewLine}{string.Join($"{Environment.NewLine}", resourceNames)}");
        }
    }
}
