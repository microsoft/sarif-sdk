// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ExportRulesDocumentationCommandBase<TContext> : PlugInDriverCommand<ExportRulesDocumentationOptions>
    {
        private const string DefaultOutputFileName = "Rules.md";
        private readonly IFileSystem _fileSystem;
        private static readonly Regex s_friendlyNameRegex = new Regex(@"(?<level>Error|Warning|Note|None)_(?<friendlyName>[^_]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ExportRulesDocumentationCommandBase(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? FileSystem.Instance;
        }

        public override int Run(ExportRulesDocumentationOptions options)
        {
            try
            {
                var rules = CompositionUtilities.GetExports<Skimmer<TContext>>(DefaultPlugInAssemblies).ToList();

                var sb = new StringBuilder();
                sb.AppendLine($"# Rules{Environment.NewLine}");

                foreach (Skimmer<TContext> rule in rules)
                {
                    BuildRule(rule, sb);
                }

                _fileSystem.WriteAllText(options.OutputFilePath ?? DefaultOutputFileName, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        internal void BuildRule(Skimmer<TContext> rule, StringBuilder sb)
        {
            sb.AppendLine($"## Rule `{rule.Moniker}`{Environment.NewLine}");
            sb.AppendLine($"### Description{Environment.NewLine}");
            sb.AppendLine(@$"{rule.FullDescription?.Markdown
                ?? rule.FullDescription?.Text
                ?? rule.ShortDescription?.Markdown
                ?? rule.ShortDescription?.Text
                ?? DriverResources.NoRuleDescription}{Environment.NewLine}");
            sb.AppendLine($"### Messages{Environment.NewLine}");

            foreach (KeyValuePair<string, MultiformatMessageString> message in rule.MessageStrings)
            {
                string ruleName = message.Key;
                Match match = s_friendlyNameRegex.Match(message.Key);
                if (match.Success)
                {
                    ruleName = match.Groups["friendlyName"].Value;
                }

                sb.AppendLine($"#### `{ruleName}`: {rule.DefaultConfiguration.Level}{Environment.NewLine}");
                sb.AppendLine($"{message.Value.Markdown ?? message.Value.Text}{Environment.NewLine}");
            }

            sb.AppendLine($"---{Environment.NewLine}");
        }
    }
}
