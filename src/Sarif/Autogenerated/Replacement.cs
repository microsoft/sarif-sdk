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
    /// The replacement of a single region of an artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Replacement : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Replacement> ValueComparer => ReplacementEqualityComparer.Instance;

        public bool ValueEquals(Replacement other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Replacement;
            }
        }

        /// <summary>
        /// The region of the artifact to delete.
        /// </summary>
        [DataMember(Name = "deletedRegion", IsRequired = true)]
        public virtual Region DeletedRegion { get; set; }

        /// <summary>
        /// The content to insert at the location specified by the 'deletedRegion' property.
        /// </summary>
        [DataMember(Name = "insertedContent", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactContent InsertedContent { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the replacement.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class.
        /// </summary>
        public Replacement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class from the supplied values.
        /// </summary>
        /// <param name="deletedRegion">
        /// An initialization value for the <see cref="P:DeletedRegion" /> property.
        /// </param>
        /// <param name="insertedContent">
        /// An initialization value for the <see cref="P:InsertedContent" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Replacement(Region deletedRegion, ArtifactContent insertedContent, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(deletedRegion, insertedContent, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Replacement(Replacement other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.DeletedRegion, other.InsertedContent, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Replacement DeepClone()
        {
            return (Replacement)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Replacement(this);
        }

        protected virtual void Init(Region deletedRegion, ArtifactContent insertedContent, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (deletedRegion != null)
            {
                DeletedRegion = new Region(deletedRegion);
            }

            if (insertedContent != null)
            {
                InsertedContent = new ArtifactContent(insertedContent);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}