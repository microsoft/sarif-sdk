// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifVersionOneToCurrentVisitor : SarifRewritingVisitorVersionOne
    {
        public SarifLog SarifLog { get; private set; }

        public override SarifLogVersionOne VisitSarifLogVersionOne(SarifLogVersionOne node)
        {
            SarifLog = new SarifLog();
            SarifLog.Runs = new List<Run>();
            SarifLog.Version = SarifVersion.TwoZeroZero;

            foreach (RunVersionOne run in node.Runs)
            {
                VisitRunVersionOne(run);
            }

            return null;
        }

        public override RunVersionOne VisitRunVersionOne(RunVersionOne node)
        {
            if (node != null)
            {
                Run run = new Run()
                {
                    Architecture = node.Architecture,
                    BaselineId = node.BaselineId,
                    AutomationId = node.AutomationId,
                    DefaultFileEncoding = "???",
                    Id = node.Id,
                    Results = new List<Result>(),
                    RichMessageMimeType = "???"
                };

                SarifLog.Runs.Add(run);

                VisitToolVersionOne(node.Tool);
            }

            return null;
        }

        public override ResultVersionOne VisitResultVersionOne(ResultVersionOne node)
        {
            return base.VisitResultVersionOne(node);
        }

        public override ToolVersionOne VisitToolVersionOne(ToolVersionOne node)
        {
            if (node != null)
            {
                Tool tool = new Tool()
                {
                    FileVersion = node.FileVersion,
                    FullName = node.FullName,
                    Language = node.Language,
                    Name = node.Name,
                    Properties = node.Properties,
                    SarifLoggerVersion = node.SarifLoggerVersion,
                    SemanticVersion = node.SemanticVersion,
                    Version = node.Version
                };

                foreach (string propertyName in node.PropertyNames)
                {
                    tool.PropertyNames.Add(propertyName);
                }

                tool.Tags.UnionWith(node.Tags);

                SarifLog.Runs.Last().Tool = tool;
            }

            return null;
        }
        
    }
}
