// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// The replacement of a single range of bytes in a file. Specifies the location within the file where the replacement is to be made, the number of bytes to remove at that location, and a sequence of bytes to insert at that location.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class ReplacementVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<ReplacementVersionOne> ValueComparer => ReplacementVersionOneEqualityComparer.Instance;

        public bool ValueEquals(ReplacementVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.ReplacementVersionOne;
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
        /// The MIME Base64-encoded byte sequence to be inserted at the byte offset specified by the 'offset' property, measured from the beginning of the file.
        /// </summary>
        [DataMember(Name = "insertedBytes", IsRequired = false, EmitDefaultValue = false)]
        public string InsertedBytes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacementVersionOne" /> class.
        /// </summary>
        public ReplacementVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacementVersionOne" /> class from the supplied values.
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
        public ReplacementVersionOne(int offset, int deletedLength, string insertedBytes)
        {
            Init(offset, deletedLength, insertedBytes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacementVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ReplacementVersionOne(ReplacementVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Offset, other.DeletedLength, other.InsertedBytes);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ReplacementVersionOne DeepClone()
        {
            return (ReplacementVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new ReplacementVersionOne(this);
        }

        private void Init(int offset, int deletedLength, string insertedBytes)
        {
            Offset = offset;
            DeletedLength = deletedLength;
            InsertedBytes = insertedBytes;
        }
    }
}