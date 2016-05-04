// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class Tool : ISarifNode
    {
        public static IEqualityComparer<Tool> ValueComparer => ToolEqualityComparer.Instance;

        public bool ValueEquals(Tool other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Tool;
            }
        }

        /// <summary>
        /// The name of the tool.
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// The name of the tool along with its version and any other useful identifying information, such as its locale.
        /// </summary>
        [DataMember(Name = "fullName", IsRequired = false, EmitDefaultValue = false)]
        public string FullName { get; set; }

        /// <summary>
        /// The tool version.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// The tool version rendered as Semantic Versioning 2.0.
        /// </summary>
        [DataMember(Name = "semanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string SemanticVersion { get; set; }

        /// <summary>
        /// The binary version of the tool's primary executable file (for operating systems such as Windows that provide that information).
        /// </summary>
        [DataMember(Name = "fileVersion", IsRequired = false, EmitDefaultValue = false)]
        public string FileVersion { get; set; }

        /// <summary>
        /// The tool language (expressed as an ISO 649 two-letter lowercase culture code) and region (expressed as an ISO 3166 two-letter uppercase subculture code associated with a country or region).
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the tool.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the tool.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class.
        /// </summary>
        public Tool()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P: Name" /> property.
        /// </param>
        /// <param name="fullName">
        /// An initialization value for the <see cref="P: FullName" /> property.
        /// </param>
        /// <param name="version">
        /// An initialization value for the <see cref="P: Version" /> property.
        /// </param>
        /// <param name="semanticVersion">
        /// An initialization value for the <see cref="P: SemanticVersion" /> property.
        /// </param>
        /// <param name="fileVersion">
        /// An initialization value for the <see cref="P: FileVersion" /> property.
        /// </param>
        /// <param name="language">
        /// An initialization value for the <see cref="P: Language" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Tool(string name, string fullName, string version, string semanticVersion, string fileVersion, string language, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(name, fullName, version, semanticVersion, fileVersion, language, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Tool(Tool other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.FullName, other.Version, other.SemanticVersion, other.FileVersion, other.Language, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Tool DeepClone()
        {
            return (Tool)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Tool(this);
        }

        private void Init(string name, string fullName, string version, string semanticVersion, string fileVersion, string language, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Name = name;
            FullName = fullName;
            Version = version;
            SemanticVersion = semanticVersion;
            FileVersion = fileVersion;
            Language = language;
            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

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