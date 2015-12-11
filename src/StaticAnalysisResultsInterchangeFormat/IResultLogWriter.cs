// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.DataContracts;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>This interface serves as a sink for <see cref="ResultLog"/> format issues.</summary>
    public interface IResultLogWriter
    {
        /// <summary>Writes run and tool information entries to the log. These must be the first
        /// entries written into a log, and they may be written at most once.</summary>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info block has already been
        /// written.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> is null.</exception>
        /// <param name="toolInfo">The tool information to write.</param>
        /// <param name="runInfo">The run information to write.</param>
        void WriteToolAndRunInfo(ToolInfo toolInfo, RunInfo runInfo);

        /// <summary>Writes a result to the log. The log must have tool info written first by calling
        /// <see cref="M:WriteToolInfo" />.</summary>
        /// <remarks>This function makes a copy of the data stored in <paramref name="result"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)</remarks>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info is not yet written.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        ///  <param name="result">The result to write.</param>
        void WriteResult(Result result);
    }
}
