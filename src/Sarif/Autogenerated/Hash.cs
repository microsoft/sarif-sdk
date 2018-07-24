// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A hash value of some file or collection of files, together with the hash function used to compute the hash.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
    public partial class Hash : ISarifNode
    {
        public static IEqualityComparer<Hash> ValueComparer => HashEqualityComparer.Instance;

        public bool ValueEquals(Hash other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Hash;
            }
        }

        /// <summary>
        /// The hash value of some file or collection of files, computed by the hash function named in the 'algorithm' property.
        /// </summary>
        [DataMember(Name = "value", IsRequired = true)]
        public string Value { get; set; }

        /// <summary>
        /// The name of the hash function used to compute the hash value specified in the 'value' property.
        /// </summary>
        [DataMember(Name = "algorithm", IsRequired = true)]
        public string Algorithm { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hash" /> class.
        /// </summary>
        public Hash()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hash" /> class from the supplied values.
        /// </summary>
        /// <param name="value">
        /// An initialization value for the <see cref="P: Value" /> property.
        /// </param>
        /// <param name="algorithm">
        /// An initialization value for the <see cref="P: Algorithm" /> property.
        /// </param>
        public Hash(string value, string algorithm)
        {
            Init(value, algorithm);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hash" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Hash(Hash other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Value, other.Algorithm);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Hash DeepClone()
        {
            return (Hash)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Hash(this);
        }

        private void Init(string value, string algorithm)
        {
            Value = value;
            Algorithm = algorithm;
        }
    }
}