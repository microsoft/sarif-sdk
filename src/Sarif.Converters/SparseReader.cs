// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// A Fast XML Reader that reads only the requested entities
    /// and uses delegates to create objects along the way
    /// </summary>
    public sealed class SparseReader : IDisposable
    {
        private readonly SparseReaderDispatchTable _dispatchTable;
        private readonly XmlReader _xmlReader;

        /// <summary>Initializes a new instance of the <see cref="SparseReader"/> class.</summary>
        /// <param name="dispatchTable">The dispatch table used to fire delegates for XML elements on
        /// this instance.</param>
        /// <param name="xmlReader">The reader from which XML shall be retrieved. This SparseReader takes
        /// ownership of this XML reader and destroys it upon destruction.</param>
        public SparseReader(SparseReaderDispatchTable dispatchTable, XmlReader xmlReader)
        {
            _dispatchTable = dispatchTable;
            _xmlReader = xmlReader;
        }

        /// <summary>
        /// Creates a new <see cref="SparseReader"/> pointing to a <see cref="System.IO.Stream"/> rather
        /// than a <see cref="System.Xml.XmlReader"/>.
        /// </summary>
        /// <param name="dispatchTable">The dispatch table used to fire delegates for XML elements on
        /// this instance.</param>
        /// <param name="stream">The stream from which XML shall be retrieved. The SparseReader takes
        /// ownership of this stream and is responsible for destroying it.</param>
        /// <returns>
        /// A <see cref="SparseReader"/> wrapping the stream <paramref name="stream"/>.
        /// </returns>
        public static SparseReader CreateFromStream(SparseReaderDispatchTable dispatchTable, Stream stream)
        {
            return CreateFromStream(dispatchTable, stream, null);
        }

        /// <summary>
        /// Creates a new <see cref="SparseReader"/> pointing to a <see cref="System.IO.Stream"/> rather
        /// than a <see cref="System.Xml.XmlReader"/>.
        /// </summary>
        /// <param name="dispatchTable">The dispatch table used to fire delegates for XML elements on
        /// this instance.</param>
        /// <param name="stream">The stream from which XML shall be retrieved. The SparseReader takes
        /// ownership of this stream and is responsible for destroying it.</param>
        /// <param name="schemaSet">The xml schema to validate the input against</param>
        /// <returns>
        /// A <see cref="SparseReader"/> wrapping the stream <paramref name="stream"/>.
        /// </returns>
        public static SparseReader CreateFromStream(SparseReaderDispatchTable dispatchTable, Stream stream, XmlSchemaSet schemaSet)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                CloseInput = true,
                Schemas = schemaSet,
                XmlResolver = null
            };

            XmlReader xReader = null;
            try
            {
                xReader = XmlReader.Create(stream, settings);
                xReader.MoveToContent(); // If this throws, we destroy the reader in the finally block below.
                var sparseReader = new SparseReader(dispatchTable, xReader); // nothrow
                xReader = null; // Ownership transfered to SparseReader; don't destroy here
                return sparseReader;
            }
            finally
            {
                if (xReader != null)
                {
                    xReader.Dispose();
                }
            }
        }

        public bool IsEmptyElement { get { return _xmlReader.IsEmptyElement; } }

        /// <summary>Reads the children of <see cref="M:XmlReader"/>.</summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="parent">The parent.</param>
        public void ReadChildren(string tagName, object parent)
        {
            ReadChildren(tagName, parent, out string innerText);
        }

        /// <summary>Reads the children of <see cref="M:XmlReader"/>.</summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="innerText">The inner text, if any, associated with the tag.</param>
        public void ReadChildren(string tagName, object parent, out string innerText)
        {
            innerText = null;

            // check for empty element
            bool isEmpty = _xmlReader.IsEmptyElement;

            // start reading (assumes all attributes are already consumed)
            ReadStartElement();

            if (isEmpty)
                return; // DONE with this element

            while (!IsEndState(tagName))
            {
                // Try to call a handler for this element...
                if (!_dispatchTable.Dispatch(_xmlReader.LocalName, this, parent))
                {
                    if (_xmlReader.NodeType == XmlNodeType.Text)
                    {
                        innerText = _xmlReader.ReadContentAsString();
                        continue;
                    }

                    // ... and skip the element if no such handler was registered.
                    Skip();
                }
            }

            // conclude reading
            ReadEndElement(tagName);
        }

        /// <summary>Gets the local name of the current element.</summary>
        /// <value>The local name of the current element.</value>
        /// <seealso cref="P:XmlReader.LocalName"/>
        public string LocalName
        {
            get
            {
                return _xmlReader.LocalName;
            }
        }

        /// <summary>Skips the element on which this <see cref="SparseReader"/> is currently positioned.</summary>
        /// <seealso cref="M:XmlReader.Skip"/>
        public void Skip()
        {
            _xmlReader.Skip();
        }

        /// <summary>Reads an attribute value as a string.</summary>
        /// <param name="attributeName">Name of the attribute to read.</param>
        /// <returns>The attribute string if the attribute exists; or null if it doesn't exist.</returns>
        /// <seealso cref="M:XmlReader.GetAttribute"/>
        public string ReadAttributeString(string attributeName)
        {
            return _xmlReader.GetAttribute(attributeName);
        }

        /// <summary>Reads an attribute value as an integer.</summary>
        /// <param name="attributeName">Name of the attribute to read.</param>
        /// <returns>The attribute value converted to an integer if it exists; otherwise, null.</returns>
        /// <seealso cref="M:XmlReader.GetAttribute"/>
        public int? ReadAttributeInt(string attributeName)
        {
            string stringValue = _xmlReader.GetAttribute(attributeName);
            if (stringValue == null)
            {
                return null;
            }
            return XmlConvert.ToInt32(stringValue);
        }

        /// <summary>Reads an attribute value as a boolean.</summary>
        /// <param name="attributeName">Name of the attribute to read.</param>
        /// <returns>The attribute value converted to a boolean if it exists; otherwise, null.</returns>
        /// <seealso cref="M:XmlReader.GetAttribute"/>
        public bool? ReadAttributeBool(string attributeName)
        {
            string stringValue = _xmlReader.GetAttribute(attributeName);
            if (stringValue == null)
            {
                return null;
            }
            return XmlConvert.ToBoolean(stringValue);
        }

        /// <summary>Reads the current element's content as a string and consumes the element.</summary>
        /// <returns>The element's content as a string.</returns>
        /// <seealso cref="M:XmlReader.ReadElementContentAsString"/>
        public string ReadElementContentAsString()
        {
            return _xmlReader.ReadElementContentAsString();
        }

        /// <summary>
        /// Reads the current element's content as an integer and consumes the element.
        /// </summary>
        /// <returns>The element's content as a integer.</returns>
        /// <seealso cref="M:XmlReader.ReadElementContentAsInt32"/>
        public int ReadElementContentAsInt32()
        {
            return _xmlReader.ReadElementContentAsInt();
        }

        /// <summary>
        /// Read's the current element's content and attempts to convert it to an integer. Consumes the
        /// element.
        /// </summary>
        /// <returns>The element content as an int if it can be parsed as an int; otherwise, 0.</returns>
        /// <seealso cref="M:XmlReader.ReadElementContentAsInt32"/>
        public int ReadElementContentAsInt32OrDefault()
        {
            if (_xmlReader.IsEmptyElement)
            {
                this.Skip();
                return default(int);
            }

            int content;

            // The NumberStyles constants here are from XmlConvert.ToInt32:
            // http://referencesource.microsoft.com/#System.Xml/Xml/System/Xml/XmlConvert.cs#927
            if (int.TryParse(ReadElementContentAsString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out content))
            {
                return content;
            }

            return 0;
        }

        /// <summary>
        /// Reads the current element's content as a double and consumes the element.
        /// </summary>
        /// <returns>The element's content as a double.</returns>
        /// <seealso cref="M:XmlReader.ReadElementContentAsDouble"/>
        public double ReadElementContentAsDouble()
        {
            return _xmlReader.ReadElementContentAsDouble();
        }

        // http://referencesource.microsoft.com/#System.Xml/Xml/System/Xml/XmlConvert.cs#1415
        private static readonly char[] WhitespaceCharacters = new char[] { ' ', '\t', '\n', '\r' };

        /// <summary>
        /// Attempts to the current element's content as a double and consumes the element.
        /// </summary>
        /// <returns>The element's content as a double if it can be parsed as a double; otherwise, 0.0.</returns>
        /// <seealso cref="M:XmlReader.ReadElementContentAsDouble"/>
        public double ReadElementContentAsDoubleOrDefault()
        {
            if (_xmlReader.IsEmptyElement)
            {
                _xmlReader.Skip();
                return default(double);
            }

            string content = this.ReadElementContentAsString();
            if (content == null)
            {
                return default(double);
            }
            else
            {
                content = content.Trim(WhitespaceCharacters);

                if (content == "-INF")
                {
                    return double.NegativeInfinity;
                }

                if (content == "INF")
                {
                    return double.PositiveInfinity;
                }
            }

            double dVal;
            if (!double.TryParse(content,
                                 NumberStyles.AllowLeadingSign |
                                 NumberStyles.AllowDecimalPoint |
                                 NumberStyles.AllowExponent |
                                 NumberStyles.AllowLeadingWhite |
                                 NumberStyles.AllowTrailingWhite,
                                 NumberFormatInfo.InvariantInfo,
                                 out dVal))
            {
                return default(double);
            }

            if (dVal == 0 && content[0] == '-')
            {
                return -0d;
            }

            return dVal;
        }

        /// <summary>
        /// Destroys this <see cref="SparseReader"/> (and the underlying <see cref="_xmlReader"/>).
        /// </summary>
        /// <seealso cref="M:System.IDisposable.Dispose()"/>
        public void Dispose()
        {
            _xmlReader.Dispose();
        }

        private bool IsEndState(string expectedTagName)
        {
            return IsMatchingState(expectedTagName, false) || _xmlReader.EOF;
        }

        private bool IsMatchingState(string expectedTagName, bool isStartState = true)
        {
            // basic checks
            if (_xmlReader.IsStartElement() != isStartState ||
                string.IsNullOrEmpty(expectedTagName))
            {
                return false;
            }

            // check that tag names match
            return expectedTagName.Equals(_xmlReader.LocalName);
        }

        private void ReadStartElement()
        {
            _xmlReader.ReadStartElement();
            _xmlReader.MoveToContent();
        }

        private void ReadEndElement(string expectedTagName)
        {
            if (!IsEndState(expectedTagName))
            {
                throw new XmlException("XML Reader is in invalid state"); // TODO Code Analysis Exception
            }

            _xmlReader.ReadEndElement();
            _xmlReader.MoveToContent();
        }
    }
}