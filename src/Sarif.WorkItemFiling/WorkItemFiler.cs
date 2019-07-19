// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class WorkItemFiler
    {
        private readonly IFileSystem _fileSystem;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="fileSystem">
        /// An object that implements the <see cref="IFileSystem"/> interface, providing
        /// access to the file system;
        /// </param>
        public WorkItemFiler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Files work items from the results in a SARIF log file.
        /// </summary>
        /// <param name="path">
        /// The path to the SARIF log file.
        /// </param>
        public void FileWorkItems(string path)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }
        }
    }
}
