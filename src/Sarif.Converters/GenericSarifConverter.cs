// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class GenericSarifConverter : ToolFileConverterBase
    {
        public override string ToolName => ToolFormat.GenericSarif;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            var serializer = new JsonSerializer() { };

            using (JsonTextReader reader = new JsonTextReader(new StreamReader(input)))
            {
                var log = serializer.Deserialize<SarifLog>(reader);

                foreach (var run in log.Runs)
                {
                    if (run?.Tool?.Driver?.Rules?.Count > 0)
                    {
                        foreach (var rule in run.Tool.Driver.Rules)
                        {
                            if (rule.Help != null)
                            {
                                if (!string.IsNullOrWhiteSpace(rule.Help.Markdown) && string.IsNullOrWhiteSpace(rule.Help.Text))
                                {
                                    rule.Help.Text = rule.Help.Markdown;
                                }
                                else if (!string.IsNullOrWhiteSpace(rule.Help.Text) && string.IsNullOrWhiteSpace(rule.Help.Markdown))
                                {
                                    rule.Help.Markdown = rule.Help.Text;
                                }
                            }

                        }
                    }
                }

                PersistResults(output, log);
            }            
        }
    }
}
