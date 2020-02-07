// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// The analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class ToolVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<ToolVersionOne> ValueComparer => ToolVersionOneEqualityComparer.Instance;

        public bool ValueEquals(ToolVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.ToolVersionOne;
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
        /// The tool version, in whatever format the tool natively provides.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// The tool version in the format specified by Semantic Versioning 2.0.
        /// </summary>
        [DataMember(Name = "semanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string SemanticVersion { get; set; }

        /// <summary>
        /// The binary version of the tool's primary executable file (for operating systems such as Windows that provide that information).
        /// </summary>
        [DataMember(Name = "fileVersion", IsRequired = false, EmitDefaultValue = false)]
        public string FileVersion { get; set; }

        /// <summary>
        /// A version that uniquely identifies the SARIF logging component that generated this file, if it is versioned separately from the tool.
        /// </summary>
        [DataMember(Name = "sarifLoggerVersion", IsRequired = false, EmitDefaultValue = false)]
        public string SarifLoggerVersion { get; set; }

        /// <summary>
        /// The tool language (expressed as an ISO 649 two-letter lowercase culture code) and region (expressed as an ISO 3166 two-letter uppercase subculture code associated with a country or region).
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the tool.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolVersionOne" /> class.
        /// </summary>
        public ToolVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolVersionOne" /> class from the supplied values.
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
        /// <param name="sarifLoggerVersion">
        /// An initialization value for the <see cref="P: SarifLoggerVersion" /> property.
        /// </param>
        /// <param name="language">
        /// An initialization value for the <see cref="P: Language" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public ToolVersionOne(string name, string fullName, string version, string semanticVersion, string fileVersion, string sarifLoggerVersion, string language, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(name, fullName, version, semanticVersion, fileVersion, sarifLoggerVersion, language, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ToolVersionOne(ToolVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.FullName, other.Version, other.SemanticVersion, other.FileVersion, other.SarifLoggerVersion, other.Language, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ToolVersionOne DeepClone()
        {
            return (ToolVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new ToolVersionOne(this);
        }

        private void Init(string name, string fullName, string version, string semanticVersion, string fileVersion, string sarifLoggerVersion, string language, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Name = name;
            FullName = fullName;
            Version = version;
            SemanticVersion = semanticVersion;
            FileVersion = fileVersion;
            SarifLoggerVersion = sarifLoggerVersion;
            Language = language;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}