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

        /// <summary>The string constant "SourceBasePath"</summary>
        public readonly string SourceBasePath;

        /// <summary>The string constant "SourceFiles"</summary>
        public readonly string SourceFiles;

        /// <summary>The string constant "File"</summary>
        public readonly string File;

        /// <summary>The string constant "size"</summary>
        public readonly string SizeAttribute;

        /// <summary>The string constant "type"</summary>
        public readonly string TypeAttribute;

        /// <summary>The string constant "encoding"</summary>
        public readonly string EncodingAttribute;

        /// <summary>The string constant "Name"</summary>
        public readonly string Name;

        /// <summary>The string constant "Vulnerabilities"</summary>
        public readonly string Vulnerabilities;

        /// <summary>The string constant "Vulnerability"</summary>
        public readonly string Vulnerability;

        /// <summary>The string constant "ClassID"</summary>
        public readonly string ClassId;

        /// <summary>The string constant "Kingdom"</summary>
        public readonly string Kingdom;

        /// <summary>The string constant "Type"</summary>
        public readonly string Type;

        /// <summary>The string constant "Subtype"</summary>
        public readonly string Subtype;

        /// <summary>The string constant "DefaultSeverity"</summary>
        public readonly string DefaultSeverity;

        /// <summary>The string constant "InstanceSeverity"</summary>
        public readonly string InstanceSeverity;

        /// <summary>The string constant "Confidence"</summary>
        public readonly string Confidence;

        /// <summary>The string constant "Level"</summary>
        public readonly string Level;

        /// <summary>The string constant "AnalysisInfo"</summary>
        public readonly string AnalysisInfo;

        // Members and further nested members of AnalysisInfo

        /// <summary>The string constant "ReplacementDefinitions"</summary>
        public readonly string ReplacementDefinitions;

        /// <summary>The string constant "Def"</summary>
        public readonly string Def;

        /// <summary>The string constant "key"</summary>
        public readonly string KeyAttribute;

        /// <summary>The string constant "value"</summary>
        public readonly string ValueAttribute;

        /// <summary>The string constant "Unified"</summary>
        public readonly string Unified;

        /// <summary>The string constant "Trace"</summary>
        public readonly string Trace;

        /// <summary>The string constant "Entry"</summary>
        public readonly string Entry;

        /// <summary>The string constant "NodeRef"</summary>
        public readonly string NodeRef;

        /// <summary>The string constant "isDefault"</summary>
        public readonly string IsDefaultAttribute;

        /// <summary>The string constant "label"</summary>
        public readonly string LabelAttribute;

        /// <summary>The string constant "SourceLocation"</summary>
        public readonly string SourceLocation;

        /// <summary>The string constant "snippet"</summary>
        public readonly string SnippetAttribute;

        /// <summary>The string constant "path"</summary>
        public readonly string PathAttribute;

        /// <summary>The string constant "line"</summary>
        public readonly string LineAttribute;

        /// <summary>The string constant "lineEnd"</summary>
        public readonly string LineEndAttribute;

        /// <summary>The string constant "colStart"</summary>
        public readonly string ColStartAttribute;

        /// <summary>The string constant "colEnd"</summary>
        public readonly string ColEndAttribute;

        /// <summary>The string constant "Description"</summary>
        public readonly string Description;

        /// <summary>The string constant "CustomDescription"</summary>
        public readonly string CustomDescription;

        /// <summary>The string constant "classID"</summary>
        public readonly string ClassIdAttribute;

        /// <summary>The string constant "Abstract"</summary>
        public readonly string Abstract;

        /// <summary>The string constant "Explanation"</summary>
        public readonly string Explanation;

        /// <summary>The string constant "UnifiedNodePool"</summary>
        public readonly string UnifiedNodePool;

        /// <summary>The string constant "Node"</summary>
        public readonly string Node;

        /// <summary>The string constant "Action"</summary>
        public readonly string Action;

        /// <summary>The string constant "Snippets"</summary>
        public readonly string Snippets;

        /// <summary>The string constant "Snippet"</summary>
        public readonly string Snippet;

        /// <summary>The string constant "id"</summary>
        public readonly string IdAttribute;

        /// <summary>The string constant "StartLine"</summary>
        public readonly string StartLine;

        /// <summary>The string constant "EndLine"</summary>
        public readonly string EndLine;

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

        /// <summary>The string constant "FilterResult".</summary>
        public readonly string FilterResult;

        /// <summary>The string constant "RuleInfo".</summary>
        public readonly string RuleInfo;

        /// <summary>The string constant "Rule".</summary>
        public readonly string Rule;

        /// <summary>The string constant "MetaInfo".</summary>
        public readonly string MetaInfo;

        /// <summary>The string constant "Group".</summary>
        public readonly string Group;

        /// <summary>The string constant "NameAttribute".</summary>
        public readonly string NameAttribute;

        /// <summary>The string constant "Hostname".</summary>
        public readonly string Hostname;

        /// <summary>The string constant "Username".</summary>
        public readonly string Username;

        /// <summary>The string constant "Platform".</summary>
        public readonly string Platform;

        /// <summary>The string constant "EngineVersion".</summary>
        public readonly string EngineVersion;

        /// <summary>The string constant "InstanceID".</summary>
        public readonly string InstanceID;

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
            SourceBasePath = nameTable.Add("SourceBasePath");
            SourceFiles = nameTable.Add("SourceFiles");
            File = nameTable.Add("File");
            SizeAttribute = nameTable.Add("size");
            TypeAttribute = nameTable.Add("type");
            EncodingAttribute = nameTable.Add("encoding");
            Name = nameTable.Add("Name");
            Vulnerabilities = nameTable.Add("Vulnerabilities");
            Vulnerability = nameTable.Add("Vulnerability");
            ClassId = nameTable.Add("ClassID");
            Kingdom = nameTable.Add("Kingdom");
            Type = nameTable.Add("Type");
            Subtype = nameTable.Add("Subtype");
            DefaultSeverity = nameTable.Add("DefaultSeverity");
            InstanceSeverity = nameTable.Add("InstanceSeverity");
            Confidence = nameTable.Add("Confidence");
            Level = nameTable.Add("Level");
            AnalysisInfo = nameTable.Add("AnalysisInfo");
            ReplacementDefinitions = nameTable.Add("ReplacementDefinitions");
            Def = nameTable.Add("Def");
            KeyAttribute = nameTable.Add("key");
            ValueAttribute = nameTable.Add("value");
            Unified = nameTable.Add("Unified");
            Trace = nameTable.Add("Trace");
            Entry = nameTable.Add("Entry");
            NodeRef = nameTable.Add("NodeRef");
            IsDefaultAttribute = nameTable.Add("isDefault");
            LabelAttribute = nameTable.Add("label");
            SourceLocation = nameTable.Add("SourceLocation");
            SnippetAttribute = nameTable.Add("snippet");
            PathAttribute = nameTable.Add("path");
            LineAttribute = nameTable.Add("line");
            LineEndAttribute = nameTable.Add("lineEnd");
            ColStartAttribute = nameTable.Add("colStart");
            ColEndAttribute = nameTable.Add("colEnd");
            Description = nameTable.Add("Description");
            CustomDescription = nameTable.Add("CustomDescription");
            ClassIdAttribute = nameTable.Add("classID");
            Abstract = nameTable.Add("Abstract");
            Explanation = nameTable.Add("Explanation");
            UnifiedNodePool = nameTable.Add("UnifiedNodePool");
            Node = nameTable.Add("Node");
            Action = nameTable.Add("Action");
            Snippets = nameTable.Add("Snippets");
            Snippet = nameTable.Add("Snippet");
            IdAttribute = nameTable.Add("id");
            StartLine = nameTable.Add("StartLine");
            EndLine = nameTable.Add("EndLine");
            Text = nameTable.Add("Text");
            CommandLine = nameTable.Add("CommandLine");
            Argument = nameTable.Add("Argument");
            Errors = nameTable.Add("Errors");
            Error = nameTable.Add("Error");
            CodeAttribute = nameTable.Add("code");
            MachineInfo = nameTable.Add("MachineInfo");
            FilterResult = nameTable.Add("FilterResult");
            RuleInfo = nameTable.Add("RuleInfo");
            Rule = nameTable.Add("Rule");
            MetaInfo = nameTable.Add("MetaInfo");
            Group = nameTable.Add("Group");
            NameAttribute = nameTable.Add("name");
            Hostname = nameTable.Add("Hostname");
            Username = nameTable.Add("Username");
            Platform = nameTable.Add("Platform");
            EngineVersion = nameTable.Add("EngineVersion");
            InstanceID = nameTable.Add("InstanceID");
        }
    }
}
