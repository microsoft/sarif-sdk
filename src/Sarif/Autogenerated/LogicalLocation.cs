// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A logical location of a construct that produced a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class LogicalLocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<LogicalLocation> ValueComparer => LogicalLocationEqualityComparer.Instance;

        public bool ValueEquals(LogicalLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.LogicalLocation;
            }
        }

        /// <summary>
        /// Identifies the construct in which the result occurred. For example, this property might contain the name of a class or a method.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Name { get; set; }

        /// <summary>
        /// The index within the logical locations array.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Index { get; set; }

        /// <summary>
        /// The human-readable fully qualified name of the logical location.
        /// </summary>
        [DataMember(Name = "fullyQualifiedName", IsRequired = false, EmitDefaultValue = false)]
        public virtual string FullyQualifiedName { get; set; }

        /// <summary>
        /// The machine-readable name for the logical location, such as a mangled function name provided by a C++ compiler that encodes calling convention, return type and other details along with the function name.
        /// </summary>
        [DataMember(Name = "decoratedName", IsRequired = false, EmitDefaultValue = false)]
        public virtual string DecoratedName { get; set; }

        /// <summary>
        /// Identifies the index of the immediate parent of the construct in which the result was detected. For example, this property might point to a logical location that represents the namespace that holds a type.
        /// </summary>
        [DataMember(Name = "parentIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int ParentIndex { get; set; }

        /// <summary>
        /// The type of construct this logical location component refers to. Should be one of 'function', 'member', 'module', 'namespace', 'parameter', 'resource', 'returnType', 'type', 'variable', 'object', 'array', 'property', 'value', 'element', 'text', 'attribute', 'comment', 'declaration', 'dtd' or 'processingInstruction', if any of those accurately describe the construct.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Kind { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the logical location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocation" /> class.
        /// </summary>
        public LogicalLocation()
        {
            Index = -1;
            ParentIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
        /// </param>
        /// <param name="index">
        /// An initialization value for the <see cref="P:Index" /> property.
        /// </param>
        /// <param name="fullyQualifiedName">
        /// An initialization value for the <see cref="P:FullyQualifiedName" /> property.
        /// </param>
        /// <param name="decoratedName">
        /// An initialization value for the <see cref="P:DecoratedName" /> property.
        /// </param>
        /// <param name="parentIndex">
        /// An initialization value for the <see cref="P:ParentIndex" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P:Kind" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public LogicalLocation(string name, int index, string fullyQualifiedName, string decoratedName, int parentIndex, string kind, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(name, index, fullyQualifiedName, decoratedName, parentIndex, kind, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public LogicalLocation(LogicalLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.Index, other.FullyQualifiedName, other.DecoratedName, other.ParentIndex, other.Kind, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual LogicalLocation DeepClone()
        {
            return (LogicalLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new LogicalLocation(this);
        }

        protected virtual void Init(string name, int index, string fullyQualifiedName, string decoratedName, int parentIndex, string kind, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Name = name;
            Index = index;
            FullyQualifiedName = fullyQualifiedName;
            DecoratedName = decoratedName;
            ParentIndex = parentIndex;
            Kind = kind;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}