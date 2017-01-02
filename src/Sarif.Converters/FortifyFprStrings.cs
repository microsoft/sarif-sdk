// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprStrings
    {
        // CommandLine element

        /// <summary>The string constant "CommandLine".</summary>
        public readonly string CommandLine;

        // CommandLine members

        /// <summary>The string constant "Argument".</summary>
        public readonly string Argument;

        /// <summary>
        /// Initializes a new instance of the <see cref="FortifyFprStrings"/> class.
        /// </summary>
        /// <param name="nameTable">
        /// The name table from which strings shall be retrieved.
        /// </param>
        public FortifyFprStrings(XmlNameTable nameTable)
        {
            CommandLine = nameTable.Add("CommandLine");
            Argument = nameTable.Add("Argument");
        }
    }
}
