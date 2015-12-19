// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Extension methods applied to class XmlWriter.</summary>
    public static class XmlWriterExtensions
    {
        /// <summary>Writes an element whose value is the supplied integer.</summary>
        /// <param name="writer">The writer to which an element shall be written.</param>
        /// <param name="localElementName">The name of the element to be written.</param>
        /// <param name="value">The value of the resulting element.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void WriteElementInt(this XmlWriter writer, string localElementName, int value)
        {
            if (String.IsNullOrWhiteSpace(localElementName))
            {
                throw new ArgumentException(ExceptionStrings.XmlElementNameWasUnset, "localElementName");
            }

            writer.WriteStartElement(String.Empty, localElementName, String.Empty);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        /// <summary>Writes an element if the supplied value is not null or whitespace.</summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="localElementName"/> is not set.</exception>
        /// <param name="writer">The writer to which an element shall be written.</param>
        /// <param name="localElementName">The name of the element to be written.</param>
        /// <param name="elementValue">The element string if the element should be written; otherwise, null.</param>
        public static void WriteElementIfNonempty(this XmlWriter writer, string localElementName, string elementValue)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (String.IsNullOrWhiteSpace(localElementName))
            {
                throw new ArgumentException(ExceptionStrings.XmlElementNameWasUnset, "localElementName");
            }

            if (String.IsNullOrWhiteSpace(elementValue))
            {
                return;
            }

            writer.WriteElementString(localElementName, elementValue);
        }

        internal static void WriteEntryList(
            this XmlWriter writer,
            string entryListName,
            IEnumerable<KeyValuePair<string, string>> entryListEntries)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(entryListName));

            // Null is semantically the same as an empty list
            if (entryListEntries == null)
            {
                return;
            }

            using (IEnumerator<KeyValuePair<string, string>> entryEnumerator = entryListEntries.GetEnumerator())
            {
                if (!entryEnumerator.MoveNext())
                {
                    // There were no entries to enumerate. Don't even write the outer element.
                    return;
                }

                // We now know that there is at least one entry to enumerate. As a result, we want to
                // write the outer node.
                writer.WriteStartElement(entryListName);

                do
                {
                    KeyValuePair<string, string> currentEntry = entryEnumerator.Current;
                    writer.WriteStartElement("ENTRY");
                    writer.WriteAttributeString("key", currentEntry.Key);
                    writer.WriteValue(currentEntry.Value);
                    writer.WriteEndElement(); // </ENTRY>
                } while (entryEnumerator.MoveNext());
            }

            writer.WriteEndElement(); // </entryListName>
        }
    }
}
