// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// References to external files that should be inlined with the content of a root log file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    public partial class ExternalFiles : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExternalFiles> ValueComparer => ExternalFilesEqualityComparer.Instance;

        public bool ValueEquals(ExternalFiles other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExternalFiles;
            }
        }

        /// <summary>
        /// The location of a file containing a run.conversion object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "conversion", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation Conversion { get; set; }

        /// <summary>
        /// The location of a file containing a run.files object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "files", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation Files { get; set; }

        /// <summary>
        /// The location of a file containing a run.graphs object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "graphs", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation Graphs { get; set; }

        /// <summary>
        /// An array of locations of files containing arrays of run.invocation objects to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "invocations", IsRequired = false, EmitDefaultValue = false)]
        public IList<FileLocation> Invocations { get; set; }

        /// <summary>
        /// The location of a file containing a run.logicalLocations object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation LogicalLocations { get; set; }

        /// <summary>
        /// The location of a file containing a run.resources object to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "resources", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation Resources { get; set; }

        /// <summary>
        /// An array of locations of files containins arrays of run.result objects to be merged with the root log file.
        /// </summary>
        [DataMember(Name = "results", IsRequired = false, EmitDefaultValue = false)]
        public IList<FileLocation> Results { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the external files.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFiles" /> class.
        /// </summary>
        public ExternalFiles()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFiles" /> class from the supplied values.
        /// </summary>
        /// <param name="conversion">
        /// An initialization value for the <see cref="P: Conversion" /> property.
        /// </param>
        /// <param name="files">
        /// An initialization value for the <see cref="P: Files" /> property.
        /// </param>
        /// <param name="graphs">
        /// An initialization value for the <see cref="P: Graphs" /> property.
        /// </param>
        /// <param name="invocations">
        /// An initialization value for the <see cref="P: Invocations" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P: LogicalLocations" /> property.
        /// </param>
        /// <param name="resources">
        /// An initialization value for the <see cref="P: Resources" /> property.
        /// </param>
        /// <param name="results">
        /// An initialization value for the <see cref="P: Results" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public ExternalFiles(FileLocation conversion, FileLocation files, FileLocation graphs, IEnumerable<FileLocation> invocations, FileLocation logicalLocations, FileLocation resources, IEnumerable<FileLocation> results, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(conversion, files, graphs, invocations, logicalLocations, resources, results, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFiles" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExternalFiles(ExternalFiles other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Conversion, other.Files, other.Graphs, other.Invocations, other.LogicalLocations, other.Resources, other.Results, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ExternalFiles DeepClone()
        {
            return (ExternalFiles)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalFiles(this);
        }

        private void Init(FileLocation conversion, FileLocation files, FileLocation graphs, IEnumerable<FileLocation> invocations, FileLocation logicalLocations, FileLocation resources, IEnumerable<FileLocation> results, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (conversion != null)
            {
                Conversion = new FileLocation(conversion);
            }

            if (files != null)
            {
                Files = new FileLocation(files);
            }

            if (graphs != null)
            {
                Graphs = new FileLocation(graphs);
            }

            if (invocations != null)
            {
                var destination_0 = new List<FileLocation>();
                foreach (var value_0 in invocations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new FileLocation(value_0));
                    }
                }

                Invocations = destination_0;
            }

            if (logicalLocations != null)
            {
                LogicalLocations = new FileLocation(logicalLocations);
            }

            if (resources != null)
            {
                Resources = new FileLocation(resources);
            }

            if (results != null)
            {
                var destination_1 = new List<FileLocation>();
                foreach (var value_1 in results)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new FileLocation(value_1));
                    }
                }

                Results = destination_1;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}