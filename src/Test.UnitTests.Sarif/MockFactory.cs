// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Moq;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal static class MockFactory
    {
        public static IFileSystem MakeMockFileSystem(string fileName, string fileText = null, string[] fileLines = null)
        {
            var mock = new Mock<IFileSystem>(MockBehavior.Strict);
            mock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns((string s) => s.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            mock.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns((string path) => path);
            mock.Setup(fs => fs.ReadAllText(fileName)).Returns(fileText ?? string.Join(Environment.NewLine, fileLines));
            mock.Setup(fs => fs.ReadAllLines(fileName)).Returns(fileLines ?? fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            return mock.Object;
        }
    }
}
