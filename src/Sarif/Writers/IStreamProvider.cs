// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    ///  IStreamProvider allows customizing how classes which need to open streams
    ///  get them, so that non-file system streams can be used if desired.
    /// </summary>
    public interface IStreamProvider
    {
        Stream OpenWrite(string filePath);
        Stream OpenRead(string filePath);
        void Delete(string filePath);
    }
}
