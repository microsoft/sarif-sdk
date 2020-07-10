// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class ExtensionMethods
    {
        public static IDictionary<string, MultiformatMessageString> ConvertToMultiformatMessageStringsDictionary(this IDictionary<string, string> v1MessageStringsDictionary)
        {
            return v1MessageStringsDictionary?.ToDictionary(
                 keyValuePair => keyValuePair.Key,
                 keyValuePair => new MultiformatMessageString { Text = keyValuePair.Value });
        }

        public static bool IsEmptyEnumerable(this object value)
        {
            if (!(value is IEnumerable e))
            {
                return false;
            }
            else
            {
                // This is empty if MoveNext returns false the first time
                return !e.GetEnumerator().MoveNext();
            }
        }

        public static bool HasAtLeastOneNonNullValue<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any((m) => m != null);
        }

        public static bool HasAtLeastOneNonDefaultValue<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer) where T : new()
        {
            var defaultInstance = new T();
            return collection != null && collection.Any((m) => m != null && !comparer.Equals(defaultInstance, m));
        }

        public static string InstanceIdInstanceComponent(this RunAutomationDetails runAutomationDetails)
        {
            string instanceId = runAutomationDetails.Id;

            if (instanceId == null)
            {
                return null;
            }

            if (instanceId.EndsWith("/"))
            {
                return string.Empty;
            }

            return instanceId.Substring(instanceId.LastIndexOf('/') + 1);
        }

        public static string InstanceIdLogicalComponent(this RunAutomationDetails runAutomationDetails)
        {
            string instanceId = runAutomationDetails.Id;

            if (instanceId == null)
            {
                return null;
            }

            if (!instanceId.Contains("/"))
            {
                return null;
            }

            return instanceId.Substring(0, instanceId.LastIndexOf('/'));
        }

        public static bool IsEqualToOrHierarchicalDescendantOf(this string child, string parent)
        {
            if (child == parent) { return true; }

            string[] childComponents = child.Split(SarifConstants.HierarchicalComponentSeparator);
            string[] parentComponents = parent.Split(SarifConstants.HierarchicalComponentSeparator);

            if (childComponents.Length < parentComponents.Length) { return false; }

            for (int i = 0; i < parentComponents.Length; ++i)
            {
                if (childComponents[i] != parentComponents[i]) { return false; }
            }

            return true;
        }

        public static Message ToMessage(this string text)
        {
            return new Message { Text = text };
        }

        public static string ToAndPhrase(this List<string> words)
        {
            words = words.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            if (words.Count == 0)
            {
                return "''";
            }
            else if (words.Count == 1)
            {
                return string.Format("'{0}'", words[0]);
            }
            else
            {
                string phrasedWithAnd = string.Format("'{0}'", words[0]);
                for (int j = 1; j < words.Count - 1; j++)
                {
                    phrasedWithAnd = string.Join(", ", phrasedWithAnd, string.Format("'{0}'", words[j]));
                }
                phrasedWithAnd = string.Join(" and ", phrasedWithAnd, string.Format("'{0}'", words[words.Count - 1]));
                return phrasedWithAnd;
            }
        }

        public static OptionallyEmittedData ToFlags(this IEnumerable<OptionallyEmittedData> optionallyEmittedData)
        {
            OptionallyEmittedData convertedToFlags = OptionallyEmittedData.None;
            if (optionallyEmittedData != null)
            {
                Array.ForEach(optionallyEmittedData.ToArray(), data => convertedToFlags |= data);
            }

            return convertedToFlags;
        }

        public static string GetFileName(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new InvalidOperationException();
            }

            return Path.GetFileName(uri.LocalPath);
        }

        public static string FormatForVisualStudio(this Region region)
        {
            if (region == null)
            {
                return string.Empty;
            }

            if (region.StartLine < 0)
            {
                throw new NotImplementedException();
            }

            // VS supports the following formatting options:
            //    (startLine)
            //    (startLine-endLine)
            //    (startLine,startColumn)
            //    (startLine,startColumn-endColumn)
            //    (startLine,startColumn,endLine,endColumn)

            bool multiline = region.EndLine > region.StartLine;
            bool multicolumn = (multiline || region.EndColumn > region.StartColumn);

            if (multiline)
            {
                if (multicolumn && (region.StartColumn > 1 || region.EndColumn > 1))
                {
                    //  (startLine,startColumn,endLine,endColumn)
                    return
                        "(" +
                        region.StartLine.ToString(CultureInfo.InvariantCulture) + "," +
                        (region.StartColumn > 0 ? region.StartColumn.ToString(CultureInfo.InvariantCulture) : "1") + "," +
                        region.EndLine.ToString(CultureInfo.InvariantCulture) + "," +
                        (region.EndColumn > 0 ? region.EndColumn.ToString(CultureInfo.InvariantCulture) : "1") +
                        ")";
                }
                //  (startLine-endLine)
                return
                    "(" +
                    region.StartLine.ToString(CultureInfo.InvariantCulture) + "-" + region.EndLine.ToString(CultureInfo.InvariantCulture) +
                    ")";
            }

            if (multicolumn)
            {
                // (startLine,startColumn-endColumn)
                return
                    "(" +
                    region.StartLine.ToString(CultureInfo.InvariantCulture) + "," +
                    region.StartColumn.ToString(CultureInfo.InvariantCulture) + "-" +
                    region.EndColumn.ToString(CultureInfo.InvariantCulture) +
                    ")";
            }

            if (region.StartColumn > 1)
            {
                // (startLine,startColumn)
                return
                     "(" +
                     region.StartLine.ToString(CultureInfo.InvariantCulture) + "," + region.StartColumn.ToString(CultureInfo.InvariantCulture) +
                     ")";
            }
            // (startLine)
            return
                 "(" +
                 region.StartLine.ToString(CultureInfo.InvariantCulture) +
                 ")";
        }

        public static string FormatForVisualStudio(this Result result, ReportingDescriptor rule = null)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (rule == null)
            {
                rule = result.GetRule();
            }

            var messageLines = new List<string>();

            foreach (Location location in result.Locations)
            {
                Uri uri = location.PhysicalLocation.ArtifactLocation.Uri;
                string path = uri.IsAbsoluteUri && uri.IsFile ? uri.LocalPath : uri.ToString();
                messageLines.Add(
                    string.Format(
                        CultureInfo.InvariantCulture, "{0}{1}: {2} {3}: {4}",
                        path,
                        location.PhysicalLocation.Region.FormatForVisualStudio(),
                        result.Kind == ResultKind.Fail ? result.Level.FormatForVisualStudio() : result.Kind.FormatForVisualStudio(),
                        result.RuleId,
                        result.GetMessageText(rule)
                        ));
            }

            return string.Join(Environment.NewLine, messageLines);
        }

        public static string FormatForVisualStudio(this FailureLevel level)
        {
            switch (level)
            {
                case FailureLevel.Error:
                    return "error";

                case FailureLevel.Warning:
                    return "warning";

                case FailureLevel.Note:
                    return "note";

                default:
                    throw new InvalidOperationException();
            }
        }

        public static string FormatForVisualStudio(this ResultKind kind)
        {
            switch (kind)
            {
                case ResultKind.Informational:
                    return "info";
                case ResultKind.NotApplicable:
                    return "notapplicable";
                case ResultKind.Open:
                    return "open";
                case ResultKind.Pass:
                    return "pass";
                case ResultKind.Review:
                    return "review";
                default:
                    return "info";
            }
        }

        public static string GetMessageText(this Result result, ReportingDescriptor rule)
        {
            return GetMessageText(result, rule, concise: false);
        }

        public static string GetMessageText(this Result result, ReportingDescriptor rule, bool concise = false, int maxLength = 120)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            string text = result.Message?.Text;
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;    // Ensure that it's not null.

                if (rule == null)
                {
                    rule = result.GetRule();
                }

                if (rule != null)
                {
                    string messageId = result.Message?.Id;
                    MultiformatMessageString formatString = null;

                    if (!string.IsNullOrWhiteSpace(messageId)
                        && rule.MessageStrings?.TryGetValue(messageId, out formatString) == true)
                    {
                        string[] arguments;

                        if (result.Message?.Arguments != null)
                        {
                            arguments = new string[result.Message.Arguments.Count];
                            result.Message.Arguments.CopyTo(arguments, 0);
                        }
                        else
                        {
                            arguments = new string[0];
                        }

                        text = GetFormattedMessage(formatString.Text, arguments);
                    }
                }
            }

            if (concise)
            {
                text = GetFirstSentence(text);
                if (text.Length > maxLength)
                {
                    text = text.Substring(0, maxLength) + "\u2026"; // \u2026 is Unicode "horizontal ellipsis".
                }
            }

            return text;
        }

        internal static string GetFormattedMessage(string formatString, string[] arguments)
        {
            string formattedMessage = string.Format(CultureInfo.InvariantCulture, formatString, arguments);

            return formattedMessage ?? string.Empty;
        }

        public static string GetFirstSentence(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text == string.Empty)
            {
                return text;
            }

            // We will return at most the first line
            string[] lineBreaks = { Environment.NewLine, "\r", "\n" };

            foreach (string s in lineBreaks)
            {
                int index = text.IndexOf(s);

                if (index > -1)
                {
                    text = text.Substring(0, index);
                    break;
                }
            }

            string pattern = @"^        # Start of string
                               .*?      # Zero or more characters, match fewest
                               [.?!]    # End-of-sentence punctuation characters
                               [)""']*  # Optional character that could bound the punctuation, such as 'The quick brown (fox.)'
                               (?=      # Start look-ahead
                               (\s+     # One or more spaces
                               \p{P}*   # Zero or more punctuation characters
                               \p{Lu})  # A capital letter
                               |        # Or...
                               \s*$     # Zero or more spaces followed by end of string
                               )        # End look-ahead";

            RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;
            Match match = Regex.Match(text, pattern, options);

            if (match.Success)
            {
                text = match.Value;
            }

            return text.TrimEnd('.') + ".";
        }

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
            if (properties != null && properties.TryGetValue(key, out string propValue))
            {
                return propValue;
            }

            return null;
        }

        /// <summary>Checks if a character is a newline.</summary>
        /// <param name="testedCharacter">The character to check.</param>
        /// <returns>true if newline, false if not.</returns>
        internal static bool IsNewline(this char testedCharacter)
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
        internal static bool Matches(this char[] array, int startIndex, string target)
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
    }
}