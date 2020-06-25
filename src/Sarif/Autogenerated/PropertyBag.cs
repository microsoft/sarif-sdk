// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Key/value pairs that provide additional information about the object.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class PropertyBag : ISarifNode
    {
        public static IEqualityComparer<PropertyBag> ValueComparer => PropertyBagEqualityComparer.Instance;

        public bool ValueEquals(PropertyBag other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.PropertyBag;
            }
        }

        /// <summary>
        /// A set of distinct strings that provide additional information.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<string> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag" /> class.
        /// </summary>
        public PropertyBag()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag" /> class from the supplied values.
        /// </summary>
        /// <param name="tags">
        /// An initialization value for the <see cref="P:Tags" /> property.
        /// </param>
        public PropertyBag(IEnumerable<string> tags)
        {
            Init(tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public PropertyBag(PropertyBag other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual PropertyBag DeepClone()
        {
            return (PropertyBag)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new PropertyBag(this);
        }

        protected virtual void Init(IEnumerable<string> tags)
        {
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