// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExportCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public ExportCommand()
        {
            _fileSystem = new FileSystem();
        }

        public int Run(ExportOptions options)
        {
            try
            {
                var list = CompositionUtilities.GetExports<SarifValidationSkimmerBase>(
                    new Assembly[] { Assembly.GetExecutingAssembly() }).ToList();

                var sb = new StringBuilder();
                sb.AppendLine("# Rules");
                sb.AppendLine();

                foreach (SarifValidationSkimmerBase rule in list)
                {
                    BuildRule(rule, sb);
                }

                _fileSystem.WriteAllText($"{options.OutputDirectoryPath}\\rules.md", sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private void BuildRule(SarifValidationSkimmerBase rule, StringBuilder sb)
        {
            sb.AppendLine($"## Rule `{rule.Id}.{rule.Name}`");
            sb.AppendLine();
            sb.AppendLine("### Description");
            sb.AppendLine();
            sb.AppendLine(rule.FullDescription.Text);
            sb.AppendLine();
            sb.AppendLine("### Messages");
            sb.AppendLine();

            foreach (System.Collections.Generic.KeyValuePair<string, MultiformatMessageString> message in rule.MessageStrings)
            {
                sb.AppendLine($"#### `{message.Key.Split('_').Last()}`: {rule.DefaultLevel.ToString()}");
                sb.AppendLine();
                sb.AppendLine($"{message.Value.Text}");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }
    }
}
