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
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ExternalProperties : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExternalProperties> ValueComparer => ExternalPropertiesEqualityComparer.Instance;

        public bool ValueEquals(ExternalProperties other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExternalProperties;
            }
        }

        /// <summary>
        /// The URI of the JSON schema corresponding to the version of the external property file format.
        /// </summary>
        [DataMember(Name = "schema", IsRequired = false, EmitDefaultValue = false)]
        public Uri Schema { get; set; }

        /// <summary>
        /// The SARIF format version of this log file.
        /// </summary>
        [DataMember(Name = "version", IsRequired = true)]
        public string Version { get; set; }

        /// <summary>
        /// A stable, unique identifer for the external properties in the form of a GUID.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public string Guid { get; set; }

        /// <summary>
        /// A stable, unique identifer for the external properties in the form of a GUID.
        /// </summary>
        [DataMember(Name = "runGuid", IsRequired = false, EmitDefaultValue = false)]
        public string RunGuid { get; set; }

        /// <summary>
        /// A conversion object that describes how a converter transformed an analysis tool's native reporting format into the SARIF format.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public Conversion Conversion { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is the id of a graph and each of whose values is a 'graph' object with that id.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        public object Graphs { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [DataMember(Name = "externalizedProperties", IsRequired = false, EmitDefaultValue = false)]
        public PropertyBag ExternalizedProperties { get; set; }

        /// <summary>
        /// An array of artifact objects relevant to the run.
        /// </summary>
        [DataMember(Name = "artifacts", IsRequired = false, EmitDefaultValue = false)]
        public IList<Artifact> Artifacts { get; set; }

        /// <summary>
        /// Describes the invocation of the analysis tool.
        /// </summary>
        [DataMember(Name = "invocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Invocation> Invocations { get; set; }

        /// <summary>
        /// An array of logical locations such as namespaces, types or functions.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<LogicalLocation> LogicalLocations { get; set; }

        /// <summary>
        /// An array of threadFlowLocation objects cached at run level.
        /// </summary>
        [DataMember(Name = "threadFlowLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ThreadFlowLocation> ThreadFlowLocations { get; set; }

        /// <summary>
        /// The set of results contained in an SARIF log. The results array can be omitted when a run is solely exporting rules metadata. It must be present (but may be empty) if a log file represents an actual scan.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<Result> Results { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to a taxonomy in which results are categorized.
        /// </summary>
        [DataMember(Name = "taxonomies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptor> Taxonomies { get; set; }

        /// <summary>
        /// The analysis tool that was run.
        /// </summary>
        [DataMember(Name = "driver", IsRequired = false, EmitDefaultValue = false)]
        public ToolComponent Driver { get; set; }

        /// <summary>
        /// Tool extensions that contributed to or reconfigured the analysis tool that was run.
        /// </summary>
        [DataMember(Name = "extensions", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ToolComponent> Extensions { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the external properties.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalProperties" /> class.
        /// </summary>
        public ExternalProperties()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalProperties" /> class from the supplied values.
        /// </summary>
        /// <param name="schema">
        /// An initialization value for the <see cref="P:Schema" /> property.
        /// </param>
        /// <param name="version">
        /// An initialization value for the <see cref="P:Version" /> property.
        /// </param>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="runGuid">
        /// An initialization value for the <see cref="P:RunGuid" /> property.
        /// </param>
        /// <param name="conversion">
        /// An initialization value for the <see cref="P:Conversion" /> property.
        /// </param>
        /// <param name="graphs">
        /// An initialization value for the <see cref="P:Graphs" /> property.
        /// </param>
        /// <param name="externalizedProperties">
        /// An initialization value for the <see cref="P:ExternalizedProperties" /> property.
        /// </param>
        /// <param name="artifacts">
        /// An initialization value for the <see cref="P:Artifacts" /> property.
        /// </param>
        /// <param name="invocations">
        /// An initialization value for the <see cref="P:Invocations" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P:LogicalLocations" /> property.
        /// </param>
        /// <param name="threadFlowLocations">
        /// An initialization value for the <see cref="P:ThreadFlowLocations" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P:Results" /> property.
        /// </param>
        /// <param name="taxonomies">
        /// An initialization value for the <see cref="P:Taxonomies" /> property.
        /// </param>
        /// <param name="driver">
        /// An initialization value for the <see cref="P:Driver" /> property.
        /// </param>
        /// <param name="extensions">
        /// An initialization value for the <see cref="P:Extensions" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ExternalProperties(Uri schema, string version, string guid, string runGuid, Conversion conversion, object graphs, PropertyBag externalizedProperties, IEnumerable<Artifact> artifacts, IEnumerable<Invocation> invocations, IEnumerable<LogicalLocation> logicalLocations, IEnumerable<ThreadFlowLocation> threadFlowLocations, IEnumerable<Result> results, IEnumerable<ReportingDescriptor> taxonomies, ToolComponent driver, IEnumerable<ToolComponent> extensions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(schema, version, guid, runGuid, conversion, graphs, externalizedProperties, artifacts, invocations, logicalLocations, threadFlowLocations, results, taxonomies, driver, extensions, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalProperties" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExternalProperties(ExternalProperties other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Schema, other.Version, other.Guid, other.RunGuid, other.Conversion, other.Graphs, other.ExternalizedProperties, other.Artifacts, other.Invocations, other.LogicalLocations, other.ThreadFlowLocations, other.Results, other.Taxonomies, other.Driver, other.Extensions, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ExternalProperties DeepClone()
        {
            return (ExternalProperties)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalProperties(this);
        }

        private void Init(Uri schema, string version, string guid, string runGuid, Conversion conversion, object graphs, PropertyBag externalizedProperties, IEnumerable<Artifact> artifacts, IEnumerable<Invocation> invocations, IEnumerable<LogicalLocation> logicalLocations, IEnumerable<ThreadFlowLocation> threadFlowLocations, IEnumerable<Result> results, IEnumerable<ReportingDescriptor> taxonomies, ToolComponent driver, IEnumerable<ToolComponent> extensions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (schema != null)
            {
                Schema = new Uri(schema.OriginalString, schema.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            Version = version;
            Guid = guid;
            RunGuid = runGuid;
            if (conversion != null)
            {
                Conversion = new Conversion(conversion);
            }

            Graphs = graphs;
            if (externalizedProperties != null)
            {
                ExternalizedProperties = new PropertyBag(externalizedProperties);
            }

            if (artifacts != null)
            {
                var destination_0 = new List<Artifact>();
                foreach (var value_0 in artifacts)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Artifact(value_0));
                    }
                }

                Artifacts = destination_0;
            }

            if (invocations != null)
            {
                var destination_1 = new List<Invocation>();
                foreach (var value_1 in invocations)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Invocation(value_1));
                    }
                }

                Invocations = destination_1;
            }

            if (logicalLocations != null)
            {
                var destination_2 = new List<LogicalLocation>();
                foreach (var value_2 in logicalLocations)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new LogicalLocation(value_2));
                    }
                }

                LogicalLocations = destination_2;
            }

            if (threadFlowLocations != null)
            {
                var destination_3 = new List<ThreadFlowLocation>();
                foreach (var value_3 in threadFlowLocations)
                {
                    if (value_3 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new ThreadFlowLocation(value_3));
                    }
                }

                ThreadFlowLocations = destination_3;
            }

            if (results != null)
            {
                var destination_4 = new List<Result>();
                foreach (var value_4 in results)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Result(value_4));
                    }
                }

                Results = destination_4;
            }

            if (taxonomies != null)
            {
                var destination_5 = new List<ReportingDescriptor>();
                foreach (var value_5 in taxonomies)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new ReportingDescriptor(value_5));
                    }
                }

                Taxonomies = destination_5;
            }

            if (driver != null)
            {
                Driver = new ToolComponent(driver);
            }

            if (extensions != null)
            {
                var destination_6 = new List<ToolComponent>();
                foreach (var value_6 in extensions)
                {
                    if (value_6 == null)
                    {
                        destination_6.Add(null);
                    }
                    else
                    {
                        destination_6.Add(new ToolComponent(value_6));
                    }
                }

                Extensions = destination_6;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}