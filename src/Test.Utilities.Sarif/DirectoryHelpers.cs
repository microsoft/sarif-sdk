// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    public static class DirectoryHelpers
    {
        /// <summary>
        /// Retrieves the repo root that contains this file (which may be a submodule), e.g.:
        ///     d:\src\sarif-sdk\
        /// </summary>
        /// <returns></returns>
        public static string GetEnlistmentRoot()
        {
            string path = typeof(DirectoryHelpers).Assembly.Location;
            return GitHelper.Default.GetTopLevel(path);
        }

        /// <summary>
        /// Retrieves the source code directory (named '\src\') at the root of the repo.
        ///     d:\src\sarif-sdk\src\
        /// </summary>
        /// <returns></returns>
        public static string GetEnlistmentSrcDirectory()
        {
            string path = typeof(DirectoryHelpers).Assembly.Location;
            path = GitHelper.Default.GetTopLevel(path);
            return Path.Combine(GetEnlistmentRoot(), @"src\");
        }

        public static string GetFileNameWithoutExtension(this Assembly assembly)
        {
            return (assembly == null) 
                ? null
                : Path.GetFileNameWithoutExtension(assembly.Location);
        }

        /// <summary>
        /// Retrieves a directory for either test binary-specific or shared test assets, e.g.:
        ///     d:\src\sarif-sdk\src\Test.UnitTests.Sarif\TestData
        ///     d:\src\sarif-sdk\src\TestData (shared between test binaries)
        /// </summary>
        /// <returns></returns>
        public static string GetTestDataDirectory(Assembly testAssembly = null)
        {
            string srcDirectory = DirectoryHelpers.GetEnlistmentSrcDirectory();
            string binaryDirectory = testAssembly?.GetFileNameWithoutExtension();

            // Path.GetFullPath will elide duplicate backslashes if present.
            return Path.GetFullPath(@$"{srcDirectory}\{binaryDirectory}\TestData\");
        }
    }
}
