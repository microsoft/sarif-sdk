using System;
using System.IO;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    /// <summary>
    ///  ResourceExtractor provides simple helpers for extracting resource stream content for tests.
    /// </summary>
    public class ResourceExtractor
    {
        public const string TestLogResourceNameRoot = "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.TestData";
        private Assembly ClassAssembly { get; set; }

        public ResourceExtractor(Type forType)
        {
            ClassAssembly = forType.Assembly;
        }

        private string GetResourcePath(string resourceName, string root = null)
        {
            return $"{root ?? TestLogResourceNameRoot}.{resourceName}";
        }

        public string GetResourceText(string resourceName, string root = null)
        {
            string resourcePath = GetResourcePath(resourceName, root);
            string text = null;

            using (Stream stream = ClassAssembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null) { return string.Empty; }

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
