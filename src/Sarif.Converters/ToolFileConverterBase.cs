// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Writers;

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

        public abstract void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions);

        // internal as well as protected it can be exercised by unit tests.
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

            string fullyQualifiedLogicalName = logicalLocation.ParentKey == null ? logicalLocation.Name : logicalLocation.ParentKey + delimiter + logicalLocation.Name;
            string generatedKey = fullyQualifiedLogicalName;

            while (LogicalLocationsDictionary.ContainsKey(generatedKey) && !logicalLocation.ValueEquals(LogicalLocationsDictionary[generatedKey]))
            {
                generatedKey = fullyQualifiedLogicalName + "-" + disambiguator.ToString(CultureInfo.InvariantCulture);
                ++disambiguator;
            }

            if (!LogicalLocationsDictionary.ContainsKey(generatedKey))
            {
                string logicalName = null;
                int index;

                if (logicalLocation.ParentKey == null)
                {
                    index = !string.IsNullOrWhiteSpace(delimiter) ? fullyQualifiedLogicalName.LastIndexOf(delimiter) : -1; ;
                }
                else
                {
                    index = logicalLocation.ParentKey.Length;
                }

                if (index == -1)
                {
                    if (generatedKey != fullyQualifiedLogicalName)
                    {
                        // It's a top-level location and FQLN differs from the key
                        logicalName = fullyQualifiedLogicalName;
                    }
                }
                else
                {
                    // Get the rightmost segment as the name
                    // Example: Foo::Bar -> Bar where '::' is the delimiter
                    int length = delimiter?.Length ?? 0;
                    logicalName = fullyQualifiedLogicalName.Substring(index + length);
                }

                logicalLocation.Name = logicalName;

                if (disambiguator > 0)
                {
                    logicalLocation.FullyQualifiedName = fullyQualifiedLogicalName;
                }

                LogicalLocationsDictionary.Add(generatedKey, logicalLocation);
            }

            return generatedKey;
        }
    }
}
