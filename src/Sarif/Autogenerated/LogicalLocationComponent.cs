// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A component of a logical location.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class LogicalLocationComponent : ISarifNode
    {
        public static IEqualityComparer<LogicalLocationComponent> ValueComparer => LogicalLocationComponentEqualityComparer.Instance;

        public bool ValueEquals(LogicalLocationComponent other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.LogicalLocationComponent;
            }
        }

        /// <summary>
        /// Identifies the construct in which the result occurred. For example, this property might contain the name of a class or a method.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// The type of construct this logicalLocationComponent refers to. Should be one of 'declaration', 'function', 'member', 'module', 'namespace', 'package', 'resource', or 'type', if any of those accurately describe the construct.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public string Kind { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocationComponent" /> class.
        /// </summary>
        public LogicalLocationComponent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocationComponent" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P: Name" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P: Kind" /> property.
        /// </param>
        public LogicalLocationComponent(string name, string kind)
        {
            Init(name, kind);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocationComponent" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public LogicalLocationComponent(LogicalLocationComponent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.Kind);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public LogicalLocationComponent DeepClone()
        {
            return (LogicalLocationComponent)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new LogicalLocationComponent(this);
        }

        private void Init(string name, string kind)
        {
            Name = name;
            Kind = kind;
        }
    }
}