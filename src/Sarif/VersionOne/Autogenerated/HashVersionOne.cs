// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A hash value of some file or collection of files, together with the algorithm used to compute the hash.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class HashVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<HashVersionOne> ValueComparer => HashVersionOneEqualityComparer.Instance;

        public bool ValueEquals(HashVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.HashVersionOne;
            }
        }

        /// <summary>
        /// The hash value of some file or collection of files, computed by the algorithm named in the 'algorithm' property.
        /// </summary>
        [DataMember(Name = "value", IsRequired = true)]
        public string Value { get; set; }

        /// <summary>
        /// The name of the algorithm used to compute the hash value specified in the 'value' property.
        /// </summary>
        [DataMember(Name = "algorithm", IsRequired = true)]
        public AlgorithmKindVersionOne Algorithm { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashVersionOne" /> class.
        /// </summary>
        public HashVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="value">
        /// An initialization value for the <see cref="P: Value" /> property.
        /// </param>
        /// <param name="algorithm">
        /// An initialization value for the <see cref="P: Algorithm" /> property.
        /// </param>
        public HashVersionOne(string value, AlgorithmKindVersionOne algorithm)
        {
            Init(value, algorithm);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public HashVersionOne(HashVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Value, other.Algorithm);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public HashVersionOne DeepClone()
        {
            return (HashVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new HashVersionOne(this);
        }

        private void Init(string value, AlgorithmKindVersionOne algorithm)
        {
            Value = value;
            Algorithm = algorithm;
        }
    }
}