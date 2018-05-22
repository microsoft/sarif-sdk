// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifCurrentToVersionOneVisitor : SarifRewritingVisitor
    {
        public SarifLogVersionOne SarifLogVersionOne { get; private set; }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
            SarifLogVersionOne = new SarifLogVersionOne(SarifVersionVersionOne.OneZeroZero.ConvertToSchemaUri(),
                                    SarifVersionVersionOne.OneZeroZero,
                                    new List<RunVersionOne>());

            foreach (Run run in node.Runs)
            {
                VisitRun(run);
            }

            return null;
        }

        public new RunVersionOne VisitRun(Run v2Run)
        {
            if (v2Run != null)
            {
                RunVersionOne run = new RunVersionOne()
                {
                    Architecture = v2Run.Architecture,
                    AutomationId = v2Run.AutomationLogicalId,
                    BaselineId = v2Run.BaselineInstanceGuid,
                    Id = v2Run.InstanceGuid,
                    Properties = v2Run.Properties,
                    Results = new List<ResultVersionOne>(),
                    StableId = v2Run.LogicalId,
                    Tool = CreateTool(v2Run.Tool)
                };

                SarifLogVersionOne.Runs.Add(run);

                if (v2Run.LogicalLocations != null)
                {
                    run.LogicalLocations = new Dictionary<string, LogicalLocationVersionOne>();

                    foreach (var pair in v2Run.LogicalLocations)
                    {
                        run.LogicalLocations.Add(pair.Key, CreateLogicalLocationVersionOne(pair.Value));
                    }
                }

                if (v2Run.Tags.Count > 0)
                {
                    run.Tags.UnionWith(v2Run.Tags);
                }
            }

            return null;
        }

        public static LogicalLocationVersionOne CreateLogicalLocationVersionOne(LogicalLocation v2LogicalLocatiom)
        {
            LogicalLocationVersionOne logicalLocation = null;

            if (v2LogicalLocatiom != null)
            {
                logicalLocation = new LogicalLocationVersionOne
                {
                    Kind = v2LogicalLocatiom.Kind,
                    Name = v2LogicalLocatiom.Name,
                    ParentKey = v2LogicalLocatiom.ParentKey
                };
            }

            return logicalLocation;
        }

        public static ToolVersionOne CreateTool(Tool v2Tool)
        {
            ToolVersionOne tool = null;

            if (v2Tool != null)
            {
                tool = new ToolVersionOne()
                {
                    FileVersion = v2Tool.FileVersion,
                    FullName = v2Tool.FullName,
                    Language = v2Tool.Language,
                    Name = v2Tool.Name,
                    Properties = v2Tool.Properties,
                    SarifLoggerVersion = v2Tool.SarifLoggerVersion,
                    SemanticVersion = v2Tool.SemanticVersion,
                    Version = v2Tool.Version
                };

                if (v2Tool.Tags.Count > 0)
                {
                    tool.Tags.UnionWith(v2Tool.Tags);
                }
            }

            return tool;
        }
    }
}
