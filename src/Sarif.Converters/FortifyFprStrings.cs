// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprStrings
    {
        /// <summary>The string constant "CreatedTS"</summary>
        public readonly string CreatedTimestamp;

        /// <summary>The string constant "date"</summary>
        public readonly string DateAttribute;

        /// <summary>The string constant "time"</summary>
        public readonly string TimeAttribute;

        /// <summary>The string constant "UUID"</summary>
        public readonly string Uuid;

        /// <summary>The string constant "Build".</summary>
        public readonly string Build;

        /// <summary>The string constant "BuildID"</summary>
        public readonly string BuildId;

        /// <summary>The string constant "SourceFiles"</summary>
        public readonly string SourceFiles;

        /// <summary>The string constant "File"</summary>
        public readonly string File;

        /// <summary>The string constant "size"</summary>
        public readonly string SizeAttribute;

        /// <summary>The string constant "type"</summary>
        public readonly string TypeAttribute;

        /// <summary>The string constant "Name"</summary>
        public readonly string Name;

        /// <summary>The string constant "Vulnerabilities"</summary>
        public readonly string Vulnerabilities;

        /// <summary>The string constant "Vulnerability"</summary>
        public readonly string Vulnerability;

        /// <summary>The string constant "ClassID"</summary>
        public readonly string ClassId;

        /// <summary>The string constant "AnalysisInfo"</summary>
        public readonly string AnalysisInfo;

        // Members and further nested members of AnalysisInfo

        /// <summary>The string constant "SourceLocation"</summary>
        public readonly string SourceLocation;

        /// <summary>The string constant "snippet"</summary>
        public readonly string SnippetAttribute;

        /// <summary>The string constant "path"</summary>
        public readonly string PathAttribute;

        /// <summary>The string constant "line"</summary>
        public readonly string LineAttribute;

        /// <summary>The string constant "colStart"</summary>
        public readonly string ColStartAttribute;

        /// <summary>The string constant "colEnd"</summary>
        public readonly string ColEndAttribute;

        /// <summary>The string constant "Description"</summary>
        public readonly string Description;

        /// <summary>The string constant "classID"</summary>
        public readonly string ClassIdAttribute;

        /// <summary>The string constant "Abstract"</summary>
        public readonly string Abstract;

        /// <summary>The string constant "Explanation"</summary>
        public readonly string Explanation;

        /// <summary>The string constant "Snippets"</summary>
        public readonly string Snippets;

        /// <summary>The string constant "Snippet"</summary>
        public readonly string Snippet;

        /// <summary>The string constant "id"</summary>
        public readonly string IdAttribute;

        /// <summary>The string constant "Text"</summary>
        public readonly string Text;

        /// <summary>The string constant "CommandLine".</summary>
        public readonly string CommandLine;

        /// <summary>The string constant "Argument".</summary>
        public readonly string Argument;

        /// <summary>The string constant "Errors".</summary>
        public readonly string Errors;

        /// <summary>The string constant "Error".</summary>
        public readonly string Error;

        /// <summary>The string constant "code".</summary>
        public readonly string CodeAttribute;

        /// <summary>The string constant "MachineInfo".</summary>
        public readonly string MachineInfo;

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
            CreatedTimestamp = nameTable.Add("CreatedTS");
            DateAttribute = nameTable.Add("date");
            TimeAttribute = nameTable.Add("time");
            Uuid = nameTable.Add("UUID");
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
            AnalysisInfo = nameTable.Add("AnalysisInfo");
            SourceLocation = nameTable.Add("SourceLocation");
            SnippetAttribute = nameTable.Add("snippet");
            PathAttribute = nameTable.Add("path");
            LineAttribute = nameTable.Add("line");
            ColStartAttribute = nameTable.Add("colStart");
            ColEndAttribute = nameTable.Add("colEnd");
            Description = nameTable.Add("Description");
            ClassIdAttribute = nameTable.Add("classID");
            Abstract = nameTable.Add("Abstract");
            Explanation = nameTable.Add("Explanation");
            Snippets = nameTable.Add("Snippets");
            Snippet = nameTable.Add("Snippet");
            IdAttribute = nameTable.Add("id");
            Text = nameTable.Add("Text");
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
