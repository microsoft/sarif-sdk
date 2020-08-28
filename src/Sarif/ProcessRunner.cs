// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ProcessRunner : IProcessRunner
    {
        /// <summary>
        /// Runs the specified executable with the specified arguments and returns the contents of
        /// the stdout stream.
        /// </summary>
        public string Run(string workingDirectory, string fileName, string arguments)
            => new ExternalProcess(workingDirectory, fileName, arguments).StdOut.Text;
    }
}
