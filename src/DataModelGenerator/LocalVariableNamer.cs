// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A local variable name generation facility.</summary>
    internal class LocalVariableNamer
    {
        private readonly string _prefix;
        private int _index;

        public LocalVariableNamer(string prefix)
        {
            _prefix = prefix;
            _index = 0;
        }

        /// <summary>Makes the next local variable name.</summary>
        /// <returns>A string of the local variable name.</returns>
        public string MakeName()
        {
            return _prefix + "_" + _index++.ToString(CultureInfo.InvariantCulture);
        }
    }
}
