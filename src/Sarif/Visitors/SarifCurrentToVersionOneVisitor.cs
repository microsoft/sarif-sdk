// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifCurrentToVersionOneVisitor : SarifRewritingVisitor
    {
        private static readonly SarifVersion FromSarifVersion = SarifVersion.TwoZeroZero;
        private static readonly SarifVersion ToSarifVersion = SarifVersion.OneZeroZero;
        private static readonly string FromPropertyBagPrefix = 
            SarifTransformerUtilities.PropertyBagTransformerItemPrefixes[FromSarifVersion];
        private static readonly string ToPropertyBagPrefix =
            SarifTransformerUtilities.PropertyBagTransformerItemPrefixes[SarifVersion.OneZeroZero];

        public SarifLogVersionOne SarifLogVersionOne { get; private set; }

        public override SarifLog VisitSarifLog(SarifLog v2SarifLog)
        {
            SarifLogVersionOne = new SarifLogVersionOne(SarifVersionVersionOne.OneZeroZero.ConvertToSchemaUri(),
                                    SarifVersionVersionOne.OneZeroZero,
                                    new List<RunVersionOne>());

            foreach (Run run in v2SarifLog.Runs)
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
                    AutomationId = v2Run.AutomationId,
                    BaselineId = v2Run.BaselineId,
                    Id = v2Run.Id,
                    Properties = v2Run.Properties,
                    Results = new List<ResultVersionOne>(),
                    StableId = v2Run.StableId,
                    Tool = CreateTool(v2Run.Tool)
                };

                SarifLogVersionOne.Runs.Add(run);

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

                if (v2Run.Tags.Count > 0)
                {
                    run.Tags.UnionWith(v2Run.Tags);
                }
            }

            return null;
        }

        public static FileDataVersionOne CreateFileDataVersionOne(FileData v2FileData)
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
                    Properties = v2FileData.Properties ?? new Dictionary<string, SerializedPropertyInfo>()
                };

                if (v2FileData.FileLocation != null)
                {
                    fileData.Uri = v2FileData.FileLocation.Uri;
                    fileData.UriBaseId = v2FileData.FileLocation.UriBaseId;
                }

                fileData.Contents = SarifTransformerUtilities.TextMimeTypes.Contains(v2FileData.MimeType) ?
                    SarifUtilities.GetUtf8Base64String(v2FileData.Contents.Text) :
                    v2FileData.Contents.Binary;

                if (v2FileData.Hashes != null)
                {
                    fileData.Hashes = new List<HashVersionOne>();

                    foreach (Hash hash in v2FileData.Hashes)
                    {
                        fileData.Hashes.Add(CreateHash(hash));
                    }
                }

                if (!string.IsNullOrWhiteSpace(v2FileData.Encoding))
                {
                    fileData.SetProperty($"{FromPropertyBagPrefix}/encoding", v2FileData.Encoding);
                }

                if (!v2FileData.Roles.HasFlag(FileRoles.None))
                {
                    fileData.SetProperty($"{FromPropertyBagPrefix}/roles", v2FileData.Roles);
                }

                if (v2FileData.Tags.Count > 0)
                {
                    fileData.Tags.UnionWith(v2FileData.Tags);
                }

                if (fileData.Properties != null && fileData.Properties.Count == 0)
                {
                    fileData.Properties = null;
                }
                else
                {
                    // Remove any transformer compatibility property bag items
                    // that were added by a previous transformer
                    SarifTransformerUtilities.RemoveSarifPropertyBagItems(fileData, ToSarifVersion);
                }
            }

            return fileData;
        }

        public static HashVersionOne CreateHash(Hash v2Hash)
        {
            HashVersionOne hash = null;

            if (v2Hash != null)
            {
                AlgorithmKindVersionOne algorithm;
                if (!SarifTransformerUtilities.AlgorithmNameKindMap.TryGetValue(v2Hash.Algorithm, out algorithm))
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

        public static RuleVersionOne CreateRule(Rule v2Rule)
        {
            RuleVersionOne rule = null;

            if (v2Rule != null)
            {
                rule = new RuleVersionOne
                {
                    Id = v2Rule.Id,
                    MessageFormats = v2Rule.MessageStrings,
                    Properties = v2Rule.Properties ?? new Dictionary<string, SerializedPropertyInfo>()
                };

                if (v2Rule.Configuration != null)
                {
                    rule.Configuration = v2Rule.Configuration.Enabled ?
                            RuleConfigurationVersionOne.Enabled :
                            RuleConfigurationVersionOne.Disabled;
                    rule.DefaultLevel = SarifTransformerUtilities.CreateResultLevelVersionOne(v2Rule.Configuration.DefaultLevel);

                    if (v2Rule.Configuration.Parameters != null)
                    {
                        v2Rule.SetProperty("sarifv2/configuration.parameters", v2Rule.Configuration.Parameters);
                    }
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

                    if (!string.IsNullOrWhiteSpace(v2Rule.HelpLocation.UriBaseId))
                    {
                        v2Rule.SetProperty("sarifv2/helplocation.uribaseid", v2Rule.Configuration.Parameters);
                    }
                }

                if (v2Rule.Tags.Count > 0)
                {
                    rule.Tags.UnionWith(v2Rule.Tags);
                }

                if (rule.Properties != null && rule.Properties.Count == 0)
                {
                    rule.Properties = null;
                }
            }

            return rule;
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
