// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Newtonsoft.Json;
using Utilities = Microsoft.CodeAnalysis.Sarif.Visitors.SarifTransformerUtilities;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifCurrentToVersionOneVisitor : SarifRewritingVisitor
    {
        private static readonly SarifVersion FromSarifVersion = SarifVersion.TwoZeroZero;
        private static readonly string FromPropertyBagPrefix =
            Utilities.PropertyBagTransformerItemPrefixes[FromSarifVersion];

        private RunVersionOne _currentRun = null;

        public SarifLogVersionOne SarifLogVersionOne { get; private set; }

        public override SarifLog VisitSarifLog(SarifLog v2SarifLog)
        {
            SarifLogVersionOne = new SarifLogVersionOne(SarifVersionVersionOne.OneZeroZero.ConvertToSchemaUri(),
                                                        SarifVersionVersionOne.OneZeroZero,
                                                        new List<RunVersionOne>());

            foreach (Run v2Run in v2SarifLog.Runs)
            {
                SarifLogVersionOne.Runs.Add(CreateRun(v2Run));
            }

            return null;
        }

        public FileDataVersionOne CreateFileDataVersionOne(FileData v2FileData)
        {
            FileDataVersionOne fileData = null;

            if (v2FileData != null)
            {
                fileData = new FileDataVersionOne
                {
                    Length = v2FileData.Length,
                    MimeType = v2FileData.MimeType,
                    Offset = v2FileData.Offset,
                    ParentKey = v2FileData.ParentKey,
                    Properties = v2FileData.Properties
                };

                if (v2FileData.FileLocation != null)
                {
                    fileData.Uri = v2FileData.FileLocation.Uri;
                    fileData.UriBaseId = v2FileData.FileLocation.UriBaseId;
                }

                if (v2FileData.Contents != null)
                {
                    fileData.Contents = Utilities.TextMimeTypes.Contains(v2FileData.MimeType) ?
                        SarifUtilities.GetUtf8Base64String(v2FileData.Contents.Text) :
                        v2FileData.Contents.Binary;
                }

                if (v2FileData.Hashes != null)
                {
                    fileData.Hashes = new List<HashVersionOne>();

                    foreach (Hash hash in v2FileData.Hashes)
                    {
                        fileData.Hashes.Add(CreateHash(hash));
                    }
                }

                if (v2FileData.Tags.Count > 0)
                {
                    fileData.Tags.UnionWith(v2FileData.Tags);
                }
            }

            return fileData;
        }

        public HashVersionOne CreateHash(Hash v2Hash)
        {
            HashVersionOne hash = null;

            if (v2Hash != null)
            {
                AlgorithmKindVersionOne algorithm;
                if (!Utilities.AlgorithmNameKindMap.TryGetValue(v2Hash.Algorithm, out algorithm))
                {
                    algorithm = AlgorithmKindVersionOne.Unknown;
                }

                hash = new HashVersionOne
                {
                    Algorithm = algorithm,
                    Value = v2Hash.Value
                };
            }

            return hash;
        }

        public LogicalLocationVersionOne CreateLogicalLocationVersionOne(LogicalLocation v2LogicalLocatiom)
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

        public RuleVersionOne CreateRule(Rule v2Rule)
        {
            RuleVersionOne rule = null;

            if (v2Rule != null)
            {
                rule = new RuleVersionOne
                {
                    Id = v2Rule.Id,
                    MessageFormats = v2Rule.MessageStrings,
                    Properties = v2Rule.Properties
                };

                if (v2Rule.Configuration != null)
                {
                    rule.Configuration = v2Rule.Configuration.Enabled ?
                            RuleConfigurationVersionOne.Enabled :
                            RuleConfigurationVersionOne.Disabled;
                    rule.DefaultLevel = Utilities.CreateResultLevelVersionOne(v2Rule.Configuration.DefaultLevel);
                }

                if (v2Rule.Name != null)
                {
                    rule.Name = v2Rule.Name.Text;
                }

                if (v2Rule.FullDescription != null)
                {
                    rule.FullDescription = v2Rule.FullDescription.Text;
                }

                if (v2Rule.ShortDescription != null)
                {
                    rule.ShortDescription = v2Rule.ShortDescription.Text;
                }

                if (v2Rule.HelpLocation != null)
                {
                    rule.HelpUri = v2Rule.HelpLocation.Uri;
                }
            }

            return rule;
        }

        public RunVersionOne CreateRun(Run v2Run)
        {
            RunVersionOne run = null;

            if (v2Run != null)
            {
                string serializedV1Run;

                if (v2Run.TryGetProperty("sarifv1/run", out serializedV1Run))
                {
                    run = JsonConvert.DeserializeObject<RunVersionOne>(serializedV1Run, Utilities.JsonSettingsV1);
                }
                else
                {
                    run = new RunVersionOne()
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

                    _currentRun = run;

                    if (v2Run.Files != null)
                    {
                        run.Files = new Dictionary<string, FileDataVersionOne>();

                        foreach (var pair in v2Run.Files)
                        {
                            run.Files.Add(pair.Key, CreateFileDataVersionOne(pair.Value));
                        }
                    }

                    if (v2Run.LogicalLocations != null)
                    {
                        run.LogicalLocations = new Dictionary<string, LogicalLocationVersionOne>();

                        foreach (var pair in v2Run.LogicalLocations)
                        {
                            run.LogicalLocations.Add(pair.Key, CreateLogicalLocationVersionOne(pair.Value));
                        }
                    }

                    if (v2Run.Resources?.Rules != null)
                    {
                        run.Rules = new Dictionary<string, RuleVersionOne>();

                        foreach (var pair in v2Run.Resources.Rules)
                        {
                            run.Rules.Add(pair.Key, CreateRule(pair.Value));
                        }
                    }

                    // Stash the entire v2 run in this v1 run's property bag
                    run.SetProperty($"{FromPropertyBagPrefix}/run", JsonConvert.SerializeObject(v2Run, Utilities.JsonSettingsV2));
                }
            }

            return run;
        }

        public ToolVersionOne CreateTool(Tool v2Tool)
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
            }

            return tool;
        }
    }
}
