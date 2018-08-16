// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal static class FortifyUtilities
    {
        private static readonly Dictionary<string, string> FormattedTextReplacementss = new Dictionary<string, string>
        {
            // XML <Content> tag
            { "<[/]?Content>", string.Empty },

            // XML <Paragraph> and <AltParagraph> tags
            { "<[/]?(Alt)?Paragraph>", Environment.NewLine },
            
            // XML <ConditionalText> and <IfDef> tags
            { "<[/]?(ConditionalText|IfDef){1}[^>]*>", string.Empty },
            
            // Multiple newlines, plus (sometimes) spaces
            { "\\n[\\r\\n ]*\\n", Environment.NewLine + Environment.NewLine },
            
            // HTML <b> => Markdown bold
            { "<[/]?b>", "**" },
            
            // HTML <i> => Markdown italics
            { "<[/]?i>", "_" },
            
            // HTML <code> => Markdown monospace
            { "<[/]?code>", "`" },
            
            // HTML <pre> => Markdown monospace
            { "<[/]?pre>", "`" }
        };

        internal static string ParseFormattedContentText(string content)
        {
            foreach (string pattern in FormattedTextReplacementss.Keys)
            {
                content = Regex.Replace(content, pattern, FormattedTextReplacementss[pattern], RegexOptions.Compiled);
            }

            return content.Trim(new[] { '\r', '\n', ' ' });
        }
    }
}
