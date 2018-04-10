// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A logical location of a construct that produced a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class LogicalLocationVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<LogicalLocationVersionOne> ValueComparer => LogicalLocationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(LogicalLocationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.LogicalLocationVersionOne;
            }
        }

        /// <summary>
        /// Identifies the construct in which the result occurred. For example, this property might contain the name of a class or a method.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Identifies the key of the immediate parent of the construct in which the result was detected. For example, this property might point to a logical location that represents the namespace that holds a type.
        /// </summary>
        [DataMember(Name = "parentKey", IsRequired = false, EmitDefaultValue = false)]
        public string ParentKey { get; set; }

        /// <summary>
        /// The type of construct this logicalLocationComponent refers to. Should be one of 'function', 'member', 'module', 'namespace', 'package', 'resource', or 'type', if any of those accurately describe the construct.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public string Kind { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocationVersionOne" /> class.
        /// </summary>
        public LogicalLocationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P: Name" /> property.
        /// </param>
        /// <param name="parentKey">
        /// An initialization value for the <see cref="P: ParentKey" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P: Kind" /> property.
        /// </param>
        public LogicalLocationVersionOne(string name, string parentKey, string kind)
        {
            Init(name, parentKey, kind);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalLocationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public LogicalLocationVersionOne(LogicalLocationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.ParentKey, other.Kind);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public LogicalLocationVersionOne DeepClone()
        {
            return (LogicalLocationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new LogicalLocationVersionOne(this);
        }

        private void Init(string name, string parentKey, string kind)
        {
            Name = name;
            ParentKey = parentKey;
            Kind = kind;
        }
    }
}