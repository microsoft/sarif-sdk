// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Completely populate all Region property members. Missing data
        /// is computed based on the values that are already present.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="newLineIndex"></param>
        public static void Populate(this Region region, NewLineIndex newLineIndex)
        {
            // TODO: we need charOffset and byteOffset to be expressed as
            // nullable types in order to differentiate between text
            // and binary file regions. For text files, we need to populate
            // startLine, etc. based on document offset. For now, we'll 
            // assume we're always looking at text files

            if (region.StartLine == 0)
            {
                OffsetInfo offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.CharOffset);
                region.StartLine = offsetInfo.LineNumber;
                region.StartColumn = offsetInfo.ColumnNumber;

                offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.CharOffset + region.Length);
                region.StartLine = offsetInfo.LineNumber;
                region.EndColumn = offsetInfo.ColumnNumber;
            }
            else
            {
                // Make endColumn and endLine explicit, if not expressed
                if (region.EndLine == 0) { region.EndLine = region.StartLine; }
                if (region.EndColumn == 0) { region.EndColumn = region.StartColumn; }

                LineInfo lineInfo = newLineIndex.GetLineInfoForLine(region.StartLine);
                region.CharOffset = lineInfo.StartOffset + (region.StartColumn - 1);

                lineInfo = newLineIndex.GetLineInfoForLine(region.EndLine);
                region.Length = lineInfo.StartOffset + (region.EndColumn - 1) - region.CharOffset;
            }
        }

        public static string GetMessageText(this Result result, RunLog runLog, bool concise = false)
        {
            if (concise && !string.IsNullOrEmpty(result.ShortMessage))
            {
                return result.ShortMessage;
            }

            string text = result.FullMessage;

            if (string.IsNullOrEmpty(text))
            {
                string ruleId = result.RuleId;
                string formatSpecifierId = result.FormattedMessage.SpecifierId;
                string formatSpecifier;

                foreach (RuleDescriptor ruleDescriptor in runLog.RuleInfo)
                {
                    if (ruleDescriptor.Id == ruleId)
                    {
                        string[] arguments = new string[result.FormattedMessage.Arguments.Count];
                        result.FormattedMessage.Arguments.CopyTo(arguments, 0);
                        formatSpecifier = ruleDescriptor.FormatSpecifiers[formatSpecifierId];
                        text = string.Format(formatSpecifier, arguments);
                    }
                }
            }

            if (concise)
            {
                text = GetFirstSentence(text);
            }

            return text;
        }

        public static string GetFirstSentence(string text)
        {
            int length = 0;
            bool insideQuotes = false;

            foreach (char ch in text)
            {
                length++;
                switch (ch)
                {
                    case '\'':
                    {
                        insideQuotes = !insideQuotes;
                        break;
                    }

                    case '.':
                    {
                        if (insideQuotes) { continue; }
                        return text.Substring(0, length);
                    }
                }
            }
            return text;
        }
    }
}
