// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers
{
    /// <summary>An implementation of <see cref="IResultLogWriter"/> which merely stores its results in a list.</summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.IResultLogWriter"/>
    public sealed class ResultLogObjectWriter : IResultLogWriter
    {
        private ToolInfo _toolInfo;
        private RunInfo _runInfo;
        private ImmutableList<Result> _issueList;

        /// <summary>Initializes a new instance of the <see cref="ResultLogObjectWriter"/> class.</summary>
        public ResultLogObjectWriter()
        {
            _issueList = ImmutableList<Result>.Empty;
        }

        /// <summary>Gets the ToolInfo block.</summary>
        /// <value>The <see cref="ToolInfo"/> block if it has been written; otherwise, null.</value>
        public ToolInfo ToolInfo { get { return _toolInfo; } }

        /// <summary>Gets the RuleInfo block.</summary>
        /// <value>The <see cref="RunInfo"/> block if it has been written; otherwise, null.</value>
        public RunInfo RunInfo { get { return _runInfo; } }

        /// <summary>Gets the list of issues written so far.</summary>
        /// <value>The list of <see cref="Result"/> objects written so far.</value>
        public ImmutableList<Result> IssueList { get { return _issueList; } }

        /// <summary>Writes a tool information entry to the log. This must be the first entry written into
        /// a log, and it may be written at most once.</summary>
        /// <exception cref="InvalidOperationException">Thrown if the tool info block has already been
        /// written.</exception>
        /// <param name="toolInfo">The tool information to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.IsarifWriter.WriteToolInfo(ToolInfo)"/>
        public void WriteToolAndRunInfo(ToolInfo toolInfo, RunInfo runInfo)
        {
            if (toolInfo == null)
            {
                throw new ArgumentNullException(nameof(toolInfo));
            }

            if (_toolInfo != null)
            {
                throw new InvalidOperationException(SarifResources.ToolInfoAlreadyWritten);
            }

            _toolInfo = toolInfo;
            _runInfo = runInfo;
        }

        /// <summary>Writes a result to the log. The log must have tool info written first by calling
        /// <see cref="M:WriteToolInfo" />.</summary>
        /// <remarks>This function makes a copy of the data stored in <paramref name="result"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the tool info is not yet written.</exception>
        /// <param name="result">The result to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.IsarifWriter.WriteIssue(Result)"/>
        public void WriteResult(Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (_toolInfo == null)
            {
                throw new InvalidOperationException(SarifResources.CannotWriteResultToolInfoMissing);
            }

            _issueList = _issueList.Add(new Result(result));
        }
    }
}
