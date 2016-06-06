// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Base class for tool file converters. Encapsulates the common logic
    /// for populating the logicalLocations dictionary.
    /// </summary>
    public abstract class ToolFileConverterBase : IToolFileConverter
    {
        protected ToolFileConverterBase()
        {
            LogicalLocationsDictionary = new Dictionary<string, LogicalLocation>();
        }

        public abstract void Convert(Stream input, IResultLogWriter output);

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

            string logicalLocationKey = logicalLocation.ParentKey == null ? logicalLocation.Name : logicalLocation.ParentKey + delimiter + logicalLocation.Name;
            string generatedKey = logicalLocationKey;

            while (LogicalLocationsDictionary.ContainsKey(generatedKey) && !logicalLocation.ValueEquals(LogicalLocationsDictionary[generatedKey]))
            {
                generatedKey = logicalLocationKey + "-" + disambiguator.ToString(CultureInfo.InvariantCulture);
                ++disambiguator;
            }

            if (!LogicalLocationsDictionary.ContainsKey(generatedKey))
            {
                LogicalLocationsDictionary.Add(generatedKey, logicalLocation);
            }

            return generatedKey;
        }
    }
}
