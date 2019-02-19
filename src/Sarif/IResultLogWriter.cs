// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>This interface serves as a sink for <see cref="SarifLog"/> format issues.</summary>
    public interface IResultLogWriter
    {
        /// <summary>
        /// Initialize the current output log. This method persists all run properties
        /// except for those that may be populated during the course of persisting 
        /// results. A result might produce a new file object to be stored in run.files,
        /// for example, so run.Files will not be persisted on initialization.
        /// </summary>
        /// <param name="id">A string that uniquely identifies a run.</param>
        /// <param name="automationId">A global identifier for a run that permits correlation with a larger automation process.</param> 
        void Initialize(Run run);

        /// <summary>
        /// Write information about scanned files to the log. This information may appear
        /// after the results, as the full list of scanned files might not be known until
        /// all results have been generated.
        /// </summary>
        /// <param name="fileDictionary">
        /// A dictionary whose keys are the strings representing the locations of scanned files
        /// and whose values provide information about those files.
        /// </param>
        void WriteArtifacts(IList<Artifact> fileDictionary);

        /// <summary>
        /// Write information about the logical locations where results were produced to
        /// the log. This information may appear after the results, as the full list of
        /// logical locations will not be known until all results have been generated.
        /// </summary>
        /// <param name="logicalLocations">
        /// An array whose elements contain information about the components of the
        /// logical locations where results were found.
        /// </param>
        void WriteLogicalLocations(IList<LogicalLocation> logicalLocations);

        /// <summary>
        /// Write information about the tool to the log. This information may appear
        /// after the results, as some tool data, such as the relevant set of rule
        /// observed during analysis, might not be known until all results have been
        /// generated. A SARIF file may also contain only message metadata.
        /// </summary>
        /// <param name="tool">
        /// The tool that was executed in the current run.
        /// </param>
        void WriteTool(Tool tool);

        /// <summary>
        /// Initialize the results array associated with the current output log. SARIF producers that
        /// are explicitly generating results (as opposed to other SARIF scenarios such as publishing
        /// rules metadata) should proactively call this method in order to ensure that an explicit 
        /// (but empty) results array exists in the log when no literal results were produced.
        /// </summary>
        void OpenResults();

        /// <summary>
        /// Writes a result to the log.
        /// </summary>
        /// <remarks>
        /// This function makes a copy of the data stored in <paramref name="result"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)
        /// </remarks>
        /// <exception cref="IOException">
        /// A file IO error occured. Clients implementing
        /// <see cref="ToolFileConverterBase"/> should allow these exceptions to propagate.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tool info is not yet written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is null.
        /// </exception>
        ///  <param name="result">
        ///  The result to write.
        ///  </param>
        void WriteResult(Result result);

        /// <summary>
        /// Close out the results array
        /// </summary>
        void CloseResults();

        /// <summary>
        /// Writes a set of results to the log.
        /// </summary>
        /// <remarks>
        /// This function makes a copy of the data stored in <paramref name="results"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)
        /// </remarks>
        /// <exception cref="IOException">
        /// A file IO error occured. Clients implementing
        /// <see cref="ToolFileConverterBase"/> should allow these exceptions to propagate.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tool info is not yet written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is null.
        /// </exception>
        ///  <param name="results">
        ///  The results to write.
        ///  </param>
        void WriteResults(IEnumerable<Result> results);

        /// <summary>
        /// Write a set of invocations to the log.
        /// </summary>
        /// <param name="invocations">
        /// The invocations to write.
        /// </param>
        void WriteInvocations(IEnumerable<Invocation> invocations);
    }
}
