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
    /// A proposed fix for the problem represented by a result object. A fix specifies a set of artifacts to modify. For each artifact, it specifies a set of bytes to remove, and provides a set of new bytes to replace them.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Fix : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Fix> ValueComparer => FixEqualityComparer.Instance;

        public bool ValueEquals(Fix other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Fix;
            }
        }

        /// <summary>
        /// A message that describes the proposed fix, enabling viewers to present the proposed change to an end user.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// One or more artifact changes that comprise a fix for a result.
        /// </summary>
        [DataMember(Name = "artifactChanges", IsRequired = true)]
        public virtual IList<ArtifactChange> ArtifactChanges { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the fix.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fix" /> class.
        /// </summary>
        public Fix()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fix" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="artifactChanges">
        /// An initialization value for the <see cref="P:ArtifactChanges" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Fix(Message description, IEnumerable<ArtifactChange> artifactChanges, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(description, artifactChanges, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fix" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Fix(Fix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.ArtifactChanges, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Fix DeepClone()
        {
            return (Fix)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Fix(this);
        }

        protected virtual void Init(Message description, IEnumerable<ArtifactChange> artifactChanges, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (description != null)
            {
                Description = new Message(description);
            }

            if (artifactChanges != null)
            {
                var destination_0 = new List<ArtifactChange>();
                foreach (var value_0 in artifactChanges)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ArtifactChange(value_0));
                    }
                }

                ArtifactChanges = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}