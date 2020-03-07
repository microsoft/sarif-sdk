// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The runtime environment of the analysis tool run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Invocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Invocation> ValueComparer => InvocationEqualityComparer.Instance;

        public bool ValueEquals(Invocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

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
        /// An array of strings, containing in order the command line arguments passed to the tool from the operating system.
        /// </summary>
        [DataMember(Name = "arguments", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Arguments { get; set; }

        /// <summary>
        /// The locations of any response files specified on the tool's command line.
        /// </summary>
        [DataMember(Name = "responseFiles", IsRequired = false, EmitDefaultValue = false)]
        public IList<ArtifactLocation> ResponseFiles { get; set; }

        /// <summary>
        /// The Coordinated Universal Time (UTC) date and time at which the run started. See "Date/time properties" in the SARIF spec for the required format.
        /// </summary>
        [DataMember(Name = "startTimeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public DateTime StartTimeUtc { get; set; }

        /// <summary>
        /// The Coordinated Universal Time (UTC) date and time at which the run ended. See "Date/time properties" in the SARIF spec for the required format.
        /// </summary>
        [DataMember(Name = "endTimeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public DateTime EndTimeUtc { get; set; }

        /// <summary>
        /// The process exit code.
        /// </summary>
        [DataMember(Name = "exitCode", IsRequired = false, EmitDefaultValue = false)]
        public int ExitCode { get; set; }

        /// <summary>
        /// An array of configurationOverride objects that describe rules related runtime overrides.
        /// </summary>
        [DataMember(Name = "ruleConfigurationOverrides", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ConfigurationOverride> RuleConfigurationOverrides { get; set; }

        /// <summary>
        /// An array of configurationOverride objects that describe notifications related runtime overrides.
        /// </summary>
        [DataMember(Name = "notificationConfigurationOverrides", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ConfigurationOverride> NotificationConfigurationOverrides { get; set; }

        /// <summary>
        /// A list of runtime conditions detected by the tool during the analysis.
        /// </summary>
        [DataMember(Name = "toolExecutionNotifications", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Notification> ToolExecutionNotifications { get; set; }

        /// <summary>
        /// A list of conditions detected by the tool that are relevant to the tool's configuration.
        /// </summary>
        [DataMember(Name = "toolConfigurationNotifications", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Notification> ToolConfigurationNotifications { get; set; }

        /// <summary>
        /// The reason for the process exit.
        /// </summary>
        [DataMember(Name = "exitCodeDescription", IsRequired = false, EmitDefaultValue = false)]
        public string ExitCodeDescription { get; set; }

        /// <summary>
        /// The name of the signal that caused the process to exit.
        /// </summary>
        [DataMember(Name = "exitSignalName", IsRequired = false, EmitDefaultValue = false)]
        public string ExitSignalName { get; set; }

        /// <summary>
        /// The numeric value of the signal that caused the process to exit.
        /// </summary>
        [DataMember(Name = "exitSignalNumber", IsRequired = false, EmitDefaultValue = false)]
        public int ExitSignalNumber { get; set; }

        /// <summary>
        /// The reason given by the operating system that the process failed to start.
        /// </summary>
        [DataMember(Name = "processStartFailureMessage", IsRequired = false, EmitDefaultValue = false)]
        public string ProcessStartFailureMessage { get; set; }

        /// <summary>
        /// Specifies whether the tool's execution completed successfully.
        /// </summary>
        [DataMember(Name = "executionSuccessful", IsRequired = true)]
        public bool ExecutionSuccessful { get; set; }

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
        /// An absolute URI specifying the location of the analysis tool's executable.
        /// </summary>
        [DataMember(Name = "executableLocation", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation ExecutableLocation { get; set; }

        /// <summary>
        /// The working directory for the analysis tool run.
        /// </summary>
        [DataMember(Name = "workingDirectory", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation WorkingDirectory { get; set; }

        /// <summary>
        /// The environment variables associated with the analysis tool process, expressed as key/value pairs.
        /// </summary>
        [DataMember(Name = "environmentVariables", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// A file containing the standard input stream to the process that was invoked.
        /// </summary>
        [DataMember(Name = "stdin", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation Stdin { get; set; }

        /// <summary>
        /// A file containing the standard output stream from the process that was invoked.
        /// </summary>
        [DataMember(Name = "stdout", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation Stdout { get; set; }

        /// <summary>
        /// A file containing the standard error stream from the process that was invoked.
        /// </summary>
        [DataMember(Name = "stderr", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation Stderr { get; set; }

        /// <summary>
        /// A file containing the interleaved standard output and standard error stream from the process that was invoked.
        /// </summary>
        [DataMember(Name = "stdoutStderr", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation StdoutStderr { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the invocation.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation" /> class.
        /// </summary>
        public Invocation()
        {
            // NOTYETAUTOGENERATED: required until https://github.com/microsoft/jschema/issues/95 is fixed
            // 
            // We must dig the pit of success for developers by instantiating an invocation assuming
            // a successful invocation. On invocation, producers can set this to false in the 
            // event of failure.
            ExecutionSuccessful = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation" /> class from the supplied values.
        /// </summary>
        /// <param name="commandLine">
        /// An initialization value for the <see cref="P:CommandLine" /> property.
        /// </param>
        /// <param name="arguments">
        /// An initialization value for the <see cref="P:Arguments" /> property.
        /// </param>
        /// <param name="responseFiles">
        /// An initialization value for the <see cref="P:ResponseFiles" /> property.
        /// </param>
        /// <param name="startTimeUtc">
        /// An initialization value for the <see cref="P:StartTimeUtc" /> property.
        /// </param>
        /// <param name="endTimeUtc">
        /// An initialization value for the <see cref="P:EndTimeUtc" /> property.
        /// </param>
        /// <param name="exitCode">
        /// An initialization value for the <see cref="P:ExitCode" /> property.
        /// </param>
        /// <param name="ruleConfigurationOverrides">
        /// An initialization value for the <see cref="P:RuleConfigurationOverrides" /> property.
        /// </param>
        /// <param name="notificationConfigurationOverrides">
        /// An initialization value for the <see cref="P:NotificationConfigurationOverrides" /> property.
        /// </param>
        /// <param name="toolExecutionNotifications">
        /// An initialization value for the <see cref="P:ToolExecutionNotifications" /> property.
        /// </param>
        /// <param name="toolConfigurationNotifications">
        /// An initialization value for the <see cref="P:ToolConfigurationNotifications" /> property.
        /// </param>
        /// <param name="exitCodeDescription">
        /// An initialization value for the <see cref="P:ExitCodeDescription" /> property.
        /// </param>
        /// <param name="exitSignalName">
        /// An initialization value for the <see cref="P:ExitSignalName" /> property.
        /// </param>
        /// <param name="exitSignalNumber">
        /// An initialization value for the <see cref="P:ExitSignalNumber" /> property.
        /// </param>
        /// <param name="processStartFailureMessage">
        /// An initialization value for the <see cref="P:ProcessStartFailureMessage" /> property.
        /// </param>
        /// <param name="executionSuccessful">
        /// An initialization value for the <see cref="P:ExecutionSuccessful" /> property.
        /// </param>
        /// <param name="machine">
        /// An initialization value for the <see cref="P:Machine" /> property.
        /// </param>
        /// <param name="account">
        /// An initialization value for the <see cref="P:Account" /> property.
        /// </param>
        /// <param name="processId">
        /// An initialization value for the <see cref="P:ProcessId" /> property.
        /// </param>
        /// <param name="executableLocation">
        /// An initialization value for the <see cref="P:ExecutableLocation" /> property.
        /// </param>
        /// <param name="workingDirectory">
        /// An initialization value for the <see cref="P:WorkingDirectory" /> property.
        /// </param>
        /// <param name="environmentVariables">
        /// An initialization value for the <see cref="P:EnvironmentVariables" /> property.
        /// </param>
        /// <param name="stdin">
        /// An initialization value for the <see cref="P:Stdin" /> property.
        /// </param>
        /// <param name="stdout">
        /// An initialization value for the <see cref="P:Stdout" /> property.
        /// </param>
        /// <param name="stderr">
        /// An initialization value for the <see cref="P:Stderr" /> property.
        /// </param>
        /// <param name="stdoutStderr">
        /// An initialization value for the <see cref="P:StdoutStderr" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Invocation(string commandLine, IEnumerable<string> arguments, IEnumerable<ArtifactLocation> responseFiles, DateTime startTimeUtc, DateTime endTimeUtc, int exitCode, IEnumerable<ConfigurationOverride> ruleConfigurationOverrides, IEnumerable<ConfigurationOverride> notificationConfigurationOverrides, IEnumerable<Notification> toolExecutionNotifications, IEnumerable<Notification> toolConfigurationNotifications, string exitCodeDescription, string exitSignalName, int exitSignalNumber, string processStartFailureMessage, bool executionSuccessful, string machine, string account, int processId, ArtifactLocation executableLocation, ArtifactLocation workingDirectory, IDictionary<string, string> environmentVariables, ArtifactLocation stdin, ArtifactLocation stdout, ArtifactLocation stderr, ArtifactLocation stdoutStderr, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(commandLine, arguments, responseFiles, startTimeUtc, endTimeUtc, exitCode, ruleConfigurationOverrides, notificationConfigurationOverrides, toolExecutionNotifications, toolConfigurationNotifications, exitCodeDescription, exitSignalName, exitSignalNumber, processStartFailureMessage, executionSuccessful, machine, account, processId, executableLocation, workingDirectory, environmentVariables, stdin, stdout, stderr, stdoutStderr, properties);
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

            Init(other.CommandLine, other.Arguments, other.ResponseFiles, other.StartTimeUtc, other.EndTimeUtc, other.ExitCode, other.RuleConfigurationOverrides, other.NotificationConfigurationOverrides, other.ToolExecutionNotifications, other.ToolConfigurationNotifications, other.ExitCodeDescription, other.ExitSignalName, other.ExitSignalNumber, other.ProcessStartFailureMessage, other.ExecutionSuccessful, other.Machine, other.Account, other.ProcessId, other.ExecutableLocation, other.WorkingDirectory, other.EnvironmentVariables, other.Stdin, other.Stdout, other.Stderr, other.StdoutStderr, other.Properties);
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

        protected virtual void Init(string commandLine, IEnumerable<string> arguments, IEnumerable<ArtifactLocation> responseFiles, DateTime startTimeUtc, DateTime endTimeUtc, int exitCode, IEnumerable<ConfigurationOverride> ruleConfigurationOverrides, IEnumerable<ConfigurationOverride> notificationConfigurationOverrides, IEnumerable<Notification> toolExecutionNotifications, IEnumerable<Notification> toolConfigurationNotifications, string exitCodeDescription, string exitSignalName, int exitSignalNumber, string processStartFailureMessage, bool executionSuccessful, string machine, string account, int processId, ArtifactLocation executableLocation, ArtifactLocation workingDirectory, IDictionary<string, string> environmentVariables, ArtifactLocation stdin, ArtifactLocation stdout, ArtifactLocation stderr, ArtifactLocation stdoutStderr, IDictionary<string, SerializedPropertyInfo> properties)
        {
            CommandLine = commandLine;
            if (arguments != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in arguments)
                {
                    destination_0.Add(value_0);
                }

                Arguments = destination_0;
            }

            if (responseFiles != null)
            {
                var destination_1 = new List<ArtifactLocation>();
                foreach (var value_1 in responseFiles)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new ArtifactLocation(value_1));
                    }
                }

                ResponseFiles = destination_1;
            }

            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            ExitCode = exitCode;
            if (ruleConfigurationOverrides != null)
            {
                var destination_2 = new List<ConfigurationOverride>();
                foreach (var value_2 in ruleConfigurationOverrides)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new ConfigurationOverride(value_2));
                    }
                }

                RuleConfigurationOverrides = destination_2;
            }

            if (notificationConfigurationOverrides != null)
            {
                var destination_3 = new List<ConfigurationOverride>();
                foreach (var value_3 in notificationConfigurationOverrides)
                {
                    if (value_3 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new ConfigurationOverride(value_3));
                    }
                }

                NotificationConfigurationOverrides = destination_3;
            }

            if (toolExecutionNotifications != null)
            {
                var destination_4 = new List<Notification>();
                foreach (var value_4 in toolExecutionNotifications)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Notification(value_4));
                    }
                }

                ToolExecutionNotifications = destination_4;
            }

            if (toolConfigurationNotifications != null)
            {
                var destination_5 = new List<Notification>();
                foreach (var value_5 in toolConfigurationNotifications)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new Notification(value_5));
                    }
                }

                ToolConfigurationNotifications = destination_5;
            }

            ExitCodeDescription = exitCodeDescription;
            ExitSignalName = exitSignalName;
            ExitSignalNumber = exitSignalNumber;
            ProcessStartFailureMessage = processStartFailureMessage;
            ExecutionSuccessful = executionSuccessful;
            Machine = machine;
            Account = account;
            ProcessId = processId;
            if (executableLocation != null)
            {
                ExecutableLocation = new ArtifactLocation(executableLocation);
            }

            if (workingDirectory != null)
            {
                WorkingDirectory = new ArtifactLocation(workingDirectory);
            }

            if (environmentVariables != null)
            {
                EnvironmentVariables = new Dictionary<string, string>(environmentVariables);
            }

            if (stdin != null)
            {
                Stdin = new ArtifactLocation(stdin);
            }

            if (stdout != null)
            {
                Stdout = new ArtifactLocation(stdout);
            }

            if (stderr != null)
            {
                Stderr = new ArtifactLocation(stderr);
            }

            if (stdoutStderr != null)
            {
                StdoutStderr = new ArtifactLocation(stdoutStderr);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}