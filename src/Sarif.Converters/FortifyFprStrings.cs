// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprStrings
    {
        // Build element

        /// <summary>The string constant "Build".</summary>
        public readonly string Build;

        // Build members

        /// <summary>The string constant "BuildID"</summary>
        public readonly string BuildID;

        // CommandLine element

        /// <summary>The string constant "CommandLine".</summary>
        public readonly string CommandLine;

        // CommandLine members

        /// <summary>The string constant "Argument".</summary>
        public readonly string Argument;

        // MachineInfo element

        /// <summary>The string constant "MachineInfo".</summary>
        public readonly string MachineInfo;

        // MachineInfo members

        /// <summary>The string constant "Hostname".</summary>
        public readonly string Hostname;

        /// <summary>The string constant "Username".</summary>
        public readonly string Username;

        /// <summary>
        /// Initializes a new instance of the <see cref="FortifyFprStrings"/> class.
        /// </summary>
        /// <param name="nameTable">
        /// The name table from which strings shall be retrieved.
        /// </param>
        public FortifyFprStrings(XmlNameTable nameTable)
        {
            Build = nameTable.Add(nameof(Build));
            BuildID = nameTable.Add(nameof(BuildID));
            CommandLine = nameTable.Add(nameof(CommandLine));
            Argument = nameTable.Add(nameof(Argument));
            MachineInfo = nameTable.Add(nameof(MachineInfo));
            Hostname = nameTable.Add(nameof(Hostname));
            Username = nameTable.Add(nameof(Username));
        }
    }
}
