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
    /// A change to a single artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ArtifactChange : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ArtifactChange> ValueComparer => ArtifactChangeEqualityComparer.Instance;

        public bool ValueEquals(ArtifactChange other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ArtifactChange;
            }
        }

        /// <summary>
        /// The location of the artifact to change.
        /// </summary>
        [DataMember(Name = "artifactLocation", IsRequired = true)]
        public virtual ArtifactLocation ArtifactLocation { get; set; }

        /// <summary>
        /// An array of replacement objects, each of which represents the replacement of a single region in a single artifact specified by 'artifactLocation'.
        /// </summary>
        [DataMember(Name = "replacements", IsRequired = true)]
        public virtual IList<Replacement> Replacements { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the change.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactChange" /> class.
        /// </summary>
        public ArtifactChange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactChange" /> class from the supplied values.
        /// </summary>
        /// <param name="artifactLocation">
        /// An initialization value for the <see cref="P:ArtifactLocation" /> property.
        /// </param>
        /// <param name="replacements">
        /// An initialization value for the <see cref="P:Replacements" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ArtifactChange(ArtifactLocation artifactLocation, IEnumerable<Replacement> replacements, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(artifactLocation, replacements, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactChange" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ArtifactChange(ArtifactChange other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.ArtifactLocation, other.Replacements, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ArtifactChange DeepClone()
        {
            return (ArtifactChange)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ArtifactChange(this);
        }

        protected virtual void Init(ArtifactLocation artifactLocation, IEnumerable<Replacement> replacements, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (artifactLocation != null)
            {
                ArtifactLocation = new ArtifactLocation(artifactLocation);
            }

            if (replacements != null)
            {
                var destination_0 = new List<Replacement>();
                foreach (var value_0 in replacements)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Replacement(value_0));
                    }
                }

                Replacements = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}