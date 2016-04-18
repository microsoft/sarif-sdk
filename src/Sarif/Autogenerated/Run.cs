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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.11.0.0")]
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
        /// A string containing the runtime parameters with which the tool was invoked. For command line tools, this string may consist of the completely specified command line used to invoke the tool.
        /// </summary>
        [DataMember(Name = "invocation", IsRequired = false, EmitDefaultValue = false)]
        public string Invocation { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a URI and each of whose values is an array of file objects representing the location of a single file scanned during the run.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<Uri, IList<FileData>> Files { get; set; }

        /// <summary>
        /// The set of results contained in an SARIF log. The results array can be omitted when a run is solely exporting rules metadata. It must be present (but may be empty) in the event that a log file represents an actual scan.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<Result> Results { get; set; }

        /// <summary>
        /// An array of 'rule' objects that describe all rules associated with an analysis tool or a specific run of an analysis tool.
        /// </summary>
        [DataMember(Name = "rules", IsRequired = false, EmitDefaultValue = false)]
        public IList<Rule> Rules { get; set; }

        /// <summary>
        /// The date and time at which the run started. See "Date/time properties" in the SARIF spec for the required format.
        /// </summary>
        [DataMember(Name = "startTime", IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The date and time at which the run ended. See "Date/time properties" in the  SARIF spec for the required format.
        /// </summary>
        [DataMember(Name = "endTime", IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// An identifier that allows the run to be correlated with other artifacts produced by a larger automation process.
        /// </summary>
        [DataMember(Name = "correlationId", IsRequired = false, EmitDefaultValue = false)]
        public string CorrelationId { get; set; }

        /// <summary>
        /// An identifier that specifies the hardware architecture for which the run was targeted.
        /// </summary>
        [DataMember(Name = "architecture", IsRequired = false, EmitDefaultValue = false)]
        public string Architecture { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

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

                if (Results != null)
                {
                    foreach (var value_1 in Results)
                    {
                        result = result * 31;
                        if (value_1 != null)
                        {
                            result = (result * 31) + value_1.GetHashCode();
                        }
                    }
                }

                if (Rules != null)
                {
                    foreach (var value_2 in Rules)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }

                result = (result * 31) + StartTime.GetHashCode();
                result = (result * 31) + EndTime.GetHashCode();
                if (CorrelationId != null)
                {
                    result = (result * 31) + CorrelationId.GetHashCode();
                }

                if (Architecture != null)
                {
                    result = (result * 31) + Architecture.GetHashCode();
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_3 in Properties)
                    {
                        xor_1 ^= value_3.Key.GetHashCode();
                        if (value_3.Value != null)
                        {
                            xor_1 ^= value_3.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (Tags != null)
                {
                    foreach (var value_4 in Tags)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
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

            if (Invocation != other.Invocation)
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

                for (int index_1 = 0; index_1 < Results.Count; ++index_1)
                {
                    if (!Object.Equals(Results[index_1], other.Results[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Rules, other.Rules))
            {
                if (Rules == null || other.Rules == null)
                {
                    return false;
                }

                if (Rules.Count != other.Rules.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < Rules.Count; ++index_2)
                {
                    if (!Object.Equals(Rules[index_2], other.Rules[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (StartTime != other.StartTime)
            {
                return false;
            }

            if (EndTime != other.EndTime)
            {
                return false;
            }

            if (CorrelationId != other.CorrelationId)
            {
                return false;
            }

            if (Architecture != other.Architecture)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Properties, other.Properties))
            {
                if (Properties == null || other.Properties == null || Properties.Count != other.Properties.Count)
                {
                    return false;
                }

                foreach (var value_2 in Properties)
                {
                    string value_3;
                    if (!other.Properties.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (value_2.Value != value_3)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Tags, other.Tags))
            {
                if (Tags == null || other.Tags == null)
                {
                    return false;
                }

                if (Tags.Count != other.Tags.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < Tags.Count; ++index_3)
                {
                    if (Tags[index_3] != other.Tags[index_3])
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
        /// <param name="invocation">
        /// An initialization value for the <see cref="P: Invocation" /> property.
        /// </param>
        /// <param name="files">
        /// An initialization value for the <see cref="P: Files" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P: Results" /> property.
        /// </param>
        /// <param name="rules">
        /// An initialization value for the <see cref="P: Rules" /> property.
        /// </param>
        /// <param name="startTime">
        /// An initialization value for the <see cref="P: StartTime" /> property.
        /// </param>
        /// <param name="endTime">
        /// An initialization value for the <see cref="P: EndTime" /> property.
        /// </param>
        /// <param name="correlationId">
        /// An initialization value for the <see cref="P: CorrelationId" /> property.
        /// </param>
        /// <param name="architecture">
        /// An initialization value for the <see cref="P: Architecture" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Run(Tool tool, string invocation, IDictionary<Uri, IList<FileData>> files, IEnumerable<Result> results, IEnumerable<Rule> rules, DateTime startTime, DateTime endTime, string correlationId, string architecture, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(tool, invocation, files, results, rules, startTime, endTime, correlationId, architecture, properties, tags);
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

            Init(other.Tool, other.Invocation, other.Files, other.Results, other.Rules, other.StartTime, other.EndTime, other.CorrelationId, other.Architecture, other.Properties, other.Tags);
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

        private void Init(Tool tool, string invocation, IDictionary<Uri, IList<FileData>> files, IEnumerable<Result> results, IEnumerable<Rule> rules, DateTime startTime, DateTime endTime, string correlationId, string architecture, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            if (tool != null)
            {
                Tool = new Tool(tool);
            }

            Invocation = invocation;
            if (files != null)
            {
                Files = new Dictionary<Uri, IList<FileData>>();
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

            if (results != null)
            {
                var destination_1 = new List<Result>();
                foreach (var value_2 in results)
                {
                    if (value_2 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Result(value_2));
                    }
                }

                Results = destination_1;
            }

            if (rules != null)
            {
                var destination_2 = new List<Rule>();
                foreach (var value_3 in rules)
                {
                    if (value_3 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new Rule(value_3));
                    }
                }

                Rules = destination_2;
            }

            StartTime = startTime;
            EndTime = endTime;
            CorrelationId = correlationId;
            Architecture = architecture;
            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_3 = new List<string>();
                foreach (var value_4 in tags)
                {
                    destination_3.Add(value_4);
                }

                Tags = destination_3;
            }
        }
    }
}