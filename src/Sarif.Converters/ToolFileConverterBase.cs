// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Base class for tool file converters. Encapsulates the common logic
    /// for populating the logicalLocations array.
    /// </summary>
    public abstract class ToolFileConverterBase
    {
        protected ToolFileConverterBase()
        {
            _logicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
            LogicalLocations = new List<LogicalLocation>();
        }

        public abstract string ToolName { get; }

        public abstract void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert);

        // internal as well as protected so it can be exercised by unit tests.
        private readonly IDictionary<LogicalLocation, int> _logicalLocationToIndexMap;

        protected internal IList<LogicalLocation> LogicalLocations { get; }

        protected internal int AddLogicalLocation(LogicalLocation logicalLocation)
        {
            if (LogicalLocations.Count == 0)
            {
                // Someone has reset the converter in order to reuse it. 
                // So we'll clear our index map as well
                _logicalLocationToIndexMap.Clear();
            }

            if (!_logicalLocationToIndexMap.TryGetValue(logicalLocation, out int index))
            {
                index = _logicalLocationToIndexMap.Count;
                _logicalLocationToIndexMap[logicalLocation] = index;
                LogicalLocations.Add(logicalLocation);
            }
            return index;
        }

        internal static string GetLogicalLocationName(string parentKey, string fullyQualifiedLogicalName, string delimiter)
        {
            string logicalName = null;
            int index;

            if (parentKey == null)
            {
                index = !string.IsNullOrWhiteSpace(delimiter) ? fullyQualifiedLogicalName.LastIndexOf(delimiter) : -1;
            }
            else
            {
                index = parentKey.Length;
            }

            if (index == -1)
            {
                // It's a top-level location
                logicalName = fullyQualifiedLogicalName;
            }
            else
            {
                // Get the rightmost segment as the name
                // Example: Foo::Bar -> Bar where '::' is the delimiter
                int length = delimiter?.Length ?? 0;
                logicalName = fullyQualifiedLogicalName.Substring(index + length);
            }

            return logicalName;
        }

        protected Run PersistResults(IResultLogWriter output, IList<Result> results)
        {
            var run = new Run()
            {
                Tool = new Tool { Driver = new ToolComponent { Name = ToolName } },
                ColumnKind = ColumnKind.Utf16CodeUnits
            };

            return PersistResults(output, results, run);
        }

        protected static Run PersistResults(IResultLogWriter output, IList<Result> results, Run run)
        {
            output.Initialize(run);

            run.Results = results;
            var visitor = new AddFileReferencesVisitor();
            visitor.VisitRun(run);

            if (run.Results != null)
            {
                output.OpenResults();
                output.WriteResults(run.Results);
                output.CloseResults();
            }

            return run;
        }
    }
}
