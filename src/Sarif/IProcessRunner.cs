// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs the specified executable with the specified arguments and returns the contents of
        /// the stdout stream.
        /// </summary>
        string Run(
            string workingDirectory,
            string exePath,
            string arguments);
    }
}
