// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis.Driver;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat
{
    public static class Extensions
    {
        /// <summary>Retrieves a property value if it exists, or null.</summary>
        /// <param name="properties">A properties object from which the property shall be
        /// retrieved, or null.</param>
        /// <param name="key">The property name / key.</param>
        /// <returns>
        /// If <paramref name="properties"/> is not null and an entry for the supplied key exists, the
        /// value associated with that key; otherwise, null.
        /// </returns>
        internal static string PropertyValue(this Dictionary<string, string> properties, string key)
        {
            string propValue;

            if (properties != null &&
                properties.TryGetValue(key, out propValue))
            {
                return propValue;
            }

            return null;
        }

        /// <summary>Checks if a character is a newline.</summary>
        /// <param name="testedCharacter">The character to check.</param>
        /// <returns>true if newline, false if not.</returns>
        internal static bool IsNewline(char testedCharacter)
        {
            return testedCharacter == '\r'
                || testedCharacter == '\n'
                || testedCharacter == '\u2028'  // Unicode line separator
                || testedCharacter == '\u2029'; // Unicode paragraph separator
        }

        /// <summary>
        /// Returns whether or not the range [<paramref name="startIndex"/>,
        /// <paramref name="startIndex"/> + <paramref name="target"/><c>.Length</c>) is equal to the
        /// supplied string.
        /// </summary>
        /// <param name="array">The array to check.</param>
        /// <param name="startIndex">The start index in the array to check.</param>
        /// <param name="target">Target string to look for in the array.</param>
        /// <returns>
        /// true if the range [<paramref name="startIndex"/>, <paramref name="startIndex"/> +
        /// <paramref name="target"/><c>.Length</c>) is equal to
        /// <paramref name="target"/>. If the range is undefined in the bounds of the array, false.
        /// </returns>
        internal static bool ArrayMatches(char[] array, int startIndex, string target)
        {
            if (startIndex < 0)
            {
                return false;
            }

            int targetLength = target.Length;
            if (targetLength + startIndex >= array.Length)
            {
                return false;
            }

            for (int idx = 0; idx < targetLength; ++idx)
            {
                if (array[idx + startIndex] != target[idx])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates an exception with line number and position data from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="xmlReader">The xmlReader from which line data shall be retrieved.</param>
        /// <param name="message">The message to attach to the exception.</param>
        /// <returns>
        /// The new exception with <paramref name="message"/>, and file and line information from
        /// <paramref name="xmlReader"/>.
        /// </returns>
        internal static XmlException CreateException(this XmlReader xmlReader, string message)
        {
            var positionInfo = xmlReader as IXmlLineInfo;
            if (positionInfo == null || !positionInfo.HasLineInfo())
            {
                return new XmlException(message);
            }
            else
            {
                return new XmlException(message, null, positionInfo.LineNumber, positionInfo.LinePosition);
            }
        }

        /// <summary>Creates an exception with line number and position data from an
        /// <see cref="XmlReader"/>.</summary>
        /// <param name="xmlReader">The xmlReader from which line data shall be retrieved.</param>
        /// <param name="message">The message to attach to the exception.</param>
        /// <param name="args">A variable-length parameters list containing arguments formatted into
        /// <paramref name="message"/>.</param>
        /// <returns>The new exception with <paramref name="message"/>, and file and line information from
        /// <paramref name="xmlReader"/>.</returns>
        internal static XmlException CreateException(this XmlReader xmlReader, string message, params object[] args)
        {
            return xmlReader.CreateException(String.Format(CultureInfo.CurrentCulture, message, args));
        }

        /// <summary>
        /// Consumes content from an XML reader until the end element of the element at endElementDepth
        /// <paramref name="endElementDepth"/>, including the end element.
        /// </summary>
        /// <param name="xmlReader">The <see cref="XmlReader"/> whose contents shall be consumed.</param>
        /// <param name="endElementDepth">The endElementDepth of node to consume.</param>
        internal static void ConsumeElementOfDepth(this XmlReader xmlReader, int endElementDepth)
        {
            int enteringReaderDepth = xmlReader.Depth;

            if (enteringReaderDepth < endElementDepth)
            {
                return;
            }

            if (enteringReaderDepth == endElementDepth)
            {
                // Move to the following element
                xmlReader.Read();
            }

            while (xmlReader.Depth > endElementDepth && xmlReader.Read()) { }

            if (xmlReader.NodeType == XmlNodeType.EndElement)
            {
                // Consume the end element
                xmlReader.Read();
            }
        }

        /// <summary>Asserts that the local name of a given element is the name indicated, and ignores the
        /// element's contents.</summary>
        /// <exception cref="XmlException">Thrown when the XML content pointed to by
        /// <paramref name="xmlReader"/> does not match the indicated <paramref name="elementName"/> and
        /// <paramref name="options"/>.</exception>
        /// <param name="xmlReader">The XML reader from which the element shall be read.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="options">Options deciding what content to skip.</param>
        internal static void IgnoreElement(this XmlReader xmlReader, string elementName, IgnoreOptions options)
        {
            if (!IsOnElement(xmlReader, elementName))
            {
                if (options.HasFlag(IgnoreOptions.Optional))
                {
                    return;
                }
                else
                {
                    throw xmlReader.CreateException(SarifResources.ExpectedElementNamed, elementName);
                }
            }

            xmlReader.Skip();
            if (options.HasFlag(IgnoreOptions.Multiple))
            {
                while (IsOnElement(xmlReader, elementName))
                {
                    xmlReader.Skip();
                }
            }
        }

        // Same as XmlReader.IsStartElement except does not call MoveToContent first.
        private static bool IsOnElement(XmlReader xmlReader, string elementName)
        {
            return xmlReader.NodeType == XmlNodeType.Element && Ref.Equal(xmlReader.LocalName, elementName);
        }

        /// <summary>An XmlReader extension method that reads optional element's content as string.</summary>
        /// <param name="xmlReader">The xmlReader from which line data shall be retrieved.</param>
        /// <param name="elementName">Name of the element expected.</param>
        /// <returns>The optional element content as string if the element is present; otherwise, null.</returns>
        internal static string ReadOptionalElementContentAsString(this XmlReader xmlReader, string elementName)
        {
            if (xmlReader.NodeType == XmlNodeType.Element && Ref.Equal(xmlReader.LocalName, elementName))
            {
                return xmlReader.ReadElementContentAsString();
            }

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="PhysicalLocationComponent"/> given a path.
        /// </summary>
        /// <param name="component">The path for which a <see cref="PhysicalLocationComponent"/> shall be created.</param>
        /// <returns>A <see cref="PhysicalLocationComponent"/> with the URI and Mime-Type members filled out.</returns>
        internal static PhysicalLocationComponent CreatePhysicalLocationComponent(string component)
        {
            return new PhysicalLocationComponent
            {
                Uri = component.CreateUriForJsonSerialization(),
                MimeType = Writers.MimeType.DetermineFromFileExtension(component)
            };
        }

        /// <summary>
        /// Creates a new region with the start line filled out.
        /// </summary>
        /// <param name="startLine">The line to set in the region.</param>
        /// <returns>A <see cref="Region"/> with <see cref="Region.StartLine"/> filled out.</returns>
        internal static Region CreateRegion(int startLine)
        {
            return new Region
            {
                StartLine = startLine
            };
        }

        public static Uri CreateUriForJsonSerialization(this string uriText)
        {
            // Why these highjinks? When Newtonsoft JSON parsing code
            // serializes a URI, it appears to consult the Uri.OriginalString
            // member for file:/// Uris and to serialize the local path for
            // these URIs to JSON, as opposed to emitting an actual file Uri.
            // SARIF specifies that all Uri content must, in fact, comprise a
            // Uri, so we construct a Uri first and then, if necessary, 
            // create a second one using the file:/// representation as
            // the original string passed to the constructor instance.
            Uri result = new Uri(uriText, UriKind.RelativeOrAbsolute);
            if (result.IsFile && !uriText.StartsWith("file://"))
            {
                result = new Uri(result.ToString());
            }

            return result;
        }
    }
}
