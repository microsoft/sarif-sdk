// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ExportRulesDocumentationCommandBase : PlugInDriverCommand<ExportRulesDocumentationOptions>
    {
        private const string DefaultOutputFileName = "Rules.md";
        private readonly IFileSystem _fileSystem;
        private static readonly Regex s_friendlyNameRegex = new Regex("(?<level>Error|Warning|Note|None)_(?<friendlyName>[^_]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ExportRulesDocumentationCommandBase(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? FileSystem.Instance;
        }

        public override int Run(ExportRulesDocumentationOptions options)
        {
            try
            {
                List<Assembly> assemblies = LoadAssemblies(options.AssemblyPaths);

                var rules = CompositionUtilities.GetExports<ReportingDescriptor>(assemblies).ToList();

                var sb = new StringBuilder();
                sb.Append("# Rules").AppendLine(Environment.NewLine);

                foreach (ReportingDescriptor rule in rules)
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

        internal List<Assembly> LoadAssemblies(IEnumerable<string> assemblyPaths)
        {
            var assemblies = new List<Assembly>();
            if (!assemblyPaths.Any())
            {
                assemblies.Add(this.GetType().Assembly);
            }
            else
            {
                foreach (string assemblyPath in assemblyPaths)
                {
                    assemblies.Add(Assembly.LoadFrom(assemblyPath));
                }
            }

            return assemblies;
        }

        internal void BuildRule(ReportingDescriptor rule, StringBuilder sb)
        {
            sb.Append("## Rule `").Append(rule.Moniker).Append('`').AppendLine(Environment.NewLine);
            sb.Append("### Description").AppendLine(Environment.NewLine);
            sb.Append(rule.FullDescription?.Markdown
                ?? rule.FullDescription?.Text
                ?? rule.ShortDescription?.Markdown
                ?? rule.ShortDescription?.Text
                ?? DriverResources.NoRuleDescription).AppendLine(Environment.NewLine);
            sb.Append("### Messages").AppendLine(Environment.NewLine);

            foreach (KeyValuePair<string, MultiformatMessageString> message in rule.MessageStrings)
            {
                string ruleName = message.Key;
                Match match = s_friendlyNameRegex.Match(message.Key);
                if (match.Success)
                {
                    ruleName = match.Groups["friendlyName"].Value;
                }

                sb.Append("#### `").Append(ruleName).Append("`: ").Append(rule.DefaultConfiguration.Level).AppendLine(Environment.NewLine);
                sb.Append(message.Value.Markdown ?? message.Value.Text).AppendLine(Environment.NewLine);
            }

            sb.Append("---").AppendLine(Environment.NewLine);
        }
    }
}
