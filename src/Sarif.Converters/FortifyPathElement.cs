// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>A Fortify path element structure, which serves as a location identifier. This type is
    /// typically used to represent result locations, datapath sources, and datapath sinks.</summary>
    internal class FortifyPathElement
    {
        /// <summary>Full pathname of the file.</summary>
        public readonly string FilePath;
        /// <summary>The line start index.</summary>
        public readonly int LineStart;
        /// <summary>Target function name; may be null.</summary>
        public readonly string TargetFunction;

        /// <summary>Initializes a new instance of the <see cref="FortifyPathElement" /> class.</summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the
        /// required range.</exception>
        /// <param name="filePath">Full pathname of the file.</param>
        /// <param name="lineStart">The line start index.</param>
        /// <param name="targetFunction">Target function name; may be null.</param>
        public FortifyPathElement(string filePath, int lineStart, string targetFunction)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (lineStart <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineStart), lineStart, ConverterResources.FortifyBadLineNumber);
            }

            this.FilePath = filePath;
            this.LineStart = lineStart;
            this.TargetFunction = targetFunction;
        }

        /// <summary>Parses an element as a Fortify PathElement node, consuming the node.</summary>
        /// <param name="xmlReader">The <see cref="XmlReader"/> from which a node shall be parsed. When
        /// this function returns, this reader is placed directly after the element on which it is
        /// currently placed.</param>
        /// <param name="strings">Strings used in processing Fortify logs.</param>
        /// <returns>A <see cref="FortifyPathElement"/> parsed from the element on which
        /// <paramref name="xmlReader"/> is positioned when this method is called.</returns>
        public static FortifyPathElement Parse(XmlReader xmlReader, FortifyStrings strings)
        {
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

            if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.IsEmptyElement)
            {
                throw xmlReader.CreateException(ConverterResources.FortifyNotValidPathElement);
            }

            xmlReader.Read(); // Always true because !IsEmptyElement
            xmlReader.IgnoreElement(strings.FileName, IgnoreOptions.Required);
            string filePath = xmlReader.ReadElementContentAsString(strings.FilePath, string.Empty);
            int lineStart = xmlReader.ReadElementContentAsInt(strings.LineStart, string.Empty);
            xmlReader.IgnoreElement(strings.Snippet, IgnoreOptions.Optional);
            xmlReader.IgnoreElement(strings.SnippetLine, IgnoreOptions.Optional);
            string targetFunction = xmlReader.ReadOptionalElementContentAsString(strings.TargetFunction);
            xmlReader.ReadEndElement();

            try
            {
                return new FortifyPathElement(filePath, lineStart, targetFunction);
            }
            catch (ArgumentException ex)
            {
                throw xmlReader.CreateException(ex.Message);
            }
        }
    }
}
