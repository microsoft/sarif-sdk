// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>An implementation of <see cref="IResultLogWriter"/> which merely stores its results in a list.</summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.Sarif.IResultLogWriter"/>
    public sealed class ResultLogObjectWriter : IResultLogWriter
    {
        private Tool _tool;
        private ImmutableList<Result> _resultList;
        private ImmutableDictionary<Uri, IList<FileData>> _fileDictionary;

        /// <summary>Initializes a new instance of the <see cref="ResultLogObjectWriter"/> class.</summary>
        public ResultLogObjectWriter()
        {
            _resultList = ImmutableList<Result>.Empty;
        }

        /// <summary>Gets the Tool block.</summary>
        /// <value>The <see cref="Tool"/> block if it has been written; otherwise, null.</value>
        public Tool Tool { get { return _tool; } }

        /// <summary>Gets the list of issues written so far.</summary>
        /// <value>The list of <see cref="Result"/> objects written so far.</value>
        public ImmutableList<Result> ResultList { get { return _resultList; } }

        public void Initialize() { }

        /// <summary>Writes a tool information entry to the log.</summary>
        /// <exception cref="InvalidOperationException">Thrown if the tool info block has already been
        /// written.</exception>
        /// <param name="tool">The tool information to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteTool(Tool)"/>
        public void WriteTool(Tool tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            if (_tool != null)
            {
                throw new InvalidOperationException(SarifResources.ToolAlreadyWritten);
            }

            _tool = tool;
        }

        /// <summary>Writes a run information entry to the log.</summary>
        public void WriteRun(Run run) { }

        /// <summary>
        /// Write information about scanned files to the log. This information may appear
        /// after the results, as the full list of scanned files might not be known until
        /// all results have been generated.
        /// </summary>
        /// <param name="fileDictionary">
        /// A dictionary whose keys are the URIs of scanned files and whose values provide
        /// information about those files.
        /// </param>
        public void WriteFiles(Dictionary<Uri, IList<FileData>> fileDictionary)
        {
            if (fileDictionary == null)
            {
                throw new ArgumentNullException(nameof(fileDictionary));
            }

            _fileDictionary = ImmutableDictionary.CreateRange(fileDictionary);
        }

        public void OpenResults() { }

        public void CloseResults() { }

        /// <summary>
        /// Writes a result to the log.
        /// </summary>
        /// <remarks>
        /// This function makes a copy of the data stored in <paramref name="result"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tool info is not yet written.
        /// </exception>
        /// <param name="result">
        /// The result to write.
        /// </param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteIssue(Result)"/>
        public void WriteResult(Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_tool == null)
            {
                throw new InvalidOperationException(SarifResources.CannotWriteResultToolMissing);
            }

            _resultList = _resultList.Add(new Result(result));
        }

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
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.
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
        public void WriteResults(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                WriteResult(result);
            }
        }
    }
}
