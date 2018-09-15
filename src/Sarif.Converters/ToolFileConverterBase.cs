// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
            LogicalLocationsDictionary = new Dictionary<string, LogicalLocation>();
        }

        public abstract void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert);

        // internal as well as protected so it can be exercised by unit tests.
        protected internal IDictionary<string, LogicalLocation> LogicalLocationsDictionary { get; private set;  }

        protected internal string AddLogicalLocation(LogicalLocation logicalLocation)
        {
            return AddLogicalLocation(logicalLocation, delimiter: ".");
        }

        // internal as well as protected it can be exercised by unit tests.
        protected internal string AddLogicalLocation(LogicalLocation logicalLocation, string delimiter)
        {
            if (logicalLocation == null)
            {
                throw new ArgumentNullException(nameof(logicalLocation));
            }

            int disambiguator = 0;

            string fullyQualifiedLogicalName = logicalLocation.ParentKey == null ?
                logicalLocation.Name : 
                logicalLocation.ParentKey + delimiter + logicalLocation.Name;
            string generatedKey = fullyQualifiedLogicalName;

            logicalLocation.Name = GetLogicalLocationName(logicalLocation.ParentKey, fullyQualifiedLogicalName, delimiter);
            logicalLocation.FullyQualifiedName = fullyQualifiedLogicalName;

            while (LogicalLocationsDictionary.ContainsKey(generatedKey))
            {
                LogicalLocation logLoc = LogicalLocationsDictionary[generatedKey].DeepClone();
                logLoc.Name = GetLogicalLocationName(logLoc.ParentKey, fullyQualifiedLogicalName, delimiter);
                logLoc.FullyQualifiedName = fullyQualifiedLogicalName;

                if (logicalLocation.ValueEquals(logLoc))
                {
                    break;
                }

                generatedKey = fullyQualifiedLogicalName + "-" + disambiguator.ToString(CultureInfo.InvariantCulture);
                ++disambiguator;
            }

            if (disambiguator == 0)
            {
                logicalLocation.FullyQualifiedName = null;
            }

            if (logicalLocation.Name == generatedKey)
            {
                logicalLocation.Name = null;
            }

            if (!LogicalLocationsDictionary.ContainsKey(generatedKey))
            {
                LogicalLocationsDictionary.Add(generatedKey, logicalLocation);
            }

            return generatedKey;
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
