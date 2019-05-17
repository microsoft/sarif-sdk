// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes a single run of an analysis tool, and contains the reported output of that run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Run : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Run> ValueComparer => RunEqualityComparer.Instance;

        public bool ValueEquals(Run other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Run;
            }
        }

        /// <summary>
        /// Information about the tool or tool pipeline that generated the results in this run. A run can only contain results produced by a single tool or tool pipeline. A run can aggregate results from multiple log files, as long as context around the tool run (tool command-line arguments and the like) is identical for all aggregated files.
        /// </summary>
        [DataMember(Name = "tool", IsRequired = true)]
        public Tool Tool { get; set; }

        /// <summary>
        /// Describes the invocation of the analysis tool.
        /// </summary>
        [DataMember(Name = "invocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Invocation> Invocations { get; set; }

        /// <summary>
        /// A conversion object that describes how a converter transformed an analysis tool's native reporting format into the SARIF format.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public Conversion Conversion { get; set; }

        /// <summary>
        /// The language of the messages emitted into the log file during this run (expressed as an ISO 639-1 two-letter lowercase culture code) and an optional region (expressed as an ISO 3166-1 two-letter uppercase subculture code associated with a country or region). The casing is recommended but not required (in order for this data to conform to RFC5646).
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("en-US")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Language { get; set; }

        /// <summary>
        /// Specifies the revision in version control of the artifacts that were scanned.
        /// </summary>
        [DataMember(Name = "versionControlProvenance", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<VersionControlDetails> VersionControlProvenance { get; set; }

        /// <summary>
        /// The artifact location specified by each uriBaseId symbol on the machine where the tool originally ran.
        /// </summary>
        [DataMember(Name = "originalUriBaseIds", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, ArtifactLocation> OriginalUriBaseIds { get; set; }

        /// <summary>
        /// An array of artifact objects relevant to the run.
        /// </summary>
        [DataMember(Name = "artifacts", IsRequired = false, EmitDefaultValue = false)]
        public IList<Artifact> Artifacts { get; set; }

        /// <summary>
        /// An array of logical locations such as namespaces, types or functions.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<LogicalLocation> LogicalLocations { get; set; }

        /// <summary>
        /// An array of zero or more unique graph objects associated with the run.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Graph> Graphs { get; set; }

        /// <summary>
        /// The set of results contained in an SARIF log. The results array can be omitted when a run is solely exporting rules metadata. It must be present (but may be empty) if a log file represents an actual scan.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<Result> Results { get; set; }

        /// <summary>
        /// Automation details that describe this run.
        /// </summary>
        [DataMember(Name = "automationDetails", IsRequired = false, EmitDefaultValue = false)]
        public RunAutomationDetails AutomationDetails { get; set; }

        /// <summary>
        /// Automation details that describe the aggregate of runs to which this run belongs.
        /// </summary>
        [DataMember(Name = "runAggregates", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<RunAutomationDetails> RunAggregates { get; set; }

        /// <summary>
        /// The 'guid' property of a previous SARIF 'run' that comprises the baseline that was used to compute result 'baselineState' properties for the run.
        /// </summary>
        [DataMember(Name = "baselineGuid", IsRequired = false, EmitDefaultValue = false)]
        public string BaselineGuid { get; set; }

        /// <summary>
        /// An array of strings used to replace sensitive information in a redaction-aware property.
        /// </summary>
        [DataMember(Name = "redactionTokens", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<string> RedactionTokens { get; set; }

        /// <summary>
        /// Specifies the default encoding for any artifact object that refers to a text file.
        /// </summary>
        [DataMember(Name = "defaultEncoding", IsRequired = false, EmitDefaultValue = false)]
        public string DefaultEncoding { get; set; }

        /// <summary>
        /// Specifies the default source language for any artifact object that refers to a text file that contains source code.
        /// </summary>
        [DataMember(Name = "defaultSourceLanguage", IsRequired = false, EmitDefaultValue = false)]
        public string DefaultSourceLanguage { get; set; }

        /// <summary>
        /// An ordered list of character sequences that were treated as line breaks when computing region information for the run.
        /// </summary>
        [DataMember(Name = "newlineSequences", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<string> NewlineSequences { get; set; }

        /// <summary>
        /// Specifies the unit in which the tool measures columns.
        /// </summary>
        [DataMember(Name = "columnKind", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.EnumConverter))]
        public ColumnKind ColumnKind { get; set; }

        /// <summary>
        /// References to external property files that should be inlined with the content of a root log file.
        /// </summary>
        [DataMember(Name = "externalPropertyFileReferences", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFileReferences ExternalPropertyFileReferences { get; set; }

        /// <summary>
        /// An array of threadFlowLocation objects cached at run level.
        /// </summary>
        [DataMember(Name = "threadFlowLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ThreadFlowLocation> ThreadFlowLocations { get; set; }

        /// <summary>
        /// An array of toolComponent objects relevant to a taxonomy in which results are categorized.
        /// </summary>
        [DataMember(Name = "taxonomies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ToolComponent> Taxonomies { get; set; }

        /// <summary>
        /// Addresses associated with this run instance, if any.
        /// </summary>
        [DataMember(Name = "addresses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Address> Addresses { get; set; }

        /// <summary>
        /// The set of available translations of the localized data provided by the tool.
        /// </summary>
        [DataMember(Name = "translations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ToolComponent> Translations { get; set; }

        /// <summary>
        /// Contains configurations that may potentially override both reportingDescriptor.defaultConfiguration (the tool's default severities) and invocation.configurationOverrides (severities established at run-time from the command line).
        /// </summary>
        [DataMember(Name = "policies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ToolComponent> Policies { get; set; }

        /// <summary>
        /// An array of request objects cached at run level.
        /// </summary>
        [DataMember(Name = "webRequests", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<WebRequest> WebRequests { get; set; }

        /// <summary>
        /// An array of response objects cached at run level.
        /// </summary>
        [DataMember(Name = "webResponses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<WebResponse> WebResponses { get; set; }

        /// <summary>
        /// A specialLocations object that defines locations of special significance to SARIF consumers.
        /// </summary>
        [DataMember(Name = "specialLocations", IsRequired = false, EmitDefaultValue = false)]
        public SpecialLocations SpecialLocations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Run" /> class.
        /// </summary>
        public Run()
        {
            Language = "en-US";
            // NOTYETAUTOGENERATED:
            // https://github.com/Microsoft/jschema/issues/92
            // https://github.com/Microsoft/jschema/issues/95
            // 
            ColumnKind = ColumnKind.Utf16CodeUnits;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Run" /> class from the supplied values.
        /// </summary>
        /// <param name="tool">
        /// An initialization value for the <see cref="P:Tool" /> property.
        /// </param>
        /// <param name="invocations">
        /// An initialization value for the <see cref="P:Invocations" /> property.
        /// </param>
        /// <param name="conversion">
        /// An initialization value for the <see cref="P:Conversion" /> property.
        /// </param>
        /// <param name="language">
        /// An initialization value for the <see cref="P:Language" /> property.
        /// </param>
        /// <param name="versionControlProvenance">
        /// An initialization value for the <see cref="P:VersionControlProvenance" /> property.
        /// </param>
        /// <param name="originalUriBaseIds">
        /// An initialization value for the <see cref="P:OriginalUriBaseIds" /> property.
        /// </param>
        /// <param name="artifacts">
        /// An initialization value for the <see cref="P:Artifacts" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P:LogicalLocations" /> property.
        /// </param>
        /// <param name="graphs">
        /// An initialization value for the <see cref="P:Graphs" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P:Results" /> property.
        /// </param>
        /// <param name="automationDetails">
        /// An initialization value for the <see cref="P:AutomationDetails" /> property.
        /// </param>
        /// <param name="runAggregates">
        /// An initialization value for the <see cref="P:RunAggregates" /> property.
        /// </param>
        /// <param name="baselineGuid">
        /// An initialization value for the <see cref="P:BaselineGuid" /> property.
        /// </param>
        /// <param name="redactionTokens">
        /// An initialization value for the <see cref="P:RedactionTokens" /> property.
        /// </param>
        /// <param name="defaultEncoding">
        /// An initialization value for the <see cref="P:DefaultEncoding" /> property.
        /// </param>
        /// <param name="defaultSourceLanguage">
        /// An initialization value for the <see cref="P:DefaultSourceLanguage" /> property.
        /// </param>
        /// <param name="newlineSequences">
        /// An initialization value for the <see cref="P:NewlineSequences" /> property.
        /// </param>
        /// <param name="columnKind">
        /// An initialization value for the <see cref="P:ColumnKind" /> property.
        /// </param>
        /// <param name="externalPropertyFileReferences">
        /// An initialization value for the <see cref="P:ExternalPropertyFileReferences" /> property.
        /// </param>
        /// <param name="threadFlowLocations">
        /// An initialization value for the <see cref="P:ThreadFlowLocations" /> property.
        /// </param>
        /// <param name="taxonomies">
        /// An initialization value for the <see cref="P:Taxonomies" /> property.
        /// </param>
        /// <param name="addresses">
        /// An initialization value for the <see cref="P:Addresses" /> property.
        /// </param>
        /// <param name="translations">
        /// An initialization value for the <see cref="P:Translations" /> property.
        /// </param>
        /// <param name="policies">
        /// An initialization value for the <see cref="P:Policies" /> property.
        /// </param>
        /// <param name="webRequests">
        /// An initialization value for the <see cref="P:WebRequests" /> property.
        /// </param>
        /// <param name="webResponses">
        /// An initialization value for the <see cref="P:WebResponses" /> property.
        /// </param>
        /// <param name="specialLocations">
        /// An initialization value for the <see cref="P:SpecialLocations" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Run(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, string language, IEnumerable<VersionControlDetails> versionControlProvenance, IDictionary<string, ArtifactLocation> originalUriBaseIds, IEnumerable<Artifact> artifacts, IEnumerable<LogicalLocation> logicalLocations, IEnumerable<Graph> graphs, IEnumerable<Result> results, RunAutomationDetails automationDetails, IEnumerable<RunAutomationDetails> runAggregates, string baselineGuid, IEnumerable<string> redactionTokens, string defaultEncoding, string defaultSourceLanguage, IEnumerable<string> newlineSequences, ColumnKind columnKind, ExternalPropertyFileReferences externalPropertyFileReferences, IEnumerable<ThreadFlowLocation> threadFlowLocations, IEnumerable<ToolComponent> taxonomies, IEnumerable<Address> addresses, IEnumerable<ToolComponent> translations, IEnumerable<ToolComponent> policies, IEnumerable<WebRequest> webRequests, IEnumerable<WebResponse> webResponses, SpecialLocations specialLocations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(tool, invocations, conversion, language, versionControlProvenance, originalUriBaseIds, artifacts, logicalLocations, graphs, results, automationDetails, runAggregates, baselineGuid, redactionTokens, defaultEncoding, defaultSourceLanguage, newlineSequences, columnKind, externalPropertyFileReferences, threadFlowLocations, taxonomies, addresses, translations, policies, webRequests, webResponses, specialLocations, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Run" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Run(Run other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Tool, other.Invocations, other.Conversion, other.Language, other.VersionControlProvenance, other.OriginalUriBaseIds, other.Artifacts, other.LogicalLocations, other.Graphs, other.Results, other.AutomationDetails, other.RunAggregates, other.BaselineGuid, other.RedactionTokens, other.DefaultEncoding, other.DefaultSourceLanguage, other.NewlineSequences, other.ColumnKind, other.ExternalPropertyFileReferences, other.ThreadFlowLocations, other.Taxonomies, other.Addresses, other.Translations, other.Policies, other.WebRequests, other.WebResponses, other.SpecialLocations, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Run DeepClone()
        {
            return (Run)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Run(this);
        }

        private void Init(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, string language, IEnumerable<VersionControlDetails> versionControlProvenance, IDictionary<string, ArtifactLocation> originalUriBaseIds, IEnumerable<Artifact> artifacts, IEnumerable<LogicalLocation> logicalLocations, IEnumerable<Graph> graphs, IEnumerable<Result> results, RunAutomationDetails automationDetails, IEnumerable<RunAutomationDetails> runAggregates, string baselineGuid, IEnumerable<string> redactionTokens, string defaultEncoding, string defaultSourceLanguage, IEnumerable<string> newlineSequences, ColumnKind columnKind, ExternalPropertyFileReferences externalPropertyFileReferences, IEnumerable<ThreadFlowLocation> threadFlowLocations, IEnumerable<ToolComponent> taxonomies, IEnumerable<Address> addresses, IEnumerable<ToolComponent> translations, IEnumerable<ToolComponent> policies, IEnumerable<WebRequest> webRequests, IEnumerable<WebResponse> webResponses, SpecialLocations specialLocations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (tool != null)
            {
                Tool = new Tool(tool);
            }

            if (invocations != null)
            {
                var destination_0 = new List<Invocation>();
                foreach (var value_0 in invocations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Invocation(value_0));
                    }
                }

                Invocations = destination_0;
            }

            if (conversion != null)
            {
                Conversion = new Conversion(conversion);
            }

            Language = language;
            if (versionControlProvenance != null)
            {
                var destination_1 = new List<VersionControlDetails>();
                foreach (var value_1 in versionControlProvenance)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new VersionControlDetails(value_1));
                    }
                }

                VersionControlProvenance = destination_1;
            }

            if (originalUriBaseIds != null)
            {
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>();
                foreach (var value_2 in originalUriBaseIds)
                {
                    OriginalUriBaseIds.Add(value_2.Key, new ArtifactLocation(value_2.Value));
                }
            }

            if (artifacts != null)
            {
                var destination_2 = new List<Artifact>();
                foreach (var value_3 in artifacts)
                {
                    if (value_3 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new Artifact(value_3));
                    }
                }

                Artifacts = destination_2;
            }

            if (logicalLocations != null)
            {
                var destination_3 = new List<LogicalLocation>();
                foreach (var value_4 in logicalLocations)
                {
                    if (value_4 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new LogicalLocation(value_4));
                    }
                }

                LogicalLocations = destination_3;
            }

            if (graphs != null)
            {
                var destination_4 = new List<Graph>();
                foreach (var value_5 in graphs)
                {
                    if (value_5 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Graph(value_5));
                    }
                }

                Graphs = destination_4;
            }

            if (results != null)
            {
                var destination_5 = new List<Result>();
                foreach (var value_6 in results)
                {
                    if (value_6 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new Result(value_6));
                    }
                }

                Results = destination_5;
            }

            if (automationDetails != null)
            {
                AutomationDetails = new RunAutomationDetails(automationDetails);
            }

            if (runAggregates != null)
            {
                var destination_6 = new List<RunAutomationDetails>();
                foreach (var value_7 in runAggregates)
                {
                    if (value_7 == null)
                    {
                        destination_6.Add(null);
                    }
                    else
                    {
                        destination_6.Add(new RunAutomationDetails(value_7));
                    }
                }

                RunAggregates = destination_6;
            }

            BaselineGuid = baselineGuid;
            if (redactionTokens != null)
            {
                var destination_7 = new List<string>();
                foreach (var value_8 in redactionTokens)
                {
                    destination_7.Add(value_8);
                }

                RedactionTokens = destination_7;
            }

            DefaultEncoding = defaultEncoding;
            DefaultSourceLanguage = defaultSourceLanguage;
            if (newlineSequences != null)
            {
                var destination_8 = new List<string>();
                foreach (var value_9 in newlineSequences)
                {
                    destination_8.Add(value_9);
                }

                NewlineSequences = destination_8;
            }

            ColumnKind = columnKind;
            if (externalPropertyFileReferences != null)
            {
                ExternalPropertyFileReferences = new ExternalPropertyFileReferences(externalPropertyFileReferences);
            }

            if (threadFlowLocations != null)
            {
                var destination_9 = new List<ThreadFlowLocation>();
                foreach (var value_10 in threadFlowLocations)
                {
                    if (value_10 == null)
                    {
                        destination_9.Add(null);
                    }
                    else
                    {
                        destination_9.Add(new ThreadFlowLocation(value_10));
                    }
                }

                ThreadFlowLocations = destination_9;
            }

            if (taxonomies != null)
            {
                var destination_10 = new List<ToolComponent>();
                foreach (var value_11 in taxonomies)
                {
                    if (value_11 == null)
                    {
                        destination_10.Add(null);
                    }
                    else
                    {
                        destination_10.Add(new ToolComponent(value_11));
                    }
                }

                Taxonomies = destination_10;
            }

            if (addresses != null)
            {
                var destination_11 = new List<Address>();
                foreach (var value_12 in addresses)
                {
                    if (value_12 == null)
                    {
                        destination_11.Add(null);
                    }
                    else
                    {
                        destination_11.Add(new Address(value_12));
                    }
                }

                Addresses = destination_11;
            }

            if (translations != null)
            {
                var destination_12 = new List<ToolComponent>();
                foreach (var value_13 in translations)
                {
                    if (value_13 == null)
                    {
                        destination_12.Add(null);
                    }
                    else
                    {
                        destination_12.Add(new ToolComponent(value_13));
                    }
                }

                Translations = destination_12;
            }

            if (policies != null)
            {
                var destination_13 = new List<ToolComponent>();
                foreach (var value_14 in policies)
                {
                    if (value_14 == null)
                    {
                        destination_13.Add(null);
                    }
                    else
                    {
                        destination_13.Add(new ToolComponent(value_14));
                    }
                }

                Policies = destination_13;
            }

            if (webRequests != null)
            {
                var destination_14 = new List<WebRequest>();
                foreach (var value_15 in webRequests)
                {
                    if (value_15 == null)
                    {
                        destination_14.Add(null);
                    }
                    else
                    {
                        destination_14.Add(new WebRequest(value_15));
                    }
                }

                WebRequests = destination_14;
            }

            if (webResponses != null)
            {
                var destination_15 = new List<WebResponse>();
                foreach (var value_16 in webResponses)
                {
                    if (value_16 == null)
                    {
                        destination_15.Add(null);
                    }
                    else
                    {
                        destination_15.Add(new WebResponse(value_16));
                    }
                }

                WebResponses = destination_15;
            }

            if (specialLocations != null)
            {
                SpecialLocations = new SpecialLocations(specialLocations);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}