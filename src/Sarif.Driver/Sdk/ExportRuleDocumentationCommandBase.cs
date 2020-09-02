// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ExportRuleDocumentationCommandBase<TContext> : PlugInDriverCommand<ExportRuleDocumentationOptions>
    {
        private readonly IFileSystem _fileSystem;

        public ExportRuleDocumentationCommandBase(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public override int Run(ExportRuleDocumentationOptions options)
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

                _fileSystem.WriteAllText(options.OutputFilePath, sb.ToString());
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
            sb.AppendLine($"{rule.FullDescription.Text}{Environment.NewLine}");
            sb.AppendLine($"### Messages{Environment.NewLine}");

            foreach (KeyValuePair<string, MultiformatMessageString> message in rule.MessageStrings)
            {
                sb.AppendLine($"#### `{message.Key.Split('_').Last()}`: {rule.DefaultLevel}{Environment.NewLine}");
                sb.AppendLine($"{message.Value.Text}{Environment.NewLine}");
            }

            sb.AppendLine($"---{Environment.NewLine}");
        }
    }
}
