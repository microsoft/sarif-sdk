// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Base class for tool file converters. Encapsulates the common logic
    /// for populating the logicalLocations dictionary.
    /// </summary>
    public abstract class ToolFileConverterBase
    {
        protected ToolFileConverterBase()
        {
            _logicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
            LogicalLocations = new List<LogicalLocation>();
        }

        public abstract void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert);

        // internal as well as protected it can be exercised by unit tests.
        private IDictionary<LogicalLocation, int> _logicalLocationToIndexMap;

        protected internal IList<LogicalLocation> LogicalLocations { get; private set; }

        protected internal int AddLogicalLocation(LogicalLocation logicalLocation)
        {
            if (LogicalLocations.Count == 0)
            {
                // Someone has reset the converter in order to reuse it. 
                // So we'll clear our index map as well
                _logicalLocationToIndexMap.Clear();
            }

            int index;
            if (!_logicalLocationToIndexMap.TryGetValue(logicalLocation, out index))
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
    }
}
