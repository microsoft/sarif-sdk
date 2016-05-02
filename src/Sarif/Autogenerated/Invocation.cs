// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The runtime environment of the analysis tool run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.19.0.0")]
    public partial class Invocation : ISarifNode, IEquatable<Invocation>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Invocation;
            }
        }

        /// <summary>
        /// The command line used to invoke the tool.
        /// </summary>
        [DataMember(Name = "commandLine", IsRequired = false, EmitDefaultValue = false)]
        public string CommandLine { get; set; }

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
        /// The machine that hosted the analysis tool run.
        /// </summary>
        [DataMember(Name = "machine", IsRequired = false, EmitDefaultValue = false)]
        public string Machine { get; set; }

        /// <summary>
        /// The account that ran the analysis tool.
        /// </summary>
        [DataMember(Name = "account", IsRequired = false, EmitDefaultValue = false)]
        public string Account { get; set; }

        /// <summary>
        /// The process id for the analysis tool run.
        /// </summary>
        [DataMember(Name = "processId", IsRequired = false, EmitDefaultValue = false)]
        public int ProcessId { get; set; }

        /// <summary>
        /// The fully qualified path to the analysis tool.
        /// </summary>
        [DataMember(Name = "fileName", IsRequired = false, EmitDefaultValue = false)]
        public string FileName { get; set; }

        /// <summary>
        /// The working directory for the analysis rool run.
        /// </summary>
        [DataMember(Name = "workingDirectory", IsRequired = false, EmitDefaultValue = false)]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The environment variables associated with the analysis tool process, expressed as key/value pairs.
        /// </summary>
        [DataMember(Name = "environmentVariables", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public object Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Invocation);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (CommandLine != null)
                {
                    result = (result * 31) + CommandLine.GetHashCode();
                }

                result = (result * 31) + StartTime.GetHashCode();
                result = (result * 31) + EndTime.GetHashCode();
                if (CorrelationId != null)
                {
                    result = (result * 31) + CorrelationId.GetHashCode();
                }

                if (Machine != null)
                {
                    result = (result * 31) + Machine.GetHashCode();
                }

                if (Account != null)
                {
                    result = (result * 31) + Account.GetHashCode();
                }

                result = (result * 31) + ProcessId.GetHashCode();
                if (FileName != null)
                {
                    result = (result * 31) + FileName.GetHashCode();
                }

                if (WorkingDirectory != null)
                {
                    result = (result * 31) + WorkingDirectory.GetHashCode();
                }

                if (EnvironmentVariables != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in EnvironmentVariables)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (Properties != null)
                {
                    result = (result * 31) + Properties.GetHashCode();
                }

                if (Tags != null)
                {
                    foreach (var value_1 in Tags)
                    {
                        result = result * 31;
                        if (value_1 != null)
                        {
                            result = (result * 31) + value_1.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(Invocation other)
        {
            if (other == null)
            {
                return false;
            }

            if (CommandLine != other.CommandLine)
            {
                return false;
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

            if (Machine != other.Machine)
            {
                return false;
            }

            if (Account != other.Account)
            {
                return false;
            }

            if (ProcessId != other.ProcessId)
            {
                return false;
            }

            if (FileName != other.FileName)
            {
                return false;
            }

            if (WorkingDirectory != other.WorkingDirectory)
            {
                return false;
            }

            if (!Object.ReferenceEquals(EnvironmentVariables, other.EnvironmentVariables))
            {
                if (EnvironmentVariables == null || other.EnvironmentVariables == null || EnvironmentVariables.Count != other.EnvironmentVariables.Count)
                {
                    return false;
                }

                foreach (var value_0 in EnvironmentVariables)
                {
                    string value_1;
                    if (!other.EnvironmentVariables.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.Equals(Properties, other.Properties))
            {
                return false;
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

                for (int index_0 = 0; index_0 < Tags.Count; ++index_0)
                {
                    if (Tags[index_0] != other.Tags[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation" /> class.
        /// </summary>
        public Invocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation" /> class from the supplied values.
        /// </summary>
        /// <param name="commandLine">
        /// An initialization value for the <see cref="P: CommandLine" /> property.
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
        /// <param name="machine">
        /// An initialization value for the <see cref="P: Machine" /> property.
        /// </param>
        /// <param name="account">
        /// An initialization value for the <see cref="P: Account" /> property.
        /// </param>
        /// <param name="processId">
        /// An initialization value for the <see cref="P: ProcessId" /> property.
        /// </param>
        /// <param name="fileName">
        /// An initialization value for the <see cref="P: FileName" /> property.
        /// </param>
        /// <param name="workingDirectory">
        /// An initialization value for the <see cref="P: WorkingDirectory" /> property.
        /// </param>
        /// <param name="environmentVariables">
        /// An initialization value for the <see cref="P: EnvironmentVariables" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Invocation(string commandLine, DateTime startTime, DateTime endTime, string correlationId, string machine, string account, int processId, string fileName, string workingDirectory, IDictionary<string, string> environmentVariables, object properties, IEnumerable<string> tags)
        {
            Init(commandLine, startTime, endTime, correlationId, machine, account, processId, fileName, workingDirectory, environmentVariables, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Invocation(Invocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.CommandLine, other.StartTime, other.EndTime, other.CorrelationId, other.Machine, other.Account, other.ProcessId, other.FileName, other.WorkingDirectory, other.EnvironmentVariables, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Invocation DeepClone()
        {
            return (Invocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Invocation(this);
        }

        private void Init(string commandLine, DateTime startTime, DateTime endTime, string correlationId, string machine, string account, int processId, string fileName, string workingDirectory, IDictionary<string, string> environmentVariables, object properties, IEnumerable<string> tags)
        {
            CommandLine = commandLine;
            StartTime = startTime;
            EndTime = endTime;
            CorrelationId = correlationId;
            Machine = machine;
            Account = account;
            ProcessId = processId;
            FileName = fileName;
            WorkingDirectory = workingDirectory;
            if (environmentVariables != null)
            {
                EnvironmentVariables = new Dictionary<string, string>(environmentVariables);
            }

            Properties = properties;
            if (tags != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in tags)
                {
                    destination_0.Add(value_0);
                }

                Tags = destination_0;
            }
        }
    }
}