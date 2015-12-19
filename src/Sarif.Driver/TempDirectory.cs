// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// Temporary directory class; implements <see cref="IDisposable"/> to ensure an effort is made to
    /// delete the temporary directory in the face of exceptions. If you only need a single file, use the
    /// <see cref="TempFile"/> class instead.
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    /// <seealso cref="TempFile"/>
    public sealed class TempDirectory : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="TempDirectory"/> class.</summary>
        public TempDirectory()
        {
            this.Name = TempFile.CreateTempName();
            Directory.CreateDirectory(this.Name);
        }

        /// <summary>Gets the name of the generated directory.</summary>
        /// <value>The name of the generated directory.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the full path to a file with the supplied name inside this temp directory.
        /// </summary>
        /// <param name="fileName">File name of the file path to generate.</param>
        /// <returns>
        /// The current <see cref="Name"/> combined with <paramref name="fileName"/>.
        /// </returns>
        public string Combine(string fileName)
        {
            return Path.Combine(this.Name, fileName);
        }

        /// <summary>Touches the given file in the temp directory.</summary>
        /// <param name="fileName">File name of the file path to generate.</param>
        /// <returns>The full path to the file created or updated.</returns>
        public string Touch(string fileName)
        {
            string filePath = this.Combine(fileName);
            new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read | FileShare.Write | FileShare.Delete).Dispose();
            return filePath;
        }

        /// <summary>Writes text to the given file in the temp directory.</summary>
        /// <param name="fileName">File name of the file path to generate.</param>
        /// <param name="text">The text to write.</param>
        /// <returns>The full path to the file created or updated.</returns>
        public string Write(string fileName, string text)
        {
            string filePath = this.Combine(fileName);
            File.WriteAllText(filePath, text, Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources. Makes a best-effort attempt to delete the temporary directory.
        /// </summary>
        /// <seealso cref="M:System.IDisposable.Dispose()"/>
        public void Dispose()
        {
            this.DisposeImpl();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources. Makes a best-effort attempt to delete the temporary directory.
        /// </summary>
        /// <seealso cref="M:System.Object.Finalize()"/>
        ~TempDirectory()
        {
            this.DisposeImpl();
        }

        private void DisposeImpl()
        {
            try
            {
                Directory.Delete(this.Name, true);
            }
            catch (DirectoryNotFoundException)
            {
                // Not found; doesn't matter because this delete is best-effort.
            }
            catch (IOException)
            {
                // Lock violation; doesn't matter because this delete is best-effort.
            }
            catch (UnauthorizedAccessException)
            {
                // Access denied; doesn't matter because this delete is best-effort.
            }
        }
    }
}
