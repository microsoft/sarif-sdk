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
    /// A suppression that is relevant to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Suppression : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Suppression> ValueComparer => SuppressionEqualityComparer.Instance;

        public bool ValueEquals(Suppression other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Suppression;
            }
        }

        /// <summary>
        /// A string that indicates where the suppression is persisted.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = true)]
        public SuppressionKind Kind { get; set; }

        /// <summary>
        /// Identifies the location associated with the suppression.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public Location Location { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the suppression.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Suppression" /> class.
        /// </summary>
        public Suppression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Suppression" /> class from the supplied values.
        /// </summary>
        /// <param name="kind">
        /// An initialization value for the <see cref="P:Kind" /> property.
        /// </param>
        /// <param name="location">
        /// An initialization value for the <see cref="P:Location" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Suppression(SuppressionKind kind, Location location, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(kind, location, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Suppression" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Suppression(Suppression other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Kind, other.Location, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Suppression DeepClone()
        {
            return (Suppression)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Suppression(this);
        }

        private void Init(SuppressionKind kind, Location location, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Kind = kind;
            if (location != null)
            {
                Location = new Location(location);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}