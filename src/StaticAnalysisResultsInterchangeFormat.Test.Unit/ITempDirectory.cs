// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat
{
    /// <summary>Interface for temporary directories.</summary>
    /// <seealso cref="T:IDisposable"/>
    public interface ITempDirectory
    {
        /// <summary>Gets the name of the generated directory.</summary>
        /// <value>The name of the generated directory.</value>
        string Name
        {
            get;
        }
    }

    /// <summary>Extension methods for <see cref="ITempDirectory"/>.</summary>
    public static class ITempDirectoryExtensions
    {
        /// <summary>Gets the full path to a file with the supplied name inside this temp directory.</summary>
        /// <param name="tempDirectory">The <see cref="ITempDirectory"/> instance to act on.</param>
        /// <param name="fileName">File name of the file path to generate.</param>
        /// <returns>The current <see cref="ITempDirectory.Name"/> combined with
        /// <paramref name="fileName"/>.</returns>
        public static string Combine(this ITempDirectory tempDirectory, string fileName)
        {
            if (tempDirectory == null)
            {
                throw new ArgumentNullException("tempDirectory");
            }

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            return Path.Combine(tempDirectory.Name, fileName);
        }

        /// <summary>Touches the given file in the temp directory.</summary>
        /// <param name="tempDirectory">The <see cref="ITempDirectory"/> instance to act on.</param>
        /// <param name="fileName">File name of the file path to generate.</param>
        /// <returns>The full path to the file created or updated.</returns>
        public static string Touch(this ITempDirectory tempDirectory, string fileName)
        {
            if (tempDirectory == null)
            {
                throw new ArgumentNullException("tempDirectory");
            }

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            string filePath = tempDirectory.Combine(fileName);
            EnsureDirectory(filePath);
            new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read | FileShare.Write | FileShare.Delete).Dispose();
            return filePath;
        }

        /// <summary>Writes text to the given file in the temp directory.</summary>
        /// <param name="tempDirectory">The <see cref="ITempDirectory"/> instance to act on.</param>
        /// <param name="fileName">File name of the file path to generate.</param>
        /// <param name="text">The text to write.</param>
        /// <returns>The full path to the file created or updated.</returns>
        public static string Write(this ITempDirectory tempDirectory, string fileName, string text)
        {
            if (tempDirectory == null)
            {
                throw new ArgumentNullException("tempDirectory");
            }

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            string filePath = tempDirectory.Combine(fileName);
            EnsureDirectory(filePath);
            File.WriteAllText(filePath, text, Encoding.UTF8);
            return filePath;
        }

        private static void EnsureDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!String.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
