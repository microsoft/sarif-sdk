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
    /// References to external property files that should be inlined with the content of a root log file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ExternalPropertyFileReferences : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExternalPropertyFileReferences> ValueComparer => ExternalPropertyFileReferencesEqualityComparer.Instance;

        public bool ValueEquals(ExternalPropertyFileReferences other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExternalPropertyFileReferences;
            }
        }

        /// <summary>
        /// An external property file containing a run.conversion object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public virtual ExternalPropertyFileReference Conversion { get; set; }

        /// <summary>
        /// An array of external property files containing a run.graphs object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Graphs { get; set; }

        /// <summary>
        /// An external property file containing a run.properties object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "externalizedProperties", IsRequired = false, EmitDefaultValue = false)]
        public virtual ExternalPropertyFileReference ExternalizedProperties { get; set; }

        /// <summary>
        /// An array of external property files containing run.artifacts arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "artifacts", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Artifacts { get; set; }

        /// <summary>
        /// An array of external property files containing run.invocations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "invocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Invocations { get; set; }

        /// <summary>
        /// An array of external property files containing run.logicalLocations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> LogicalLocations { get; set; }

        /// <summary>
        /// An array of external property files containing run.threadFlowLocations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "threadFlowLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> ThreadFlowLocations { get; set; }

        /// <summary>
        /// An array of external property files containing run.results arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Results { get; set; }

        /// <summary>
        /// An array of external property files containing run.taxonomies arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "taxonomies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Taxonomies { get; set; }

        /// <summary>
        /// An array of external property files containing run.addresses arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "addresses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Addresses { get; set; }

        /// <summary>
        /// An external property file containing a run.driver object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "driver", IsRequired = false, EmitDefaultValue = false)]
        public virtual ExternalPropertyFileReference Driver { get; set; }

        /// <summary>
        /// An array of external property files containing run.extensions arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "extensions", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Extensions { get; set; }

        /// <summary>
        /// An array of external property files containing run.policies arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "policies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Policies { get; set; }

        /// <summary>
        /// An array of external property files containing run.translations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "translations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> Translations { get; set; }

        /// <summary>
        /// An array of external property files containing run.requests arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "webRequests", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> WebRequests { get; set; }

        /// <summary>
        /// An array of external property files containing run.responses arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "webResponses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExternalPropertyFileReference> WebResponses { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the external property files.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFileReferences" /> class.
        /// </summary>
        public ExternalPropertyFileReferences()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFileReferences" /> class from the supplied values.
        /// </summary>
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
        /// <param name="addresses">
        /// An initialization value for the <see cref="P:Addresses" /> property.
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
        /// <param name="webRequests">
        /// An initialization value for the <see cref="P:WebRequests" /> property.
        /// </param>
        /// <param name="webResponses">
        /// An initialization value for the <see cref="P:WebResponses" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ExternalPropertyFileReferences(ExternalPropertyFileReference conversion, IEnumerable<ExternalPropertyFileReference> graphs, ExternalPropertyFileReference externalizedProperties, IEnumerable<ExternalPropertyFileReference> artifacts, IEnumerable<ExternalPropertyFileReference> invocations, IEnumerable<ExternalPropertyFileReference> logicalLocations, IEnumerable<ExternalPropertyFileReference> threadFlowLocations, IEnumerable<ExternalPropertyFileReference> results, IEnumerable<ExternalPropertyFileReference> taxonomies, IEnumerable<ExternalPropertyFileReference> addresses, ExternalPropertyFileReference driver, IEnumerable<ExternalPropertyFileReference> extensions, IEnumerable<ExternalPropertyFileReference> policies, IEnumerable<ExternalPropertyFileReference> translations, IEnumerable<ExternalPropertyFileReference> webRequests, IEnumerable<ExternalPropertyFileReference> webResponses, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(conversion, graphs, externalizedProperties, artifacts, invocations, logicalLocations, threadFlowLocations, results, taxonomies, addresses, driver, extensions, policies, translations, webRequests, webResponses, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFileReferences" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExternalPropertyFileReferences(ExternalPropertyFileReferences other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Conversion, other.Graphs, other.ExternalizedProperties, other.Artifacts, other.Invocations, other.LogicalLocations, other.ThreadFlowLocations, other.Results, other.Taxonomies, other.Addresses, other.Driver, other.Extensions, other.Policies, other.Translations, other.WebRequests, other.WebResponses, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ExternalPropertyFileReferences DeepClone()
        {
            return (ExternalPropertyFileReferences)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalPropertyFileReferences(this);
        }

        protected virtual void Init(ExternalPropertyFileReference conversion, IEnumerable<ExternalPropertyFileReference> graphs, ExternalPropertyFileReference externalizedProperties, IEnumerable<ExternalPropertyFileReference> artifacts, IEnumerable<ExternalPropertyFileReference> invocations, IEnumerable<ExternalPropertyFileReference> logicalLocations, IEnumerable<ExternalPropertyFileReference> threadFlowLocations, IEnumerable<ExternalPropertyFileReference> results, IEnumerable<ExternalPropertyFileReference> taxonomies, IEnumerable<ExternalPropertyFileReference> addresses, ExternalPropertyFileReference driver, IEnumerable<ExternalPropertyFileReference> extensions, IEnumerable<ExternalPropertyFileReference> policies, IEnumerable<ExternalPropertyFileReference> translations, IEnumerable<ExternalPropertyFileReference> webRequests, IEnumerable<ExternalPropertyFileReference> webResponses, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (conversion != null)
            {
                Conversion = new ExternalPropertyFileReference(conversion);
            }

            if (graphs != null)
            {
                var destination_0 = new List<ExternalPropertyFileReference>();
                foreach (var value_0 in graphs)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ExternalPropertyFileReference(value_0));
                    }
                }

                Graphs = destination_0;
            }

            if (externalizedProperties != null)
            {
                ExternalizedProperties = new ExternalPropertyFileReference(externalizedProperties);
            }

            if (artifacts != null)
            {
                var destination_1 = new List<ExternalPropertyFileReference>();
                foreach (var value_1 in artifacts)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new ExternalPropertyFileReference(value_1));
                    }
                }

                Artifacts = destination_1;
            }

            if (invocations != null)
            {
                var destination_2 = new List<ExternalPropertyFileReference>();
                foreach (var value_2 in invocations)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new ExternalPropertyFileReference(value_2));
                    }
                }

                Invocations = destination_2;
            }

            if (logicalLocations != null)
            {
                var destination_3 = new List<ExternalPropertyFileReference>();
                foreach (var value_3 in logicalLocations)
                {
                    if (value_3 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new ExternalPropertyFileReference(value_3));
                    }
                }

                LogicalLocations = destination_3;
            }

            if (threadFlowLocations != null)
            {
                var destination_4 = new List<ExternalPropertyFileReference>();
                foreach (var value_4 in threadFlowLocations)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new ExternalPropertyFileReference(value_4));
                    }
                }

                ThreadFlowLocations = destination_4;
            }

            if (results != null)
            {
                var destination_5 = new List<ExternalPropertyFileReference>();
                foreach (var value_5 in results)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new ExternalPropertyFileReference(value_5));
                    }
                }

                Results = destination_5;
            }

            if (taxonomies != null)
            {
                var destination_6 = new List<ExternalPropertyFileReference>();
                foreach (var value_6 in taxonomies)
                {
                    if (value_6 == null)
                    {
                        destination_6.Add(null);
                    }
                    else
                    {
                        destination_6.Add(new ExternalPropertyFileReference(value_6));
                    }
                }

                Taxonomies = destination_6;
            }

            if (addresses != null)
            {
                var destination_7 = new List<ExternalPropertyFileReference>();
                foreach (var value_7 in addresses)
                {
                    if (value_7 == null)
                    {
                        destination_7.Add(null);
                    }
                    else
                    {
                        destination_7.Add(new ExternalPropertyFileReference(value_7));
                    }
                }

                Addresses = destination_7;
            }

            if (driver != null)
            {
                Driver = new ExternalPropertyFileReference(driver);
            }

            if (extensions != null)
            {
                var destination_8 = new List<ExternalPropertyFileReference>();
                foreach (var value_8 in extensions)
                {
                    if (value_8 == null)
                    {
                        destination_8.Add(null);
                    }
                    else
                    {
                        destination_8.Add(new ExternalPropertyFileReference(value_8));
                    }
                }

                Extensions = destination_8;
            }

            if (policies != null)
            {
                var destination_9 = new List<ExternalPropertyFileReference>();
                foreach (var value_9 in policies)
                {
                    if (value_9 == null)
                    {
                        destination_9.Add(null);
                    }
                    else
                    {
                        destination_9.Add(new ExternalPropertyFileReference(value_9));
                    }
                }

                Policies = destination_9;
            }

            if (translations != null)
            {
                var destination_10 = new List<ExternalPropertyFileReference>();
                foreach (var value_10 in translations)
                {
                    if (value_10 == null)
                    {
                        destination_10.Add(null);
                    }
                    else
                    {
                        destination_10.Add(new ExternalPropertyFileReference(value_10));
                    }
                }

                Translations = destination_10;
            }

            if (webRequests != null)
            {
                var destination_11 = new List<ExternalPropertyFileReference>();
                foreach (var value_11 in webRequests)
                {
                    if (value_11 == null)
                    {
                        destination_11.Add(null);
                    }
                    else
                    {
                        destination_11.Add(new ExternalPropertyFileReference(value_11));
                    }
                }

                WebRequests = destination_11;
            }

            if (webResponses != null)
            {
                var destination_12 = new List<ExternalPropertyFileReference>();
                foreach (var value_12 in webResponses)
                {
                    if (value_12 == null)
                    {
                        destination_12.Add(null);
                    }
                    else
                    {
                        destination_12.Add(new ExternalPropertyFileReference(value_12));
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