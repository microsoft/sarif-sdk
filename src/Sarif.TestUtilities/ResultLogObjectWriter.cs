// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>An implementation of <see cref="IResultLogWriter"/> which merely stores its results in a list.</summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.Sarif.IResultLogWriter"/>
    public sealed class ResultLogObjectWriter : IResultLogWriter
    {
        private Tool _tool;

        /// <summary>Gets the Tool block.</summary>
        /// <value>The <see cref="Tool"/> block if it has been written; otherwise, null.</value>
        public Tool Tool { get { return _tool; } }

        /// <summary>Gets the Run object.</summary>
        public Run Run { get; set;  }

        public void Initialize(Run run)
        {
        }

        public void WriteTool(Tool tool)
        {
            if (_tool != null)
            {
                throw new InvalidOperationException(SdkResources.ToolAlreadyWritten);
            }

            _tool = tool ?? throw new ArgumentNullException(nameof(tool));
        }

        public void WriteInvocations(IEnumerable<Invocation> invocations)
        {
        }

        public void WriteFiles(IList<FileData> fileDictionary)
        {
            throw new NotImplementedException();
        }
        public void WriteLogicalLocations(IList<LogicalLocation> logicalLocations)
        {
            throw new NotImplementedException();
        }

        public void OpenResults() { }

        public void CloseResults() { }

        public void WriteResult(Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_tool == null)
            {
                throw new InvalidOperationException(SdkResources.CannotWriteResultToolMissing);
            }
        }

        public void WriteResults(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                WriteResult(result);
            }
        }

        public void WriteRules(IList<MessageDescriptor> rules)
        {
            throw new NotImplementedException();
        }
    }
}
