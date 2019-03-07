// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// References to external property files that should be inlined with the content of a root log file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ExternalPropertyFiles : ISarifNode
    {
        public static IEqualityComparer<ExternalPropertyFiles> ValueComparer => ExternalPropertyFilesEqualityComparer.Instance;

        public bool ValueEquals(ExternalPropertyFiles other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExternalPropertyFiles;
            }
        }

        /// <summary>
        /// An external property file containing a run.conversion object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFile Conversion { get; set; }

        /// <summary>
        /// An external property file containing a run.graphs object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFile Graphs { get; set; }

        /// <summary>
        /// An external property file containing a run.properties object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "externalizedProperties", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFile ExternalizedProperties { get; set; }

        /// <summary>
        /// An array of external property files containing run.artifacts arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "artifacts", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> Artifacts { get; set; }

        /// <summary>
        /// An array of external property files containing run.invocations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "invocations", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> Invocations { get; set; }

        /// <summary>
        /// An array of external property files containing run.logicalLocations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> LogicalLocations { get; set; }

        /// <summary>
        /// An array of external property files containing run.threadFlowLocations arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "threadFlowLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ExternalPropertyFile> ThreadFlowLocations { get; set; }

        /// <summary>
        /// An array of external property files containing run.results arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> Results { get; set; }

        /// <summary>
        /// An array of external property files containing run.taxonomies arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "taxonomies", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> Taxonomies { get; set; }

        /// <summary>
        /// An array of external property files containing run.addresses arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "addresses", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ExternalPropertyFile> Addresses { get; set; }

        /// <summary>
        /// An external property file containing a run.tool object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "tool", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFile Tool { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFiles" /> class.
        /// </summary>
        public ExternalPropertyFiles()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFiles" /> class from the supplied values.
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
        /// <param name="tool">
        /// An initialization value for the <see cref="P:Tool" /> property.
        /// </param>
        public ExternalPropertyFiles(ExternalPropertyFile conversion, ExternalPropertyFile graphs, ExternalPropertyFile externalizedProperties, IEnumerable<ExternalPropertyFile> artifacts, IEnumerable<ExternalPropertyFile> invocations, IEnumerable<ExternalPropertyFile> logicalLocations, IEnumerable<ExternalPropertyFile> threadFlowLocations, IEnumerable<ExternalPropertyFile> results, IEnumerable<ExternalPropertyFile> taxonomies, IEnumerable<ExternalPropertyFile> addresses, ExternalPropertyFile tool)
        {
            Init(conversion, graphs, externalizedProperties, artifacts, invocations, logicalLocations, threadFlowLocations, results, taxonomies, addresses, tool);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFiles" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExternalPropertyFiles(ExternalPropertyFiles other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Conversion, other.Graphs, other.ExternalizedProperties, other.Artifacts, other.Invocations, other.LogicalLocations, other.ThreadFlowLocations, other.Results, other.Taxonomies, other.Addresses, other.Tool);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ExternalPropertyFiles DeepClone()
        {
            return (ExternalPropertyFiles)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalPropertyFiles(this);
        }

        private void Init(ExternalPropertyFile conversion, ExternalPropertyFile graphs, ExternalPropertyFile externalizedProperties, IEnumerable<ExternalPropertyFile> artifacts, IEnumerable<ExternalPropertyFile> invocations, IEnumerable<ExternalPropertyFile> logicalLocations, IEnumerable<ExternalPropertyFile> threadFlowLocations, IEnumerable<ExternalPropertyFile> results, IEnumerable<ExternalPropertyFile> taxonomies, IEnumerable<ExternalPropertyFile> addresses, ExternalPropertyFile tool)
        {
            if (conversion != null)
            {
                Conversion = new ExternalPropertyFile(conversion);
            }

            if (graphs != null)
            {
                Graphs = new ExternalPropertyFile(graphs);
            }

            if (externalizedProperties != null)
            {
                ExternalizedProperties = new ExternalPropertyFile(externalizedProperties);
            }

            if (artifacts != null)
            {
                var destination_0 = new List<ExternalPropertyFile>();
                foreach (var value_0 in artifacts)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ExternalPropertyFile(value_0));
                    }
                }

                Artifacts = destination_0;
            }

            if (invocations != null)
            {
                var destination_1 = new List<ExternalPropertyFile>();
                foreach (var value_1 in invocations)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new ExternalPropertyFile(value_1));
                    }
                }

                Invocations = destination_1;
            }

            if (logicalLocations != null)
            {
                var destination_2 = new List<ExternalPropertyFile>();
                foreach (var value_2 in logicalLocations)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new ExternalPropertyFile(value_2));
                    }
                }

                LogicalLocations = destination_2;
            }

            if (threadFlowLocations != null)
            {
                var destination_3 = new List<ExternalPropertyFile>();
                foreach (var value_3 in threadFlowLocations)
                {
                    if (value_3 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new ExternalPropertyFile(value_3));
                    }
                }

                ThreadFlowLocations = destination_3;
            }

            if (results != null)
            {
                var destination_4 = new List<ExternalPropertyFile>();
                foreach (var value_4 in results)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new ExternalPropertyFile(value_4));
                    }
                }

                Results = destination_4;
            }

            if (taxonomies != null)
            {
                var destination_5 = new List<ExternalPropertyFile>();
                foreach (var value_5 in taxonomies)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new ExternalPropertyFile(value_5));
                    }
                }

                Taxonomies = destination_5;
            }

            if (addresses != null)
            {
                var destination_6 = new List<ExternalPropertyFile>();
                foreach (var value_6 in addresses)
                {
                    if (value_6 == null)
                    {
                        destination_6.Add(null);
                    }
                    else
                    {
                        destination_6.Add(new ExternalPropertyFile(value_6));
                    }
                }

                Addresses = destination_6;
            }

            if (tool != null)
            {
                Tool = new ExternalPropertyFile(tool);
            }
        }
    }
}