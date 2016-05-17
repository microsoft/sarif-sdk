// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class ExtensionMethods
    {

        public static string FormatForVisualStudio(this Region region)
        {
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
                        region.StartLine.ToString() + "," +
                        (region.StartColumn > 0 ? region.StartColumn.ToString() : "1") + "," +
                        region.EndLine.ToString() + "," +
                        (region.EndColumn > 0 ? region.EndColumn.ToString() : "1") +
                        ")";
                }
                //  (startLine-endLine)
                return
                    "(" +
                    region.StartLine.ToString() + "-" + region.EndLine.ToString() +
                    ")";
            }

            if (multicolumn)
            {
                // (startLine,startColumn-endColumn)
                return
                    "(" +
                    region.StartLine.ToString() + "," +
                    region.StartColumn.ToString() + "-" +
                    region.EndColumn.ToString() +
                    ")";
            }

            if (region.StartColumn > 1)
            {
                // (startLine,startColumn)
                return
                     "(" +
                     region.StartLine.ToString() + "," + region.StartColumn.ToString() +
                     ")";
            }
            // (startLine)
            return
                 "(" +
                 region.StartLine.ToString() +
                 ")";
        }

        public static string GetMessageText(this Result result, IRule rule, bool concise = false)
        {
            string text = result.Message;

            if (string.IsNullOrEmpty(text))
            {
                Debug.Assert(rule != null);

                string ruleId = result.RuleId;
                string formatId = result.FormattedRuleMessage.FormatId;
                string messageFormat;

                string[] arguments = null;

                if (result.FormattedRuleMessage.Arguments != null)
                {
                    arguments = new string[result.FormattedRuleMessage.Arguments.Count];
                    result.FormattedRuleMessage.Arguments.CopyTo(arguments, 0);
                }
                else
                {
                    arguments = new string[0];
                }

                Debug.Assert(rule.MessageFormats.ContainsKey(formatId));

                messageFormat = rule.MessageFormats[formatId];

#if DEBUG
                int argumentsCount = arguments.Length;
                for (int i = 0; i < argumentsCount; i++)
                {
                    // If this assert fires, there are too many arguments for the specifier
                    // or there is an argument is skipped or not consumed in the specifier
                    Debug.Assert(messageFormat.Contains("{" + i.ToString() + "}"));
                }
#endif

                text = string.Format(messageFormat, arguments);

#if DEBUG
                // If this assert fires, an insufficient # of arguments might
                // have been provided to String.Format.
                Debug.Assert(!text.Contains("{"));
#endif
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
            bool withinQuotes = false;
            bool withinParentheses = false;

            foreach (char ch in text)
            {
                length++;
                switch (ch)
                {
                    case '\'':
                    {
                        // we'll ignore everything within parenthized text
                        if (!withinParentheses)
                        {
                            withinQuotes = !withinQuotes;
                        }
                        break;
                    }

                    case '(':
                    {
                        if (!withinQuotes)
                        {
                            withinParentheses = true;
                        }
                        break;
                    }

                    case ')':
                    {
                        if (!withinQuotes)
                        {
                            withinParentheses = false;
                        }
                        break;
                    }
                    case '\n':
                    case '\r':
                    case '.':
                    {
                        if (withinQuotes || withinParentheses) { continue; }
                        return text.Substring(0, length).TrimEnd('\r', '\n');
                    }
                }
            }
            return text;
        }
    }
}
