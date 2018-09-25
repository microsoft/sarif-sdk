// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    ///  FileSystemStreamProvider is an IStreamProvider which interacts with files
    ///  from the local file system.
    /// </summary>
    public class FileSystemStreamProvider : IStreamProvider
    {
        public void Delete(string filePath)
        {
            File.Delete(filePath);
        }

        public Stream OpenRead(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public Stream OpenWrite(string filePath)
        {
            return new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }
    }
}
