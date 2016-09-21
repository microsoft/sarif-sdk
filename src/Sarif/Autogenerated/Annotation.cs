// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class Annotation : ISarifNode
    {
        public static IEqualityComparer<Annotation> ValueComparer => AnnotationEqualityComparer.Instance;

        public bool ValueEquals(Annotation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Annotation;
            }
        }

        /// <summary>
        /// A message relevant to a code location
        /// </summary>
        [DataMember(Name = "message", IsRequired = true)]
        public string Message { get; set; }

        /// <summary>
        /// An array of 'physicalLocation' objects associated with the annotation.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = true)]
        public IList<PhysicalLocation> Locations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Annotation" /> class.
        /// </summary>
        public Annotation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Annotation" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P: Locations" /> property.
        /// </param>
        public Annotation(string message, IEnumerable<PhysicalLocation> locations)
        {
            Init(message, locations);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Annotation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Annotation(Annotation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Locations);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Annotation DeepClone()
        {
            return (Annotation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Annotation(this);
        }

        private void Init(string message, IEnumerable<PhysicalLocation> locations)
        {
            Message = message;
            if (locations != null)
            {
                var destination_0 = new List<PhysicalLocation>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new PhysicalLocation(value_0));
                    }
                }

                Locations = destination_0;
            }
        }
    }
}