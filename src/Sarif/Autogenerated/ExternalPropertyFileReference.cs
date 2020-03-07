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
    /// Contains information that enables a SARIF consumer to locate the external property file that contains the value of an externalized property associated with the run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ExternalPropertyFileReference : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExternalPropertyFileReference> ValueComparer => ExternalPropertyFileReferenceEqualityComparer.Instance;

        public bool ValueEquals(ExternalPropertyFileReference other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExternalPropertyFileReference;
            }
        }

        /// <summary>
        /// The location of the external property file.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactLocation Location { get; set; }

        /// <summary>
        /// A stable, unique identifer for the external property file in the form of a GUID.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Guid { get; set; }

        /// <summary>
        /// A non-negative integer specifying the number of items contained in the external property file.
        /// </summary>
        [DataMember(Name = "itemCount", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int ItemCount { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the external property file.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFileReference" /> class.
        /// </summary>
        public ExternalPropertyFileReference()
        {
            ItemCount = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFileReference" /> class from the supplied values.
        /// </summary>
        /// <param name="location">
        /// An initialization value for the <see cref="P:Location" /> property.
        /// </param>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="itemCount">
        /// An initialization value for the <see cref="P:ItemCount" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ExternalPropertyFileReference(ArtifactLocation location, string guid, int itemCount, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(location, guid, itemCount, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFileReference" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExternalPropertyFileReference(ExternalPropertyFileReference other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Location, other.Guid, other.ItemCount, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ExternalPropertyFileReference DeepClone()
        {
            return (ExternalPropertyFileReference)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalPropertyFileReference(this);
        }

        protected virtual void Init(ArtifactLocation location, string guid, int itemCount, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (location != null)
            {
                Location = new ArtifactLocation(location);
            }

            Guid = guid;
            ItemCount = itemCount;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}