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
    /// The top-level element of an external property file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ExternalProperties : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExternalProperties> ValueComparer => ExternalPropertiesEqualityComparer.Instance;

        public bool ValueEquals(ExternalProperties other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
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
        public virtual Uri Schema { get; set; }

        /// <summary>
        /// The SARIF format version of this external properties object.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.SarifVersionConverter))]
        public virtual SarifVersion Version { get; set; }

        /// <summary>
        /// A stable, unique identifer for this external properties object, in the form of a GUID.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Guid { get; set; }

        /// <summary>
        /// A stable, unique identifer for the run associated with this external properties object, in the form of a GUID.
        /// </summary>
        [DataMember(Name = "runGuid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string RunGuid { get; set; }

        /// <summary>
        /// A conversion object that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public virtual Conversion Conversion { get; set; }

        /// <summary>
        /// An array of graph objects that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Graph> Graphs { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "externalizedProperties", IsRequired = false, EmitDefaultValue = false)]
        public virtual PropertyBag ExternalizedProperties { get; set; }

        /// <summary>
        /// An array of artifact objects that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "artifacts", IsRequired = false, EmitDefaultValue = false)]
        public virtual IList<Artifact> Artifacts { get; set; }

        /// <summary>
        /// Describes the invocation of the analysis tool that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "invocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Invocation> Invocations { get; set; }

        /// <summary>
        /// An array of logical locations such as namespaces, types or functions that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<LogicalLocation> LogicalLocations { get; set; }

        /// <summary>
        /// An array of threadFlowLocation objects that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "threadFlowLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ThreadFlowLocation> ThreadFlowLocations { get; set; }

        /// <summary>
        /// An array of result objects that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Result> Results { get; set; }

        /// <summary>
        /// Tool taxonomies that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "taxonomies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ToolComponent> Taxonomies { get; set; }

        /// <summary>
        /// The analysis tool object that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "driver", IsRequired = false, EmitDefaultValue = false)]
        public virtual ToolComponent Driver { get; set; }

        /// <summary>
        /// Tool extensions that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "extensions", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ToolComponent> Extensions { get; set; }

        /// <summary>
        /// Tool policies that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "policies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ToolComponent> Policies { get; set; }

        /// <summary>
        /// Tool translations that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "translations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ToolComponent> Translations { get; set; }

        /// <summary>
        /// Addresses that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "addresses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Address> Addresses { get; set; }

        /// <summary>
        /// Requests that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "webRequests", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<WebRequest> WebRequests { get; set; }

        /// <summary>
        /// Responses that will be merged with a separate run.
        /// </summary>
        [DataMember(Name = "webResponses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<WebResponse> WebResponses { get; set; }

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
        /// <param name="policies">
        /// An initialization value for the <see cref="P:Policies" /> property.
        /// </param>
        /// <param name="translations">
        /// An initialization value for the <see cref="P:Translations" /> property.
        /// </param>
        /// <param name="addresses">
        /// An initialization value for the <see cref="P:Addresses" /> property.
        /// </param>
        /// <param name="webRequests">
        /// An initialization value for the <see cref="P:WebRequests" /> property.
        /// </param>
        /// <param name="webResponses">
        /// An initialization value for the <see cref="P:WebResponses" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ExternalProperties(Uri schema, SarifVersion version, string guid, string runGuid, Conversion conversion, IEnumerable<Graph> graphs, PropertyBag externalizedProperties, IEnumerable<Artifact> artifacts, IEnumerable<Invocation> invocations, IEnumerable<LogicalLocation> logicalLocations, IEnumerable<ThreadFlowLocation> threadFlowLocations, IEnumerable<Result> results, IEnumerable<ToolComponent> taxonomies, ToolComponent driver, IEnumerable<ToolComponent> extensions, IEnumerable<ToolComponent> policies, IEnumerable<ToolComponent> translations, IEnumerable<Address> addresses, IEnumerable<WebRequest> webRequests, IEnumerable<WebResponse> webResponses, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(schema, version, guid, runGuid, conversion, graphs, externalizedProperties, artifacts, invocations, logicalLocations, threadFlowLocations, results, taxonomies, driver, extensions, policies, translations, addresses, webRequests, webResponses, properties);
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

            Init(other.Schema, other.Version, other.Guid, other.RunGuid, other.Conversion, other.Graphs, other.ExternalizedProperties, other.Artifacts, other.Invocations, other.LogicalLocations, other.ThreadFlowLocations, other.Results, other.Taxonomies, other.Driver, other.Extensions, other.Policies, other.Translations, other.Addresses, other.WebRequests, other.WebResponses, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ExternalProperties DeepClone()
        {
            return (ExternalProperties)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalProperties(this);
        }

        protected virtual void Init(Uri schema, SarifVersion version, string guid, string runGuid, Conversion conversion, IEnumerable<Graph> graphs, PropertyBag externalizedProperties, IEnumerable<Artifact> artifacts, IEnumerable<Invocation> invocations, IEnumerable<LogicalLocation> logicalLocations, IEnumerable<ThreadFlowLocation> threadFlowLocations, IEnumerable<Result> results, IEnumerable<ToolComponent> taxonomies, ToolComponent driver, IEnumerable<ToolComponent> extensions, IEnumerable<ToolComponent> policies, IEnumerable<ToolComponent> translations, IEnumerable<Address> addresses, IEnumerable<WebRequest> webRequests, IEnumerable<WebResponse> webResponses, IDictionary<string, SerializedPropertyInfo> properties)
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

            if (graphs != null)
            {
                var destination_0 = new List<Graph>();
                foreach (var value_0 in graphs)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Graph(value_0));
                    }
                }

                Graphs = destination_0;
            }

            if (externalizedProperties != null)
            {
                ExternalizedProperties = new PropertyBag(externalizedProperties);
            }

            if (artifacts != null)
            {
                var destination_1 = new List<Artifact>();
                foreach (var value_1 in artifacts)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Artifact(value_1));
                    }
                }

                Artifacts = destination_1;
            }

            if (invocations != null)
            {
                var destination_2 = new List<Invocation>();
                foreach (var value_2 in invocations)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new Invocation(value_2));
                    }
                }

                Invocations = destination_2;
            }

            if (logicalLocations != null)
            {
                var destination_3 = new List<LogicalLocation>();
                foreach (var value_3 in logicalLocations)
                {
                    if (value_3 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new LogicalLocation(value_3));
                    }
                }

                LogicalLocations = destination_3;
            }

            if (threadFlowLocations != null)
            {
                var destination_4 = new List<ThreadFlowLocation>();
                foreach (var value_4 in threadFlowLocations)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new ThreadFlowLocation(value_4));
                    }
                }

                ThreadFlowLocations = destination_4;
            }

            if (results != null)
            {
                var destination_5 = new List<Result>();
                foreach (var value_5 in results)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new Result(value_5));
                    }
                }

                Results = destination_5;
            }

            if (taxonomies != null)
            {
                var destination_6 = new List<ToolComponent>();
                foreach (var value_6 in taxonomies)
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

                Taxonomies = destination_6;
            }

            if (driver != null)
            {
                Driver = new ToolComponent(driver);
            }

            if (extensions != null)
            {
                var destination_7 = new List<ToolComponent>();
                foreach (var value_7 in extensions)
                {
                    if (value_7 == null)
                    {
                        destination_7.Add(null);
                    }
                    else
                    {
                        destination_7.Add(new ToolComponent(value_7));
                    }
                }

                Extensions = destination_7;
            }

            if (policies != null)
            {
                var destination_8 = new List<ToolComponent>();
                foreach (var value_8 in policies)
                {
                    if (value_8 == null)
                    {
                        destination_8.Add(null);
                    }
                    else
                    {
                        destination_8.Add(new ToolComponent(value_8));
                    }
                }

                Policies = destination_8;
            }

            if (translations != null)
            {
                var destination_9 = new List<ToolComponent>();
                foreach (var value_9 in translations)
                {
                    if (value_9 == null)
                    {
                        destination_9.Add(null);
                    }
                    else
                    {
                        destination_9.Add(new ToolComponent(value_9));
                    }
                }

                Translations = destination_9;
            }

            if (addresses != null)
            {
                var destination_10 = new List<Address>();
                foreach (var value_10 in addresses)
                {
                    if (value_10 == null)
                    {
                        destination_10.Add(null);
                    }
                    else
                    {
                        destination_10.Add(new Address(value_10));
                    }
                }

                Addresses = destination_10;
            }

            if (webRequests != null)
            {
                var destination_11 = new List<WebRequest>();
                foreach (var value_11 in webRequests)
                {
                    if (value_11 == null)
                    {
                        destination_11.Add(null);
                    }
                    else
                    {
                        destination_11.Add(new WebRequest(value_11));
                    }
                }

                WebRequests = destination_11;
            }

            if (webResponses != null)
            {
                var destination_12 = new List<WebResponse>();
                foreach (var value_12 in webResponses)
                {
                    if (value_12 == null)
                    {
                        destination_12.Add(null);
                    }
                    else
                    {
                        destination_12.Add(new WebResponse(value_12));
                    }
                }

                WebResponses = destination_12;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}