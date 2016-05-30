// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes a single run of an analysis tool, and contains the output of that run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.36.0.0")]
    public partial class Run : ISarifNode
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
        /// Describes the runtime environment, including parameterization, of the analysis tool run.
        /// </summary>
        [DataMember(Name = "invocation", IsRequired = false, EmitDefaultValue = false)]
        public Invocation Invocation { get; set; }

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
        /// A list of runtime conditions detected by the tool in the course of the analysis.
        /// </summary>
        [DataMember(Name = "toolNotifications", IsRequired = false, EmitDefaultValue = false)]
        public IList<Notification> ToolNotifications { get; set; }

        /// <summary>
        /// A list of conditions detected by the tool that are relevant to the tool's configuration.
        /// </summary>
        [DataMember(Name = "configurationNotifications", IsRequired = false, EmitDefaultValue = false)]
        public IList<Notification> ConfigurationNotifications { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a string and each of whose values is a 'rule' object, that describe all rules associated with an analysis tool or a specific run of an analysis tool.
        /// </summary>
        [DataMember(Name = "rules", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, Rule> Rules { get; set; }

        /// <summary>
        /// An identifier for the run.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

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
        /// <param name="automationId">
        /// An initialization value for the <see cref="P: AutomationId" /> property.
        /// </param>
        /// <param name="baselineId">
        /// An initialization value for the <see cref="P: BaselineId" /> property.
        /// </param>
        /// <param name="architecture">
        /// An initialization value for the <see cref="P: Architecture" /> property.
        /// </param>
        public Run(Tool tool, Invocation invocation, IDictionary<string, FileData> files, IDictionary<string, LogicalLocation> logicalLocations, IEnumerable<Result> results, IEnumerable<Notification> toolNotifications, IEnumerable<Notification> configurationNotifications, IDictionary<string, Rule> rules, string id, string automationId, string baselineId, string architecture)
        {
            Init(tool, invocation, files, logicalLocations, results, toolNotifications, configurationNotifications, rules, id, automationId, baselineId, architecture);
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

            Init(other.Tool, other.Invocation, other.Files, other.LogicalLocations, other.Results, other.ToolNotifications, other.ConfigurationNotifications, other.Rules, other.Id, other.AutomationId, other.BaselineId, other.Architecture);
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

        private void Init(Tool tool, Invocation invocation, IDictionary<string, FileData> files, IDictionary<string, LogicalLocation> logicalLocations, IEnumerable<Result> results, IEnumerable<Notification> toolNotifications, IEnumerable<Notification> configurationNotifications, IDictionary<string, Rule> rules, string id, string automationId, string baselineId, string architecture)
        {
            if (tool != null)
            {
                Tool = new Tool(tool);
            }

            if (invocation != null)
            {
                Invocation = new Invocation(invocation);
            }

            if (files != null)
            {
                Files = new Dictionary<string, FileData>();
                foreach (var value_0 in files)
                {
                    Files.Add(value_0.Key, new FileData(value_0.Value));
                }
            }

            if (logicalLocations != null)
            {
                LogicalLocations = new Dictionary<string, LogicalLocation>();
                foreach (var value_1 in logicalLocations)
                {
                    LogicalLocations.Add(value_1.Key, new LogicalLocation(value_1.Value));
                }
            }

            if (results != null)
            {
                var destination_0 = new List<Result>();
                foreach (var value_2 in results)
                {
                    if (value_2 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Result(value_2));
                    }
                }

                Results = destination_0;
            }

            if (toolNotifications != null)
            {
                var destination_1 = new List<Notification>();
                foreach (var value_3 in toolNotifications)
                {
                    if (value_3 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Notification(value_3));
                    }
                }

                ToolNotifications = destination_1;
            }

            if (configurationNotifications != null)
            {
                var destination_2 = new List<Notification>();
                foreach (var value_4 in configurationNotifications)
                {
                    if (value_4 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new Notification(value_4));
                    }
                }

                ConfigurationNotifications = destination_2;
            }

            if (rules != null)
            {
                Rules = new Dictionary<string, Rule>();
                foreach (var value_5 in rules)
                {
                    Rules.Add(value_5.Key, new Rule(value_5.Value));
                }
            }

            Id = id;
            AutomationId = automationId;
            BaselineId = baselineId;
            Architecture = architecture;
        }
    }
}