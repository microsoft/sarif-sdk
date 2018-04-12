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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
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
        /// The absolute URI specified by each uriBaseId symbol on the machine where the tool originally ran.
        /// </summary>
        [DataMember(Name = "originalUriBaseIds", IsRequired = false, EmitDefaultValue = false)]
        public object OriginalUriBaseIds { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a URI and each of whose values is an array of file objects representing the location of a single file scanned during the run.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, FileData> Files { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a logical location such as a namespace, type or function.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, LogicalLocation> LogicalLocations { get; set; }

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
        /// An identifier for the run.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// A stable identifier for a run, for example, 'nightly Clang analyzer run'. Multiple runs of the same type can have the same stableId.
        /// </summary>
        [DataMember(Name = "stableId", IsRequired = false, EmitDefaultValue = false)]
        public string StableId { get; set; }

        /// <summary>
        /// A global identifier that allows the run to be correlated with other artifacts produced by a larger automation process.
        /// </summary>
        [DataMember(Name = "automationId", IsRequired = false, EmitDefaultValue = false)]
        public string AutomationId { get; set; }

        /// <summary>
        /// The 'id' property of a separate (potentially external) SARIF 'run' instance that comprises the baseline that was used to compute result 'baselineState' properties for the run.
        /// </summary>
        [DataMember(Name = "baselineId", IsRequired = false, EmitDefaultValue = false)]
        public string BaselineId { get; set; }

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
        /// <param name="originalUriBaseIds">
        /// An initialization value for the <see cref="P: OriginalUriBaseIds" /> property.
        /// </param>
        /// <param name="files">
        /// An initialization value for the <see cref="P: Files" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P: LogicalLocations" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P: Results" /> property.
        /// </param>
        /// <param name="resources">
        /// An initialization value for the <see cref="P: Resources" /> property.
        /// </param>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="stableId">
        /// An initialization value for the <see cref="P: StableId" /> property.
        /// </param>
        /// <param name="automationId">
        /// An initialization value for the <see cref="P: AutomationId" /> property.
        /// </param>
        /// <param name="baselineId">
        /// An initialization value for the <see cref="P: BaselineId" /> property.
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
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Run(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, object originalUriBaseIds, IDictionary<string, FileData> files, IDictionary<string, LogicalLocation> logicalLocations, IEnumerable<Result> results, Resources resources, string id, string stableId, string automationId, string baselineId, string architecture, string richMessageMimeType, string redactionToken, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(tool, invocations, conversion, originalUriBaseIds, files, logicalLocations, results, resources, id, stableId, automationId, baselineId, architecture, richMessageMimeType, redactionToken, properties);
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

            Init(other.Tool, other.Invocations, other.Conversion, other.OriginalUriBaseIds, other.Files, other.LogicalLocations, other.Results, other.Resources, other.Id, other.StableId, other.AutomationId, other.BaselineId, other.Architecture, other.RichMessageMimeType, other.RedactionToken, other.Properties);
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

        private void Init(Tool tool, IEnumerable<Invocation> invocations, Conversion conversion, object originalUriBaseIds, IDictionary<string, FileData> files, IDictionary<string, LogicalLocation> logicalLocations, IEnumerable<Result> results, Resources resources, string id, string stableId, string automationId, string baselineId, string architecture, string richMessageMimeType, string redactionToken, IDictionary<string, SerializedPropertyInfo> properties)
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

            OriginalUriBaseIds = originalUriBaseIds;
            if (files != null)
            {
                Files = new Dictionary<string, FileData>();
                foreach (var value_1 in files)
                {
                    Files.Add(value_1.Key, new FileData(value_1.Value));
                }
            }

            if (logicalLocations != null)
            {
                LogicalLocations = new Dictionary<string, LogicalLocation>();
                foreach (var value_2 in logicalLocations)
                {
                    LogicalLocations.Add(value_2.Key, new LogicalLocation(value_2.Value));
                }
            }

            if (results != null)
            {
                var destination_1 = new List<Result>();
                foreach (var value_3 in results)
                {
                    if (value_3 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Result(value_3));
                    }
                }

                Results = destination_1;
            }

            if (resources != null)
            {
                Resources = new Resources(resources);
            }

            Id = id;
            StableId = stableId;
            AutomationId = automationId;
            BaselineId = baselineId;
            Architecture = architecture;
            RichMessageMimeType = richMessageMimeType;
            RedactionToken = redactionToken;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}