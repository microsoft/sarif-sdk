// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        private Assembly ClassAssembly { get; }

        public ResourceExtractor(Type forType)
        {
            ClassAssembly = forType.Assembly;
        }

        private string GetResourcePath(string resourceName, string root = null)
        {
            string nameToFind = $"{root ?? ""}.{resourceName}";

            string[] resourceNames = ClassAssembly.GetManifestResourceNames();
            foreach (string name in resourceNames)
            {
                if (name.EndsWith(nameToFind, StringComparison.OrdinalIgnoreCase)) { return name; }
            }

            throw new ArgumentException($"Could not find {nameToFind}. Valid Names:\r\n{string.Join("\r\n", resourceNames)}");
        }

        public string GetResourceText(string resourceName, string root = null)
        {
            string resourcePath = GetResourcePath(resourceName, root);
            string text = null;

            using (Stream stream = ClassAssembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null) { return null; }

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            return text;
        }

        public byte[] GetResourceBytes(string resourceName, string root = null)
        {
            string resourcePath = GetResourcePath(resourceName, root);
            byte[] bytes = null;

            using (Stream stream = ClassAssembly.GetManifestResourceStream(resourcePath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }
            return bytes;
        }
    }
}
