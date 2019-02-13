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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
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
        /// Specifies the revision in version control of the files that were scanned.
        /// </summary>
        [DataMember(Name = "versionControlProvenance", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<VersionControlDetails> VersionControlProvenance { get; set; }

        /// <summary>
        /// The file location specified by each uriBaseId symbol on the machine where the tool originally ran.
        /// </summary>
        [DataMember(Name = "originalUriBaseIds", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, FileLocation> OriginalUriBaseIds { get; set; }

        /// <summary>
        /// An array of file objects relevant to the run.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IList<FileData> Files { get; set; }

        /// <summary>
        /// An array of logical locations such as namespaces, types or functions.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<LogicalLocation> LogicalLocations { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is the id of a graph and each of whose values is a 'graph' object with that id.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, Graph> Graphs { get; set; }

        /// <summary>
        /// The set of results contained in an SARIF log. The results array can be omitted when a run is solely exporting rules metadata. It must be present (but may be empty) if a log file represents an actual scan.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<Result> Results { get; set; }

        /// <summary>
        /// Automation details that describe this run.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public RunAutomationDetails Id { get; set; }

        /// <summary>
        /// Automation details that describe the aggregate of runs to which this run belongs.
        /// </summary>
        [DataMember(Name = "aggregateIds", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<RunAutomationDetails> AggregateIds { get; set; }

        /// <summary>
        /// The 'instanceGuid' property of a previous SARIF 'run' that comprises the baseline that was used to compute result 'baselineState' properties for the run.
        /// </summary>
        [DataMember(Name = "baselineInstanceGuid", IsRequired = false, EmitDefaultValue = false)]
        public string BaselineInstanceGuid { get; set; }

        /// <summary>
        /// The MIME type of all markdown text message properties in the run. Default: "text/markdown;variant=GFM"
        /// </summary>
        [DataMember(Name = "markdownMessageMimeType", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("text/markdown;variant=GFM")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string MarkdownMessageMimeType { get; set; }

        /// <summary>
        /// The string used to replace sensitive information in a redaction-aware property.
        /// </summary>
        [DataMember(Name = "redactionToken", IsRequired = false, EmitDefaultValue = false)]
        public string RedactionToken { get; set; }

        /// <summary>
        /// Specifies the default encoding for any file object that refers to a text file.
        /// </summary>
        [DataMember(Name = "defaultFileEncoding", IsRequired = false, EmitDefaultValue = false)]
        public string DefaultFileEncoding { get; set; }

        /// <summary>
        /// Specifies the default source language for any file object that refers to a text file that contains source code.
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
        [DefaultValue(ColumnKind.UnicodeCodePoints)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.EnumConverter))]
        public ColumnKind ColumnKind { get; set; }

        /// <summary>
        /// References to external property files that should be inlined with the content of a root log file.
        /// </summary>
        [DataMember(Name = "externalPropertyFiles", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFiles ExternalPropertyFiles { get; set; }

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
            MarkdownMessageMimeType = "text/markdown;variant=GFM";
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
        /// <param name="versionControlProvenance">
        /// An initialization value for the <see cref="P:VersionControlProvenance" /> property.
        /// </param>
        /// <param name="originalUriBaseIds">
        /// An initialization value for the <see cref="P:OriginalUriBaseIds" /> property.
        /// </param>
        /// <param name="files">
        /// An initialization value for the <see cref="P:Files" /> property.
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
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="aggregateIds">
        /// An initialization value for the <see cref="P:AggregateIds" /> property.
        /// </param>
        /// <param name="baselineInstanceGuid">
        /// An initialization value for the <see cref="P:BaselineInstanceGuid" /> property.
        /// </param>
        /// <param name="markdownMessageMimeType">
        /// An initialization value for the <see cref="P:MarkdownMessageMimeType" /> property.
        /// </param>
        /// <param name="redactionToken">
        /// An initialization value for the <see cref="P:RedactionToken" /> property.
        /// </param>
        /// <param name="defaultFileEncoding">
        /// An initialization value for the <see cref="P:DefaultFileEncoding" /> property.
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
        /// <param name="externalPropertyFiles">
        /// An initialization value for the <see cref="P:ExternalPropertyFiles" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Run(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, IEnumerable<VersionControlDetails> versionControlProvenance, IDictionary<string, FileLocation> originalUriBaseIds, IEnumerable<FileData> files, IEnumerable<LogicalLocation> logicalLocations, IDictionary<string, Graph> graphs, IEnumerable<Result> results, RunAutomationDetails id, IEnumerable<RunAutomationDetails> aggregateIds, string baselineInstanceGuid, string markdownMessageMimeType, string redactionToken, string defaultFileEncoding, string defaultSourceLanguage, IEnumerable<string> newlineSequences, ColumnKind columnKind, ExternalPropertyFiles externalPropertyFiles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(tool, invocations, conversion, versionControlProvenance, originalUriBaseIds, files, logicalLocations, graphs, results, id, aggregateIds, baselineInstanceGuid, markdownMessageMimeType, redactionToken, defaultFileEncoding, defaultSourceLanguage, newlineSequences, columnKind, externalPropertyFiles, properties);
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

            Init(other.Tool, other.Invocations, other.Conversion, other.VersionControlProvenance, other.OriginalUriBaseIds, other.Files, other.LogicalLocations, other.Graphs, other.Results, other.Id, other.AggregateIds, other.BaselineInstanceGuid, other.MarkdownMessageMimeType, other.RedactionToken, other.DefaultFileEncoding, other.DefaultSourceLanguage, other.NewlineSequences, other.ColumnKind, other.ExternalPropertyFiles, other.Properties);
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

        private void Init(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, IEnumerable<VersionControlDetails> versionControlProvenance, IDictionary<string, FileLocation> originalUriBaseIds, IEnumerable<FileData> files, IEnumerable<LogicalLocation> logicalLocations, IDictionary<string, Graph> graphs, IEnumerable<Result> results, RunAutomationDetails id, IEnumerable<RunAutomationDetails> aggregateIds, string baselineInstanceGuid, string markdownMessageMimeType, string redactionToken, string defaultFileEncoding, string defaultSourceLanguage, IEnumerable<string> newlineSequences, ColumnKind columnKind, ExternalPropertyFiles externalPropertyFiles, IDictionary<string, SerializedPropertyInfo> properties)
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
                OriginalUriBaseIds = new Dictionary<string, FileLocation>();
                foreach (var value_2 in originalUriBaseIds)
                {
                    OriginalUriBaseIds.Add(value_2.Key, new FileLocation(value_2.Value));
                }
            }

            if (files != null)
            {
                var destination_2 = new List<FileData>();
                foreach (var value_3 in files)
                {
                    if (value_3 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new FileData(value_3));
                    }
                }

                Files = destination_2;
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
                Graphs = new Dictionary<string, Graph>();
                foreach (var value_5 in graphs)
                {
                    Graphs.Add(value_5.Key, new Graph(value_5.Value));
                }
            }

            if (results != null)
            {
                var destination_4 = new List<Result>();
                foreach (var value_6 in results)
                {
                    if (value_6 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Result(value_6));
                    }
                }

                Results = destination_4;
            }

            if (id != null)
            {
                Id = new RunAutomationDetails(id);
            }

            if (aggregateIds != null)
            {
                var destination_5 = new List<RunAutomationDetails>();
                foreach (var value_7 in aggregateIds)
                {
                    if (value_7 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new RunAutomationDetails(value_7));
                    }
                }

                AggregateIds = destination_5;
            }

            BaselineInstanceGuid = baselineInstanceGuid;
            MarkdownMessageMimeType = markdownMessageMimeType;
            RedactionToken = redactionToken;
            DefaultFileEncoding = defaultFileEncoding;
            DefaultSourceLanguage = defaultSourceLanguage;
            if (newlineSequences != null)
            {
                var destination_6 = new List<string>();
                foreach (var value_8 in newlineSequences)
                {
                    destination_6.Add(value_8);
                }

                NewlineSequences = destination_6;
            }

            ColumnKind = columnKind;
            if (externalPropertyFiles != null)
            {
                ExternalPropertyFiles = new ExternalPropertyFiles(externalPropertyFiles);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}