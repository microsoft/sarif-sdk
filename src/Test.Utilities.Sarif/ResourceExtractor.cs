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
    public class ResourceExtractor
    {
        private readonly Type testClass;
        private readonly string typeUnderTestName;

        public Assembly ResourceAssembly => testClass.Assembly;

        public string DefaultResourceNamespaceRoot => @$"{this.testClass.Namespace}.TestData.{this.typeUnderTestName}";

        public ResourceExtractor(Type type)
        {
            this.testClass = type;
            this.typeUnderTestName = type.Name.Substring(0, type.Name.Length - "Tests".Length);
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
            Stream stream = ResourceAssembly.GetManifestResourceStream(resourcePath);

            if (stream == null)
            {
                // If this succeeds, we were provided a short resource name which 
                // needs to be appended to the default resource namespace root in 
                // order to construct the fully-qualified name.
                fallbackResourcePath = $"{DefaultResourceNamespaceRoot}.{resourcePath}";
                stream = ResourceAssembly.GetManifestResourceStream(fallbackResourcePath);
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

            string[] resourceNames = ResourceAssembly.GetManifestResourceNames();
            Array.Sort(resourceNames);

            string fallbackMessage = fallbackResourcePath == null ? null :
                $"Also considered:{Environment.NewLine}{fallbackResourcePath}{Environment.NewLine}{Environment.NewLine}";

            // This code produces a message such as what follows. The report includes the original argument passed to
            // GetResourceText and also shows the fallback path, if one was considered, built from the namespace info.
            //
            //------------------------------------------------------------------------------------------------------
            //  System.ArgumentException : Could not find:
            //  Inputs.InvalidResult.csv
            //
            //  Also considered:
            //  Microsoft.CodeAnalysis.Sarif.Converters.TestData.FlawFinderConverter.Inputs.InvalidResult.csv
            //
            //  Valid Names:
            //  Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Converters.TestData.ClangTidy.ExpectedOutputs.NoResults.sarif
            //  Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Converters.TestData.ClangTidy.ExpectedOutputs.ValidResults.sarif
            //
            throw new ArgumentException(
                $"Could not find:{Environment.NewLine}{resourcePath}{Environment.NewLine}{Environment.NewLine}{fallbackMessage}" +                
                $"Valid Names:{Environment.NewLine}{string.Join($"{Environment.NewLine}", resourceNames)}");
        }
    }
}
