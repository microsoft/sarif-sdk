// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class AnnotationVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<AnnotationVersionOne> ValueComparer => AnnotationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(AnnotationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.AnnotationVersionOne;
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
        public IList<PhysicalLocationVersionOne> Locations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationVersionOne" /> class.
        /// </summary>
        public AnnotationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P: Locations" /> property.
        /// </param>
        public AnnotationVersionOne(string message, IEnumerable<PhysicalLocationVersionOne> locations)
        {
            Init(message, locations);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public AnnotationVersionOne(AnnotationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Locations);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public AnnotationVersionOne DeepClone()
        {
            return (AnnotationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new AnnotationVersionOne(this);
        }

        private void Init(string message, IEnumerable<PhysicalLocationVersionOne> locations)
        {
            Message = message;
            if (locations != null)
            {
                var destination_0 = new List<PhysicalLocationVersionOne>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new PhysicalLocationVersionOne(value_0));
                    }
                }

                Locations = destination_0;
            }
        }
    }
}