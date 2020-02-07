// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// The runtime environment of the analysis tool run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class InvocationVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<InvocationVersionOne> ValueComparer => InvocationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(InvocationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.InvocationVersionOne;
            }
        }

        /// <summary>
        /// The command line used to invoke the tool.
        /// </summary>
        [DataMember(Name = "commandLine", IsRequired = false, EmitDefaultValue = false)]
        public string CommandLine { get; set; }

        /// <summary>
        /// The contents of any response files specified on the tool's command line.
        /// </summary>
        [DataMember(Name = "responseFiles", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> ResponseFiles { get; set; }

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
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationVersionOne" /> class.
        /// </summary>
        public InvocationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="commandLine">
        /// An initialization value for the <see cref="P: CommandLine" /> property.
        /// </param>
        /// <param name="responseFiles">
        /// An initialization value for the <see cref="P: ResponseFiles" /> property.
        /// </param>
        /// <param name="startTime">
        /// An initialization value for the <see cref="P: StartTime" /> property.
        /// </param>
        /// <param name="endTime">
        /// An initialization value for the <see cref="P: EndTime" /> property.
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
        public InvocationVersionOne(string commandLine, IDictionary<string, string> responseFiles, DateTime startTime, DateTime endTime, string machine, string account, int processId, string fileName, string workingDirectory, IDictionary<string, string> environmentVariables, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(commandLine, responseFiles, startTime, endTime, machine, account, processId, fileName, workingDirectory, environmentVariables, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public InvocationVersionOne(InvocationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.CommandLine, other.ResponseFiles, other.StartTime, other.EndTime, other.Machine, other.Account, other.ProcessId, other.FileName, other.WorkingDirectory, other.EnvironmentVariables, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public InvocationVersionOne DeepClone()
        {
            return (InvocationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new InvocationVersionOne(this);
        }

        private void Init(string commandLine, IDictionary<string, string> responseFiles, DateTime startTime, DateTime endTime, string machine, string account, int processId, string fileName, string workingDirectory, IDictionary<string, string> environmentVariables, IDictionary<string, SerializedPropertyInfo> properties)
        {
            CommandLine = commandLine;
            if (responseFiles != null)
            {
                ResponseFiles = new Dictionary<string, string>(responseFiles);
            }

            StartTime = startTime;
            EndTime = endTime;
            Machine = machine;
            Account = account;
            ProcessId = processId;
            FileName = fileName;
            WorkingDirectory = workingDirectory;
            if (environmentVariables != null)
            {
                EnvironmentVariables = new Dictionary<string, string>(environmentVariables);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}