// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// References to external property files that should be inlined with the content of a root log file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
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
        /// An external property file containing a run.resources object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "resources", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFile Resources { get; set; }

        /// <summary>
        /// An external property file containing a run.properties object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public ExternalPropertyFile Properties { get; set; }

        /// <summary>
        /// An array of external property files containing run.files arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> Files { get; set; }

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
        /// An array of external property files containing run.results arrays to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExternalPropertyFile> Results { get; set; }

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
        /// An initialization value for the <see cref="P: Conversion" /> property.
        /// </param>
        /// <param name="graphs">
        /// An initialization value for the <see cref="P: Graphs" /> property.
        /// </param>
        /// <param name="resources">
        /// An initialization value for the <see cref="P: Resources" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="files">
        /// An initialization value for the <see cref="P: Files" /> property.
        /// </param>
        /// <param name="invocations">
        /// An initialization value for the <see cref="P: Invocations" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P: LogicalLocations" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P: Results" /> property.
        /// </param>
        public ExternalPropertyFiles(ExternalPropertyFile conversion, ExternalPropertyFile graphs, ExternalPropertyFile resources, ExternalPropertyFile properties, IEnumerable<ExternalPropertyFile> files, IEnumerable<ExternalPropertyFile> invocations, IEnumerable<ExternalPropertyFile> logicalLocations, IEnumerable<ExternalPropertyFile> results)
        {
            Init(conversion, graphs, resources, properties, files, invocations, logicalLocations, results);
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

            Init(other.Conversion, other.Graphs, other.Resources, other.Properties, other.Files, other.Invocations, other.LogicalLocations, other.Results);
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

        private void Init(ExternalPropertyFile conversion, ExternalPropertyFile graphs, ExternalPropertyFile resources, ExternalPropertyFile properties, IEnumerable<ExternalPropertyFile> files, IEnumerable<ExternalPropertyFile> invocations, IEnumerable<ExternalPropertyFile> logicalLocations, IEnumerable<ExternalPropertyFile> results)
        {
            if (conversion != null)
            {
                Conversion = new ExternalPropertyFile(conversion);
            }

            if (graphs != null)
            {
                Graphs = new ExternalPropertyFile(graphs);
            }

            if (resources != null)
            {
                Resources = new ExternalPropertyFile(resources);
            }

            if (properties != null)
            {
                Properties = new ExternalPropertyFile(properties);
            }

            if (files != null)
            {
                var destination_0 = new List<ExternalPropertyFile>();
                foreach (var value_0 in files)
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

                Files = destination_0;
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

            if (results != null)
            {
                var destination_3 = new List<ExternalPropertyFile>();
                foreach (var value_3 in results)
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

                Results = destination_3;
            }
        }
    }
}