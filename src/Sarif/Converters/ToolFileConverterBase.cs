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
            LogicalLocationsDictionary = new Dictionary<string, IList<LogicalLocationComponent>>();
        }

        public abstract void Convert(Stream input, IResultLogWriter output);

        // internal as well as protected it can be exercised by unit tests.
        protected internal IDictionary<string, IList<LogicalLocationComponent>> LogicalLocationsDictionary { get; private set;  }

        // internal as well as protected it can be exercised by unit tests.
        protected internal void AddLogicalLocation(Location location, IList<LogicalLocationComponent> logicalLocationComponents)
        {
            int disambiguator = 0;
            string logicalLocationKey = location.FullyQualifiedLogicalName;
            while (LogicalLocationsDictionary.ContainsKey(logicalLocationKey) && !logicalLocationComponents.SequenceEqual(LogicalLocationsDictionary[logicalLocationKey], LogicalLocationComponent.ValueComparer))
            {
                logicalLocationKey = location.FullyQualifiedLogicalName + "-" + disambiguator.ToString(CultureInfo.InvariantCulture);
                ++disambiguator;
            }

            if (!logicalLocationKey.Equals(location.FullyQualifiedLogicalName, StringComparison.Ordinal))
            {
                location.LogicalLocationKey = logicalLocationKey;
            }

            if (!LogicalLocationsDictionary.ContainsKey(logicalLocationKey))
            {
                LogicalLocationsDictionary.Add(logicalLocationKey, logicalLocationComponents.ToArray());
            }
        }
    }
}
