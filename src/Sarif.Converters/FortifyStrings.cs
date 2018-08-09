// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    //<xs:element name="Result">
    //    <xs:complexType>
    //        <xs:sequence>
    //            <!-- Result Description -->
    //            <xs:element name="Category" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //            <xs:element name="Folder" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //            <xs:element name="Kingdom" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //            <xs:element name="Abstract" type="xs:string" minOccurs="0" maxOccurs="1"/>
    //            <xs:element name="AbstractCustom" type="xs:string" minOccurs="0" maxOccurs="1"/>
    //            <xs:element name="Friority" type="xs:string" minOccurs="0" maxOccurs="1"/>
    //            <!-- custom tags including Analysis -->
    //            <xs:element name="Tag" minOccurs="0" maxOccurs="unbounded">
    //                <xs:complexType>
    //                    <xs:sequence>
    //                        <xs:element name="Name" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //                        <xs:element name="Value" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //                    </xs:sequence>
    //                </xs:complexType>
    //            </xs:element>
    //            <xs:element name="Comment" minOccurs="0" maxOccurs="unbounded">
    //                <xs:complexType>
    //                    <xs:sequence>
    //                        <xs:element name="UserInfo" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //                        <xs:element name="Comment" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //                    </xs:sequence>
    //                </xs:complexType>
    //            </xs:element>
    //            <!-- primary or sink -->
    //            <xs:element name="Primary" type="PathElement" minOccurs="1" maxOccurs="1"/>
    //            <!-- source -->
    //            <xs:element name="Source" type="PathElement" minOccurs="0" maxOccurs="1"/>
    //            <xs:element name="TraceDiagramPath" type="xs:string" minOccurs="0" maxOccurs="1"/>
    //            <!-- optional external category (i.e. STIG) -->
    //            <xs:element name="ExternalCategory" minOccurs="0" maxOccurs="1">
    //                <xs:complexType>
    //                    <xs:simpleContent>
    //                        <xs:extension base="xs:string">
    //                            <xs:attribute name="type" type="xs:string" use="required"/>
    //                        </xs:extension>
    //                    </xs:simpleContent>
    //                </xs:complexType>
    //            </xs:element>
    //        </xs:sequence>
    //        <xs:attribute name="iid" type="xs:string" use="optional"/>
    //        <xs:attribute name="ruleID" type="xs:string" use="optional"/>
    //    </xs:complexType>
    //</xs:element>
    //<xs:complexType name="PathElement">
    //    <xs:sequence>
    //        <xs:element name="FileName" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //        <xs:element name="FilePath" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //        <xs:element name="LineStart" type="xs:string" minOccurs="1" maxOccurs="1"/>
    //        <xs:element name="Snippet" type="xs:string" minOccurs="0" maxOccurs="1"/>
    //        <xs:element name="SnippetLine" type="xs:int" minOccurs="0" maxOccurs="1"/>
    //        <xs:element name="TargetFunction" type="xs:string" minOccurs="0" maxOccurs="1"/>
    //    </xs:sequence>
    //</xs:complexType>

    /// <summary>Strings from the Fortify XSD used for parsing Fortify logs.</summary>
    internal class FortifyStrings
    {
        // ReportSection element

        /// <summary>The string constant "ReportSection".</summary>
        public readonly string ReportSection;

        /// <summary>The string constant "Title".</summary>
        public readonly string Title;

        /// <summary>The string constant "SubSection".</summary>
        public readonly string SubSection;

        /// <summary>The string constant "Description".</summary>
        public readonly string Description;

        /// <summary>The string constant "Text".</summary>
        public readonly string Text;

        // Result Element

        /// <summary>The string constant "Issue".</summary>
        public readonly string Issue;
        /// <summary>The string constant "iid".</summary>
        public readonly string Iid;
        /// <summary>The string constant "ruleID".</summary>
        public readonly string RuleId;

        // Result Members

        /// <summary>The string constant "Category".</summary>
        public readonly string Category;
        /// <summary>The string constant "Folder".</summary>
        public readonly string Folder;
        /// <summary>The string constant "Kingdom".</summary>
        public readonly string Kingdom;
        /// <summary>The string constant "Abstract".</summary>
        public readonly string Abstract;
        /// <summary>The string constant "AbstractCustom".</summary>
        public readonly string AbstractCustom;
        /// <summary>The string constant "Friority" [sic].</summary>
        public readonly string Friority;
        /// <summary>The string constant "Tag".</summary>
        public readonly string Tag; // Not currently parsed
        /// <summary>The string constant "Comment".</summary>
        public readonly string Comment; // Not currently parsed
        /// <summary>The string constant "Primary".</summary>
        public readonly string Primary;
        /// <summary>The string constant "Source".</summary>
        public readonly string Source;
        /// <summary>The string constant "TraceDiagramPath".</summary>
        public readonly string TraceDiagramPath;
        /// <summary>The string constant "ExternalCategory".</summary>
        public readonly string ExternalCategory;
        /// <summary>The string constant "type".</summary>
        public readonly string Type;

        // PathElement Members

        /// <summary>The string constant "FileName".</summary>
        public readonly string FileName;
        /// <summary>The string constant "FilePath".</summary>
        public readonly string FilePath;
        /// <summary>The string constant "LineStart".</summary>
        public readonly string LineStart;
        /// <summary>The string constant "Snippet".</summary>
        public readonly string Snippet;
        /// <summary>The string constant "SnippetLine".</summary>
        public readonly string SnippetLine;
        /// <summary>The string constant "TargetFunction".</summary>
        public readonly string TargetFunction;

        /// <summary>Initializes a new instance of the <see cref="FortifyStrings"/> class.</summary>
        /// <param name="nameTable">The name table from which strings shall be retrieved.</param>
        public FortifyStrings(XmlNameTable nameTable)
        {
            this.ReportSection = nameTable.Add("ReportSection");
            this.Title = nameTable.Add("Title");
            this.SubSection = nameTable.Add("SubSection");
            this.Description = nameTable.Add("Description");
            this.Text = nameTable.Add("Text");
            this.Issue = nameTable.Add("Issue");
            this.Iid = nameTable.Add("iid");
            this.RuleId = nameTable.Add("ruleID");
            this.Category = nameTable.Add("Category");
            this.Folder = nameTable.Add("Folder");
            this.Kingdom = nameTable.Add("Kingdom");
            this.Abstract = nameTable.Add("Abstract");
            this.AbstractCustom = nameTable.Add("AbstractCustom");
            this.Friority = nameTable.Add("Friority");
            this.Tag = nameTable.Add("Tag");
            this.Comment = nameTable.Add("Comment");
            this.Primary = nameTable.Add("Primary");
            this.Source = nameTable.Add("Source");
            this.TraceDiagramPath = nameTable.Add("TraceDiagramPath");
            this.ExternalCategory = nameTable.Add("ExternalCategory");
            this.Type = nameTable.Add("type");
            this.FileName = nameTable.Add("FileName");
            this.FilePath = nameTable.Add("FilePath");
            this.LineStart = nameTable.Add("LineStart");
            this.Snippet = nameTable.Add("Snippet");
            this.SnippetLine = nameTable.Add("SnippetLine");
            this.TargetFunction = nameTable.Add("TargetFunction");
        }
    }
}
