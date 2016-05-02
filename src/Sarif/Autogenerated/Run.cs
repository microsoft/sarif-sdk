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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.19.0.0")]
    public partial class Run : ISarifNode, IEquatable<Run>
    {
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
        /// An identifier for the run.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// An identifier that allows the run to be correlated with other artifacts produced by a larger automation process.
        /// </summary>
        [DataMember(Name = "correlationId", IsRequired = false, EmitDefaultValue = false)]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Describes the runtime environment, including parameterization, of the analysis tool run.
        /// </summary>
        [DataMember(Name = "invocation", IsRequired = false, EmitDefaultValue = false)]
        public Invocation Invocation { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a URI and each of whose values is an array of file objects representing the location of a single file scanned during the run.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<FileData>> Files { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a fully qualified logical location such as a type nested within a namespace, and each of whose values is an array of logicalLocationComponent objects.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, IList<LogicalLocationComponent>> LogicalLocations { get; set; }

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

        public override bool Equals(object other)
        {
            return Equals(other as Run);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Tool != null)
                {
                    result = (result * 31) + Tool.GetHashCode();
                }

                if (Id != null)
                {
                    result = (result * 31) + Id.GetHashCode();
                }

                if (CorrelationId != null)
                {
                    result = (result * 31) + CorrelationId.GetHashCode();
                }

                if (Invocation != null)
                {
                    result = (result * 31) + Invocation.GetHashCode();
                }

                if (Files != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in Files)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (LogicalLocations != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_1 in LogicalLocations)
                    {
                        xor_1 ^= value_1.Key.GetHashCode();
                        if (value_1.Value != null)
                        {
                            xor_1 ^= value_1.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (Results != null)
                {
                    foreach (var value_2 in Results)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }

                if (ToolNotifications != null)
                {
                    foreach (var value_3 in ToolNotifications)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.GetHashCode();
                        }
                    }
                }

                if (ConfigurationNotifications != null)
                {
                    foreach (var value_4 in ConfigurationNotifications)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
                }

                if (Rules != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_5 in Rules)
                    {
                        xor_2 ^= value_5.Key.GetHashCode();
                        if (value_5.Value != null)
                        {
                            xor_2 ^= value_5.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }
            }

            return result;
        }

        public bool Equals(Run other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.Equals(Tool, other.Tool))
            {
                return false;
            }

            if (Id != other.Id)
            {
                return false;
            }

            if (CorrelationId != other.CorrelationId)
            {
                return false;
            }

            if (!Object.Equals(Invocation, other.Invocation))
            {
                return false;
            }

            if (!Object.ReferenceEquals(Files, other.Files))
            {
                if (Files == null || other.Files == null || Files.Count != other.Files.Count)
                {
                    return false;
                }

                foreach (var value_0 in Files)
                {
                    IList<FileData> value_1;
                    if (!other.Files.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!Object.ReferenceEquals(value_0.Value, value_1))
                    {
                        if (value_0.Value == null || value_1 == null)
                        {
                            return false;
                        }

                        if (value_0.Value.Count != value_1.Count)
                        {
                            return false;
                        }

                        for (int index_0 = 0; index_0 < value_0.Value.Count; ++index_0)
                        {
                            if (!Object.Equals(value_0.Value[index_0], value_1[index_0]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (!Object.ReferenceEquals(LogicalLocations, other.LogicalLocations))
            {
                if (LogicalLocations == null || other.LogicalLocations == null || LogicalLocations.Count != other.LogicalLocations.Count)
                {
                    return false;
                }

                foreach (var value_2 in LogicalLocations)
                {
                    IList<LogicalLocationComponent> value_3;
                    if (!other.LogicalLocations.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!Object.ReferenceEquals(value_2.Value, value_3))
                    {
                        if (value_2.Value == null || value_3 == null)
                        {
                            return false;
                        }

                        if (value_2.Value.Count != value_3.Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < value_2.Value.Count; ++index_1)
                        {
                            if (!Object.Equals(value_2.Value[index_1], value_3[index_1]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (!Object.ReferenceEquals(Results, other.Results))
            {
                if (Results == null || other.Results == null)
                {
                    return false;
                }

                if (Results.Count != other.Results.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < Results.Count; ++index_2)
                {
                    if (!Object.Equals(Results[index_2], other.Results[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(ToolNotifications, other.ToolNotifications))
            {
                if (ToolNotifications == null || other.ToolNotifications == null)
                {
                    return false;
                }

                if (ToolNotifications.Count != other.ToolNotifications.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < ToolNotifications.Count; ++index_3)
                {
                    if (!Object.Equals(ToolNotifications[index_3], other.ToolNotifications[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(ConfigurationNotifications, other.ConfigurationNotifications))
            {
                if (ConfigurationNotifications == null || other.ConfigurationNotifications == null)
                {
                    return false;
                }

                if (ConfigurationNotifications.Count != other.ConfigurationNotifications.Count)
                {
                    return false;
                }

                for (int index_4 = 0; index_4 < ConfigurationNotifications.Count; ++index_4)
                {
                    if (!Object.Equals(ConfigurationNotifications[index_4], other.ConfigurationNotifications[index_4]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Rules, other.Rules))
            {
                if (Rules == null || other.Rules == null || Rules.Count != other.Rules.Count)
                {
                    return false;
                }

                foreach (var value_4 in Rules)
                {
                    Rule value_5;
                    if (!other.Rules.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!Object.Equals(value_4.Value, value_5))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

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
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="correlationId">
        /// An initialization value for the <see cref="P: CorrelationId" /> property.
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
        public Run(Tool tool, string id, string correlationId, Invocation invocation, IDictionary<string, IList<FileData>> files, IDictionary<string, IList<LogicalLocationComponent>> logicalLocations, IEnumerable<Result> results, IEnumerable<Notification> toolNotifications, IEnumerable<Notification> configurationNotifications, IDictionary<string, Rule> rules)
        {
            Init(tool, id, correlationId, invocation, files, logicalLocations, results, toolNotifications, configurationNotifications, rules);
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

            Init(other.Tool, other.Id, other.CorrelationId, other.Invocation, other.Files, other.LogicalLocations, other.Results, other.ToolNotifications, other.ConfigurationNotifications, other.Rules);
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

        private void Init(Tool tool, string id, string correlationId, Invocation invocation, IDictionary<string, IList<FileData>> files, IDictionary<string, IList<LogicalLocationComponent>> logicalLocations, IEnumerable<Result> results, IEnumerable<Notification> toolNotifications, IEnumerable<Notification> configurationNotifications, IDictionary<string, Rule> rules)
        {
            if (tool != null)
            {
                Tool = new Tool(tool);
            }

            Id = id;
            CorrelationId = correlationId;
            if (invocation != null)
            {
                Invocation = new Invocation(invocation);
            }

            if (files != null)
            {
                Files = new Dictionary<string, IList<FileData>>();
                foreach (var value_0 in files)
                {
                    var destination_0 = new List<FileData>();
                    foreach (var value_1 in value_0.Value)
                    {
                        if (value_1 == null)
                        {
                            destination_0.Add(null);
                        }
                        else
                        {
                            destination_0.Add(new FileData(value_1));
                        }
                    }

                    Files.Add(value_0.Key, destination_0);
                }
            }

            if (logicalLocations != null)
            {
                LogicalLocations = new Dictionary<string, IList<LogicalLocationComponent>>();
                foreach (var value_2 in logicalLocations)
                {
                    var destination_1 = new List<LogicalLocationComponent>();
                    foreach (var value_3 in value_2.Value)
                    {
                        if (value_3 == null)
                        {
                            destination_1.Add(null);
                        }
                        else
                        {
                            destination_1.Add(new LogicalLocationComponent(value_3));
                        }
                    }

                    LogicalLocations.Add(value_2.Key, destination_1);
                }
            }

            if (results != null)
            {
                var destination_2 = new List<Result>();
                foreach (var value_4 in results)
                {
                    if (value_4 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new Result(value_4));
                    }
                }

                Results = destination_2;
            }

            if (toolNotifications != null)
            {
                var destination_3 = new List<Notification>();
                foreach (var value_5 in toolNotifications)
                {
                    if (value_5 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new Notification(value_5));
                    }
                }

                ToolNotifications = destination_3;
            }

            if (configurationNotifications != null)
            {
                var destination_4 = new List<Notification>();
                foreach (var value_6 in configurationNotifications)
                {
                    if (value_6 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Notification(value_6));
                    }
                }

                ConfigurationNotifications = destination_4;
            }

            if (rules != null)
            {
                Rules = new Dictionary<string, Rule>();
                foreach (var value_7 in rules)
                {
                    Rules.Add(value_7.Key, new Rule(value_7.Value));
                }
            }
        }
    }
}