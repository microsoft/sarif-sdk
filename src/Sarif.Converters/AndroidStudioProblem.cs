// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// A Problem element from an Android Studio file.
    /// </summary>
    public class AndroidStudioProblem
    {
        /// <summary>The file in which the problem occurs.</summary>
        public readonly string File;
        /// <summary>
        /// The line on which the problem occurs in <see cref="P:File" /> if known; otherwise, 0.
        /// </summary>
        public readonly int Line;
        /// <summary>The module in which the problem occurs.</summary>
        public readonly string Module;
        /// <summary>The package in which the problem occurs.</summary>
        public readonly string Package;
        /// <summary>Type of the entry point.</summary>
        public readonly string EntryPointType;
        /// <summary>Fully qualified name of the entry point.</summary>
        public readonly string EntryPointName;
        /// <summary>The severity of the problem type.</summary>
        public readonly string Severity;
        /// <summary>The attribute key. (Not entirely sure what this is at the moment)</summary>
        public readonly string AttributeKey;
        /// <summary>The problem class; that is, the kind of problem.</summary>
        public readonly string ProblemClass;
        /// <summary>The hint values reported.</summary>
        public readonly ImmutableArray<string> Hints;
        /// <summary>The user-readable description.</summary>
        public readonly string Description;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidStudioProblem"/> class.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="b">The <see cref="Builder"/> from which the problem shall be constructed.</param>
        public AndroidStudioProblem(Builder b)
        {
            if (b.ProblemClass == null)
            {
                throw new ArgumentException(ConverterResources.AndroidStudioProblemMissingProblemClass);
            }

            if (b.Line != 0 && b.File == null)
            {
                throw new ArgumentException(ConverterResources.AndroidStudioFileMissing);
            }

            if (b.File == null && b.Module == null && b.Package == null && b.EntryPointName == null)
            {
                throw new ArgumentException(ConverterResources.AndroidStudioHasNoLocationInformation);
            }

            if ((b.EntryPointName == null) != (b.EntryPointType == null))
            {
                throw new ArgumentException(ConverterResources.AndroidStudioEntryPointMissingRequiredData);
            }

            this.File = b.File;
            this.Line = b.Line;
            this.Module = b.Module;
            this.Package = b.Package;
            this.EntryPointType = b.EntryPointType;
            this.EntryPointName = b.EntryPointName;
            this.Severity = b.Severity;
            this.AttributeKey = b.AttributeKey;
            this.ProblemClass = b.ProblemClass;
            this.Hints = b.Hints.IsDefaultOrEmpty ? ImmutableArray<string>.Empty : b.Hints;
            this.Description = b.Description;
        }

        /// <summary>A builder type to make it easier to construct <see cref="AndroidStudioProblem"/> objects.</summary>
        public struct Builder
        {
            /// <summary>The file in which the problem occurs.</summary>
            public string File;
            /// <summary>
            /// The line on which the problem occurs in <see cref="P:File" /> if known; otherwise, 0.
            /// </summary>
            public int Line;
            /// <summary>The module in which the problem occurs.</summary>
            public string Module;
            /// <summary>The package in which the problem occurs.</summary>
            public string Package;
            /// <summary>Type of the entry point.</summary>
            public string EntryPointType;
            /// <summary>Fully qualified name of the entry point.</summary>
            public string EntryPointName;
            /// <summary>The severity of the problem type.</summary>
            public string Severity;
            /// <summary>The attribute key. (Not entirely sure what this is at the moment)</summary>
            public string AttributeKey;
            /// <summary>The problem class; that is, the kind of problem.</summary>
            public string ProblemClass;
            /// <summary>The hint values reported.</summary>
            public ImmutableArray<string> Hints;
            /// <summary>The user-readable description.</summary>
            public string Description;

            /// <summary>Gets a value indicating whether this instance is empty.</summary>
            /// <value>true if this instance is empty, false if not.</value>
            public bool IsEmpty
            {
                get
                {
                    return this.File == null
                        && this.Line == 0
                        && this.Module == null
                        && this.Package == null
                        && this.EntryPointType == null
                        && this.EntryPointName == null
                        && this.Severity == null
                        && this.AttributeKey == null
                        && this.ProblemClass == null
                        && this.Hints.IsDefault
                        && this.Description == null;
                }
            }
        }

        /// <summary>Parses a "problem" node from an Android Studio log and consumes that node.</summary>
        /// <exception cref="XmlException">Thrown when the Android Studio file is incorrect.</exception>
        /// <param name="reader">The reader from which the problem shall be parsed.</param>
        /// <param name="strings">NameTable strings used to parse Android Studio files.</param>
        /// <returns>
        /// If the problem node is not empty, an instance of <see cref="AndroidStudioProblem" />;
        /// otherwise, null.
        /// </returns>
        public static AndroidStudioProblem Parse(XmlReader reader, AndroidStudioStrings strings)
        {
            if (!reader.IsStartElement(strings.Problem))
            {
                throw reader.CreateException(ConverterResources.AndroidStudioNotProblemElement);
            }

            Builder b = new Builder();
            if (!reader.IsEmptyElement)
            {
                int problemDepth = reader.Depth;
                reader.Read(); // Get to children
                while (reader.Depth > problemDepth)
                {
                    string nodeName = reader.LocalName;
                    if (StringReference.AreEqual(nodeName, strings.File))
                    {
                        b.File = reader.ReadElementContentAsString();
                    }
                    else if (StringReference.AreEqual(nodeName, strings.Line))
                    {
                        b.Line = Math.Max(1, reader.ReadElementContentAsInt());
                    }
                    else if (StringReference.AreEqual(nodeName, strings.Module))
                    {
                        b.Module = reader.ReadElementContentAsString();
                    }
                    else if (StringReference.AreEqual(nodeName, strings.Package))
                    {
                        b.Package = reader.ReadElementContentAsString();
                    }
                    else if (StringReference.AreEqual(nodeName, strings.EntryPoint))
                    {
                        ReadEntryPointElement(ref b, reader, strings);
                    }
                    else if (StringReference.AreEqual(nodeName, strings.ProblemClass))
                    {
                        ReadProblemClassElement(ref b, reader, strings);
                    }
                    else if (StringReference.AreEqual(nodeName, strings.Hints))
                    {
                        b.Hints = ReadHints(reader, strings);
                    }
                    else if (StringReference.AreEqual(nodeName, strings.Description))
                    {
                        b.Description = reader.ReadElementContentAsString();
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }

            reader.Read(); // Consume the empty / end element

            if (b.IsEmpty)
            {
                return null;
            }

            try
            {
                return new AndroidStudioProblem(b);
            }
            catch (ArgumentException invalidData)
            {
                throw reader.CreateException(invalidData.Message);
            }
        }

        // These functions take ref Builder to avoid boxing Builder

        private static void ReadEntryPointElement(ref Builder b, XmlReader reader, AndroidStudioStrings strings)
        {
            while (reader.MoveToNextAttribute())
            {
                string name = reader.LocalName;
                if (StringReference.AreEqual(name, strings.Type))
                {
                    b.EntryPointType = reader.Value;
                }
                else if (StringReference.AreEqual(name, strings.FQName))
                {
                    b.EntryPointName = reader.Value;
                }
            }

            reader.Skip();
        }

        private static void ReadProblemClassElement(ref Builder b, XmlReader reader, AndroidStudioStrings strings)
        {
            while (reader.MoveToNextAttribute())
            {
                string name = reader.LocalName;
                if (StringReference.AreEqual(name, strings.Severity))
                {
                    b.Severity = reader.Value;
                }
                else if (StringReference.AreEqual(name, strings.AttributeKey))
                {
                    b.AttributeKey = reader.Value;
                }
            }

            reader.MoveToElement();
            b.ProblemClass = reader.ReadElementContentAsString();
        }

        /// <summary>Reads a "hints" element, consuming the element.</summary>
        /// <exception cref="XmlException">Thrown when the Android Studio file is incorrect.</exception>
        /// <param name="reader">The reader from which the hints shall be parsed.</param>
        /// <param name="strings">NameTable strings used to parse Android Studio files.</param>
        /// <returns>The set of hint values from <paramref name="reader"/>.</returns>
        public static ImmutableArray<string> ReadHints(XmlReader reader, AndroidStudioStrings strings)
        {
            Debug.Assert(StringReference.AreEqual(reader.LocalName, strings.Hints), "ReadHints didn't have hints");

            ImmutableArray<string>.Builder result = ImmutableArray.CreateBuilder<string>();
            if (!reader.IsEmptyElement)
            {
                int hintsDepth = reader.Depth;
                reader.Read(); // Consume start element
                while (reader.Depth > hintsDepth)
                {
                    if (!StringReference.AreEqual(reader.LocalName, strings.Hint))
                    {
                        throw reader.CreateException(ConverterResources.AndroidStudioHintsElementContainedNonHint);
                    }

                    string hintContent = reader.GetAttribute(strings.Value);
                    if (hintContent == null)
                    {
                        throw reader.CreateException(ConverterResources.AndroidStudioHintElementMissingValue);
                    }

                    if (hintContent.Length != 0)
                    {
                        result.Add(hintContent);
                    }

                    reader.Skip();
                }
            }

            reader.Read(); // Consume the end / empty element
            return result.ToImmutable();
        }
    }
}
