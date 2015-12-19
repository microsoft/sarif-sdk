// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    /// Temporary file class; implements <see cref="IDisposable"/> to ensure an effort is made to
    /// delete the temporary file in the face of exceptions. If you need a directory or set of
    /// files instead of a file, use the <see cref="TempDirectory"/> class instead.
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    /// <seealso cref="TempDirectory"/>
    public sealed class TempFile : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="TempFile"/> class.</summary>
        public TempFile()
        {
            this.Name = CreateTempName();
        }

        /// <summary>Initializes a new instance of the <see cref="TempFile"/> class where the file name has the indicated extension.</summary>
        /// <param name="requestedExtension">The requested extension.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public TempFile(string requestedExtension)
        {
            string name = CreateTempName();
            if (!requestedExtension.StartsWith(".", StringComparison.Ordinal))
            {
                name += ".";
            }

            this.Name = name + requestedExtension;
        }

        /// <summary>Gets the name of the generated file.</summary>
        /// <value>The name of the generated file.</value>
        public string Name { get; private set; }

        /// <summary>Creates temporary path name.</summary>
        /// <returns>A temporary path name where a file is not present on the file system.</returns>
        public static string CreateTempName()
        {
            // This file won't be present on the file system because GetRandomFileName
            // returns a path name generated from a cryptographically secure random number
            // generator (e.g. CryptGenRandom).
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources. Makes a best-effort attempt to delete the temporary file.
        /// </summary>
        /// <seealso cref="M:System.IDisposable.Dispose()"/>
        public void Dispose()
        {
            this.DisposeImpl();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources. Makes a best-effort attempt to delete the temporary file.
        /// </summary>
        /// <seealso cref="M:System.Object.Finalize()"/>
        ~TempFile()
        {
            this.DisposeImpl();
        }

        private void DisposeImpl()
        {
            try
            {
                File.Delete(this.Name);
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
