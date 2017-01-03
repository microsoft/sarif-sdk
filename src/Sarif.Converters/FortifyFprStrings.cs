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
        public readonly string BuildId;

        /// <summary>The string constant "SourceFiles"</summary>
        public readonly string SourceFiles;

        // SourceFiles members

        /// <summary>The string constant "File"</summary>
        public readonly string File;

        /// <summary>The string constant "size"</summary>
        public readonly string SizeAttribute;

        /// <summary>The string constant "type"</summary>
        public readonly string TypeAttribute;

        // File members

        /// <summary>The string constant "Name"</summary>
        public readonly string Name;

        // Vulnerabilities element

        /// <summary>The string constant "Vulnerabilities"</summary>
        public readonly string Vulnerabilities;

        // Vulnerabilities members

        /// <summary>The string constant "Vulnerability"</summary>
        public readonly string Vulnerability;

        // Vulnerability members

        /// <summary>The string constant "ClassID"</summary>
        public readonly string ClassId;

        // Description element

        /// <summary>The string constant "Description"</summary>
        public readonly string Description;

        // Description members

        /// <summary>The string constant "classID"</summary>
        public readonly string ClassIdAttribute;

        /// <summary>The string constant "Abstract"</summary>
        public readonly string Abstract;

        // CommandLine element

        /// <summary>The string constant "CommandLine".</summary>
        public readonly string CommandLine;

        // CommandLine members

        /// <summary>The string constant "Argument".</summary>
        public readonly string Argument;

        // Errors element

        /// <summary>The string constant "Errors".</summary>
        public readonly string Errors;

        // Errors members

        /// <summary>The string constant "Error".</summary>
        public readonly string Error;

        // Error members

        /// <summary>The string constant "code".</summary>
        public readonly string CodeAttribute;

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
            Build = nameTable.Add("Build");
            BuildId = nameTable.Add("BuildID");
            SourceFiles = nameTable.Add("SourceFiles");
            File = nameTable.Add("File");
            SizeAttribute = nameTable.Add("size");
            TypeAttribute = nameTable.Add("type");
            Name = nameTable.Add("Name");
            Vulnerabilities = nameTable.Add("Vulnerabilities");
            Vulnerability = nameTable.Add("Vulnerability");
            ClassId = nameTable.Add("ClassID");
            Description = nameTable.Add("Description");
            ClassIdAttribute = nameTable.Add("classID");
            Abstract = nameTable.Add("Abstract");
            CommandLine = nameTable.Add("CommandLine");
            Argument = nameTable.Add("Argument");
            Errors = nameTable.Add("Errors");
            Error = nameTable.Add("Error");
            CodeAttribute = nameTable.Add("code");
            MachineInfo = nameTable.Add("MachineInfo");
            Hostname = nameTable.Add("Hostname");
            Username = nameTable.Add("Username");
        }
    }
}
