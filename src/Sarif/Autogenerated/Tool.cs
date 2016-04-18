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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.11.0.0")]
    public partial class Tool : ISarifNode, IEquatable<Tool>
    {
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
        /// Key/value pairs that provide additional information about the tool.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the tool.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Tool);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Name != null)
                {
                    result = (result * 31) + Name.GetHashCode();
                }

                if (FullName != null)
                {
                    result = (result * 31) + FullName.GetHashCode();
                }

                if (Version != null)
                {
                    result = (result * 31) + Version.GetHashCode();
                }

                if (SemanticVersion != null)
                {
                    result = (result * 31) + SemanticVersion.GetHashCode();
                }

                if (FileVersion != null)
                {
                    result = (result * 31) + FileVersion.GetHashCode();
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in Properties)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (Tags != null)
                {
                    foreach (var value_1 in Tags)
                    {
                        result = result * 31;
                        if (value_1 != null)
                        {
                            result = (result * 31) + value_1.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(Tool other)
        {
            if (other == null)
            {
                return false;
            }

            if (Name != other.Name)
            {
                return false;
            }

            if (FullName != other.FullName)
            {
                return false;
            }

            if (Version != other.Version)
            {
                return false;
            }

            if (SemanticVersion != other.SemanticVersion)
            {
                return false;
            }

            if (FileVersion != other.FileVersion)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Properties, other.Properties))
            {
                if (Properties == null || other.Properties == null || Properties.Count != other.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in Properties)
                {
                    string value_1;
                    if (!other.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Tags, other.Tags))
            {
                if (Tags == null || other.Tags == null)
                {
                    return false;
                }

                if (Tags.Count != other.Tags.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Tags.Count; ++index_0)
                {
                    if (Tags[index_0] != other.Tags[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

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
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Tool(string name, string fullName, string version, string semanticVersion, string fileVersion, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(name, fullName, version, semanticVersion, fileVersion, properties, tags);
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

            Init(other.Name, other.FullName, other.Version, other.SemanticVersion, other.FileVersion, other.Properties, other.Tags);
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

        private void Init(string name, string fullName, string version, string semanticVersion, string fileVersion, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Name = name;
            FullName = fullName;
            Version = version;
            SemanticVersion = semanticVersion;
            FileVersion = fileVersion;
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