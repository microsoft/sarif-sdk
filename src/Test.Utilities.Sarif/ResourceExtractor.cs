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
        private Assembly ClassAssembly { get; }

        public ResourceExtractor(Type forType)
        {
            ClassAssembly = forType.Assembly;
        }

        private string GetResourcePath(string resourceName, string root = null)
        {
            string[] resourceNames = ClassAssembly.GetManifestResourceNames();
            foreach (string name in resourceNames)
            {
                if (name == resourceName) { return name; }

                // TODO: we should consider zapping this and requiring a completely formed resource name.
                if (name.EndsWith($"{root ?? ""}.{resourceName}", StringComparison.OrdinalIgnoreCase)) { return name; }
            }

            throw new ArgumentException($"Could not find {resourceName}. Valid Names:\r\n{string.Join("\r\n", resourceNames)}");
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
