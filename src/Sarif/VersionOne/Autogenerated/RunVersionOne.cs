// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Describes a single run of an analysis tool, and contains the output of that run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class RunVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<RunVersionOne> ValueComparer => RunVersionOneEqualityComparer.Instance;

        public bool ValueEquals(RunVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.RunVersionOne;
            }
        }

        /// <summary>
        /// Information about the tool or tool pipeline that generated the results in this run. A run can only contain results produced by a single tool or tool pipeline. A run can aggregate results from multiple log files, as long as context around the tool run (tool command-line arguments and the like) is identical for all aggregated files.
        /// </summary>
        [DataMember(Name = "tool", IsRequired = true)]
        public ToolVersionOne Tool { get; set; }

        /// <summary>
        /// Describes the runtime environment, including parameterization, of the analysis tool run.
        /// </summary>
        [DataMember(Name = "invocation", IsRequired = false, EmitDefaultValue = false)]
        public InvocationVersionOne Invocation { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a URI and each of whose values is an array of file objects representing the location of a single file scanned during the run.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, FileDataVersionOne> Files { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a logical location such as a namespace, type or function.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, LogicalLocationVersionOne> LogicalLocations { get; set; }

        /// <summary>
        /// The set of results contained in an SARIF log. The results array can be omitted when a run is solely exporting rules metadata. It must be present (but may be empty) in the event that a log file represents an actual scan.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<ResultVersionOne> Results { get; set; }

        /// <summary>
        /// A list of runtime conditions detected by the tool in the course of the analysis.
        /// </summary>
        [DataMember(Name = "toolNotifications", IsRequired = false, EmitDefaultValue = false)]
        public IList<NotificationVersionOne> ToolNotifications { get; set; }

        /// <summary>
        /// A list of conditions detected by the tool that are relevant to the tool's configuration.
        /// </summary>
        [DataMember(Name = "configurationNotifications", IsRequired = false, EmitDefaultValue = false)]
        public IList<NotificationVersionOne> ConfigurationNotifications { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a string and each of whose values is a 'rule' object, that describe all rules associated with an analysis tool or a specific run of an analysis tool.
        /// </summary>
        [DataMember(Name = "rules", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, RuleVersionOne> Rules { get; set; }

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
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunVersionOne" /> class.
        /// </summary>
        public RunVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="tool">
        /// An initialization value for the <see cref="P: Tool" /> property.
        /// </param>
        /// <param name="invocation">
        /// An initialization value for the <see cref="P: Invocation" /> property.
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
        /// <param name="toolNotifications">
        /// An initialization value for the <see cref="P: ToolNotifications" /> property.
        /// </param>
        /// <param name="configurationNotifications">
        /// An initialization value for the <see cref="P: ConfigurationNotifications" /> property.
        /// </param>
        /// <param name="rules">
        /// An initialization value for the <see cref="P: Rules" /> property.
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
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public RunVersionOne(ToolVersionOne tool, InvocationVersionOne invocation, IDictionary<string, FileDataVersionOne> files, IDictionary<string, LogicalLocationVersionOne> logicalLocations, IEnumerable<ResultVersionOne> results, IEnumerable<NotificationVersionOne> toolNotifications, IEnumerable<NotificationVersionOne> configurationNotifications, IDictionary<string, RuleVersionOne> rules, string id, string stableId, string automationId, string baselineId, string architecture, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(tool, invocation, files, logicalLocations, results, toolNotifications, configurationNotifications, rules, id, stableId, automationId, baselineId, architecture, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public RunVersionOne(RunVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Tool, other.Invocation, other.Files, other.LogicalLocations, other.Results, other.ToolNotifications, other.ConfigurationNotifications, other.Rules, other.Id, other.StableId, other.AutomationId, other.BaselineId, other.Architecture, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public RunVersionOne DeepClone()
        {
            return (RunVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new RunVersionOne(this);
        }

        private void Init(ToolVersionOne tool, InvocationVersionOne invocation, IDictionary<string, FileDataVersionOne> files, IDictionary<string, LogicalLocationVersionOne> logicalLocations, IEnumerable<ResultVersionOne> results, IEnumerable<NotificationVersionOne> toolNotifications, IEnumerable<NotificationVersionOne> configurationNotifications, IDictionary<string, RuleVersionOne> rules, string id, string stableId, string automationId, string baselineId, string architecture, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (tool != null)
            {
                Tool = new ToolVersionOne(tool);
            }

            if (invocation != null)
            {
                Invocation = new InvocationVersionOne(invocation);
            }

            if (files != null)
            {
                Files = new Dictionary<string, FileDataVersionOne>();
                foreach (var value_0 in files)
                {
                    Files.Add(value_0.Key, new FileDataVersionOne(value_0.Value));
                }
            }

            if (logicalLocations != null)
            {
                LogicalLocations = new Dictionary<string, LogicalLocationVersionOne>();
                foreach (var value_1 in logicalLocations)
                {
                    LogicalLocations.Add(value_1.Key, new LogicalLocationVersionOne(value_1.Value));
                }
            }

            if (results != null)
            {
                var destination_0 = new List<ResultVersionOne>();
                foreach (var value_2 in results)
                {
                    if (value_2 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ResultVersionOne(value_2));
                    }
                }

                Results = destination_0;
            }

            if (toolNotifications != null)
            {
                var destination_1 = new List<NotificationVersionOne>();
                foreach (var value_3 in toolNotifications)
                {
                    if (value_3 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new NotificationVersionOne(value_3));
                    }
                }

                ToolNotifications = destination_1;
            }

            if (configurationNotifications != null)
            {
                var destination_2 = new List<NotificationVersionOne>();
                foreach (var value_4 in configurationNotifications)
                {
                    if (value_4 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new NotificationVersionOne(value_4));
                    }
                }

                ConfigurationNotifications = destination_2;
            }

            if (rules != null)
            {
                Rules = new Dictionary<string, RuleVersionOne>();
                foreach (var value_5 in rules)
                {
                    Rules.Add(value_5.Key, new RuleVersionOne(value_5.Value));
                }
            }

            Id = id;
            StableId = stableId;
            AutomationId = automationId;
            BaselineId = baselineId;
            Architecture = architecture;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}