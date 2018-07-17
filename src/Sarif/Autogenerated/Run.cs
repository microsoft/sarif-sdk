// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes a single run of an analysis tool, and contains the output of that run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.55.0.0")]
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
        public IList<Invocation> Invocations { get; set; }

        /// <summary>
        /// A conversion object that describes how a converter transformed an analysis tool's native output format into the SARIF format.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public Conversion Conversion { get; set; }

        /// <summary>
        /// Specifies the revision in version control of the files that were scanned.
        /// </summary>
        [DataMember(Name = "versionControlProvenance", IsRequired = false, EmitDefaultValue = false)]
        public IList<VersionControlDetails> VersionControlProvenance { get; set; }

        /// <summary>
        /// The absolute URI specified by each uriBaseId symbol on the machine where the tool originally ran.
        /// </summary>
        [DataMember(Name = "originalUriBaseIds", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, Uri> OriginalUriBaseIds { get; set; }

        /// <summary>
        /// A dictionary each of whose keys is a URI and each of whose values is a file object.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, FileData> Files { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a logical location such as a namespace, type or function.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, LogicalLocation> LogicalLocations { get; set; }

        /// <summary>
        /// An array of one or more unique 'graph' objects.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        public IList<Graph> Graphs { get; set; }

        /// <summary>
        /// The set of results contained in an SARIF log. The results array can be omitted when a run is solely exporting rules metadata. It must be present (but may be empty) in the event that a log file represents an actual scan.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<Result> Results { get; set; }

        /// <summary>
        /// Items that can be localized, such as message strings and rule metadata.
        /// </summary>
        [DataMember(Name = "resources", IsRequired = false, EmitDefaultValue = false)]
        public Resources Resources { get; set; }

        /// <summary>
        /// A stable, unique identifier for the run, in the form of a GUID.
        /// </summary>
        [DataMember(Name = "instanceGuid", IsRequired = false, EmitDefaultValue = false)]
        public string InstanceGuid { get; set; }

        /// <summary>
        /// A logical identifier for a run, for example, 'nightly Clang analyzer run'. Multiple runs of the same type can have the same stableId.
        /// </summary>
        [DataMember(Name = "logicalId", IsRequired = false, EmitDefaultValue = false)]
        public string LogicalId { get; set; }

        /// <summary>
        /// A description of the run.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public Message Description { get; set; }

        /// <summary>
        /// A global identifier that allows the run to be correlated with other artifacts produced by a larger automation process.
        /// </summary>
        [DataMember(Name = "automationLogicalId", IsRequired = false, EmitDefaultValue = false)]
        public string AutomationLogicalId { get; set; }

        /// <summary>
        /// The 'instanceGuid' property of a previous SARIF 'run' that comprises the baseline that was used to compute result 'baselineState' properties for the run.
        /// </summary>
        [DataMember(Name = "baselineInstanceGuid", IsRequired = false, EmitDefaultValue = false)]
        public string BaselineInstanceGuid { get; set; }

        /// <summary>
        /// The hardware architecture for which the run was targeted.
        /// </summary>
        [DataMember(Name = "architecture", IsRequired = false, EmitDefaultValue = false)]
        public string Architecture { get; set; }

        /// <summary>
        /// The MIME type of all rich text message properties in the run. Default: "text/markdown;variant=GFM"
        /// </summary>
        [DataMember(Name = "richMessageMimeType", IsRequired = false, EmitDefaultValue = false)]
        public string RichMessageMimeType { get; set; }

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
        /// Specifies the unit in which the tool measures columns.
        /// </summary>
        [DataMember(Name = "columnKind", IsRequired = false, EmitDefaultValue = false)]
        public ColumnKind ColumnKind { get; set; }

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Run" /> class from the supplied values.
        /// </summary>
        /// <param name="tool">
        /// An initialization value for the <see cref="P: Tool" /> property.
        /// </param>
        /// <param name="invocations">
        /// An initialization value for the <see cref="P: Invocations" /> property.
        /// </param>
        /// <param name="conversion">
        /// An initialization value for the <see cref="P: Conversion" /> property.
        /// </param>
        /// <param name="versionControlProvenance">
        /// An initialization value for the <see cref="P: VersionControlProvenance" /> property.
        /// </param>
        /// <param name="originalUriBaseIds">
        /// An initialization value for the <see cref="P: OriginalUriBaseIds" /> property.
        /// </param>
        /// <param name="files">
        /// An initialization value for the <see cref="P: Files" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P: LogicalLocations" /> property.
        /// </param>
        /// <param name="graphs">
        /// An initialization value for the <see cref="P: Graphs" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P: Results" /> property.
        /// </param>
        /// <param name="resources">
        /// An initialization value for the <see cref="P: Resources" /> property.
        /// </param>
        /// <param name="instanceGuid">
        /// An initialization value for the <see cref="P: InstanceGuid" /> property.
        /// </param>
        /// <param name="logicalId">
        /// An initialization value for the <see cref="P: LogicalId" /> property.
        /// </param>
        /// <param name="description">
        /// An initialization value for the <see cref="P: Description" /> property.
        /// </param>
        /// <param name="automationLogicalId">
        /// An initialization value for the <see cref="P: AutomationLogicalId" /> property.
        /// </param>
        /// <param name="baselineInstanceGuid">
        /// An initialization value for the <see cref="P: BaselineInstanceGuid" /> property.
        /// </param>
        /// <param name="architecture">
        /// An initialization value for the <see cref="P: Architecture" /> property.
        /// </param>
        /// <param name="richMessageMimeType">
        /// An initialization value for the <see cref="P: RichMessageMimeType" /> property.
        /// </param>
        /// <param name="redactionToken">
        /// An initialization value for the <see cref="P: RedactionToken" /> property.
        /// </param>
        /// <param name="defaultFileEncoding">
        /// An initialization value for the <see cref="P: DefaultFileEncoding" /> property.
        /// </param>
        /// <param name="columnKind">
        /// An initialization value for the <see cref="P: ColumnKind" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Run(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, IEnumerable<VersionControlDetails> versionControlProvenance, IDictionary<string, Uri> originalUriBaseIds, IDictionary<string, FileData> files, IDictionary<string, LogicalLocation> logicalLocations, IEnumerable<Graph> graphs, IEnumerable<Result> results, Resources resources, string instanceGuid, string logicalId, Message description, string automationLogicalId, string baselineInstanceGuid, string architecture, string richMessageMimeType, string redactionToken, string defaultFileEncoding, ColumnKind columnKind, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(tool, invocations, conversion, versionControlProvenance, originalUriBaseIds, files, logicalLocations, graphs, results, resources, instanceGuid, logicalId, description, automationLogicalId, baselineInstanceGuid, architecture, richMessageMimeType, redactionToken, defaultFileEncoding, columnKind, properties);
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

            Init(other.Tool, other.Invocations, other.Conversion, other.VersionControlProvenance, other.OriginalUriBaseIds, other.Files, other.LogicalLocations, other.Graphs, other.Results, other.Resources, other.InstanceGuid, other.LogicalId, other.Description, other.AutomationLogicalId, other.BaselineInstanceGuid, other.Architecture, other.RichMessageMimeType, other.RedactionToken, other.DefaultFileEncoding, other.ColumnKind, other.Properties);
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

        private void Init(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, IEnumerable<VersionControlDetails> versionControlProvenance, IDictionary<string, Uri> originalUriBaseIds, IDictionary<string, FileData> files, IDictionary<string, LogicalLocation> logicalLocations, IEnumerable<Graph> graphs, IEnumerable<Result> results, Resources resources, string instanceGuid, string logicalId, Message description, string automationLogicalId, string baselineInstanceGuid, string architecture, string richMessageMimeType, string redactionToken, string defaultFileEncoding, ColumnKind columnKind, IDictionary<string, SerializedPropertyInfo> properties)
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
            };
            if (files != null)
            {
                Files = new Dictionary<string, FileData>();
                foreach (var value_2 in files)
                {
                    Files.Add(value_2.Key, new FileData(value_2.Value));
                }
            }

            if (logicalLocations != null)
            {
                LogicalLocations = new Dictionary<string, LogicalLocation>();
                foreach (var value_3 in logicalLocations)
                {
                    LogicalLocations.Add(value_3.Key, new LogicalLocation(value_3.Value));
                }
            }

            if (graphs != null)
            {
                var destination_2 = new List<Graph>();
                foreach (var value_4 in graphs)
                {
                    if (value_4 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new Graph(value_4));
                    }
                }

                Graphs = destination_2;
            }

            if (results != null)
            {
                var destination_3 = new List<Result>();
                foreach (var value_5 in results)
                {
                    if (value_5 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new Result(value_5));
                    }
                }

                Results = destination_3;
            }

            if (resources != null)
            {
                Resources = new Resources(resources);
            }

            InstanceGuid = instanceGuid;
            LogicalId = logicalId;
            if (description != null)
            {
                Description = new Message(description);
            }

            AutomationLogicalId = automationLogicalId;
            BaselineInstanceGuid = baselineInstanceGuid;
            Architecture = architecture;
            RichMessageMimeType = richMessageMimeType;
            RedactionToken = redactionToken;
            DefaultFileEncoding = defaultFileEncoding;
            ColumnKind = columnKind;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}