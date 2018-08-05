// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal static class FortifyUtilities
    {
        private const string XmlContentElementPattern = "<[/]?Content>";
        private const string XmlParagraphElementPattern = "<[/]?(Alt)?Paragraph>";
        private const string XmlUnsupportedElementPattern = "<[/]?(ConditionalText|IfDef){1}[^>]*>";
        private const string MultipleNewlinePattern = "(\r\n){2,}";

        private static readonly Dictionary<string, string> HtmlToMarkdownConversions = new Dictionary<string, string>
        {
            { "<[/]?b>", "**" },
            { "<[/]?i>", "_" },
            { "<[/]?code>", "`" },
            { "<[/]?pre>", "`" }
        };

        internal static string ParseFormattedContentText(string content)
        {
            content = Regex.Replace(content, XmlContentElementPattern, string.Empty);
            content = Regex.Replace(content, XmlParagraphElementPattern, Environment.NewLine);
            content = Regex.Replace(content, XmlUnsupportedElementPattern, string.Empty);
            content = Regex.Replace(content, MultipleNewlinePattern, Environment.NewLine);

            foreach (string pattern in HtmlToMarkdownConversions.Keys)
            {
                content = Regex.Replace(content, pattern, HtmlToMarkdownConversions[pattern]);
            }

            return content.Trim(new[] { '\r', '\n', ' ' });
        }
    }
}
