// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public static class Extensions
    {
        // Compare tool format strings with appropriate comparison type.
        public static bool MatchesToolFormat(this string toolFormat, string other)
        {
            return toolFormat.Equals(other, StringComparison.OrdinalIgnoreCase);
        }

        // Determine whether a type has a constructor that takes no arguments.
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public,
                            binder: null,
                            types: new Type[0], // The types of the constructor arguments.
                            modifiers: new ParameterModifier[0]) != null;
        }

        // Enforce the convention that the converter type name is derived from the tool name.
        // It can reside in any namespace.
        public static string ConverterTypeName(this string toolFormat)
        {
            return toolFormat + "Converter";
        }

        /// <summary>An XmlReader extension method that reads optional element's content as string.</summary>
        /// <param name="xmlReader">The xmlReader from which line data shall be retrieved.</param>
        /// <param name="elementName">Name of the element expected.</param>
        /// <returns>The optional element content as string if the element is present; otherwise, null.</returns>
        internal static string ReadOptionalElementContentAsString(this XmlReader xmlReader, string elementName)
        {
            if (xmlReader.NodeType == XmlNodeType.Element && StringReference.AreEqual(xmlReader.LocalName, elementName))
            {
                return xmlReader.ReadElementContentAsString();
            }

            return null;
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
            return xmlReader.CreateException(string.Format(CultureInfo.CurrentCulture, message, args));
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
                    throw xmlReader.CreateException(ConverterResources.ExpectedElementNamed, elementName);
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
            return xmlReader.NodeType == XmlNodeType.Element && StringReference.AreEqual(xmlReader.LocalName, elementName);
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
    }
}
