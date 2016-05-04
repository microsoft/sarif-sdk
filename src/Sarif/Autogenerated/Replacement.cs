// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The replacement of a single range of bytes in a file. Specifies the location within the file where the replacement is to be made, the number of bytes to remove at that location, and a sequence of bytes to insert at that location.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class Replacement : ISarifNode
    {
        public static IEqualityComparer<Replacement> ValueComparer => ReplacementEqualityComparer.Instance;

        public bool ValueEquals(Replacement other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Replacement;
            }
        }

        /// <summary>
        /// A non-negative integer specifying the offset in bytes from the beginning of the file at which bytes are to be removed, inserted or both. An offset of 0 shall denote the first byte in the file.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = true)]
        public int Offset { get; set; }

        /// <summary>
        /// The number of bytes to delete, starting at the byte offset specified by offset, measured from the beginning of the file.
        /// </summary>
        [DataMember(Name = "deletedLength", IsRequired = false, EmitDefaultValue = false)]
        public int DeletedLength { get; set; }

        /// <summary>
        /// The byte sequence to be inserted at the byte offset specified by the 'offset' property, measured from the beginning of the file.
        /// </summary>
        [DataMember(Name = "insertedBytes", IsRequired = false, EmitDefaultValue = false)]
        public string InsertedBytes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class.
        /// </summary>
        public Replacement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class from the supplied values.
        /// </summary>
        /// <param name="offset">
        /// An initialization value for the <see cref="P: Offset" /> property.
        /// </param>
        /// <param name="deletedLength">
        /// An initialization value for the <see cref="P: DeletedLength" /> property.
        /// </param>
        /// <param name="insertedBytes">
        /// An initialization value for the <see cref="P: InsertedBytes" /> property.
        /// </param>
        public Replacement(int offset, int deletedLength, string insertedBytes)
        {
            Init(offset, deletedLength, insertedBytes);
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

            Init(other.Offset, other.DeletedLength, other.InsertedBytes);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Replacement DeepClone()
        {
            return (Replacement)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Replacement(this);
        }

        private void Init(int offset, int deletedLength, string insertedBytes)
        {
            Offset = offset;
            DeletedLength = deletedLength;
            InsertedBytes = insertedBytes;
        }
    }
}