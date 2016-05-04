// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A code annotation that consists of single physical location and associated message, used to express code flows through a method, or other locations that are related to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class AnnotatedCodeLocation : ISarifNode
    {
        public static IEqualityComparer<AnnotatedCodeLocation> ValueComparer => AnnotatedCodeLocationEqualityComparer.Instance;

        public bool ValueEquals(AnnotatedCodeLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.AnnotatedCodeLocation;
            }
        }

        /// <summary>
        /// A code location to which this annotation refers.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = true)]
        public PhysicalLocation PhysicalLocation { get; set; }

        /// <summary>
        /// A message relevant to this annotation.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A unique set of strings that provide additional information about the code location.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class.
        /// </summary>
        public AnnotatedCodeLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P: PhysicalLocation" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public AnnotatedCodeLocation(PhysicalLocation physicalLocation, string message, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(physicalLocation, message, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public AnnotatedCodeLocation(AnnotatedCodeLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.PhysicalLocation, other.Message, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public AnnotatedCodeLocation DeepClone()
        {
            return (AnnotatedCodeLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new AnnotatedCodeLocation(this);
        }

        private void Init(PhysicalLocation physicalLocation, string message, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocation(physicalLocation);
            }

            Message = message;
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