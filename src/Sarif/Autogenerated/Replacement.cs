// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
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
        /// The region of the file to delete.
        /// </summary>
        [DataMember(Name = "deletedRegion", IsRequired = true)]
        public Region DeletedRegion { get; set; }

        /// <summary>
        /// The content to insert at the location specified by the 'deletedRegion' property.
        /// </summary>
        [DataMember(Name = "insertedContent", IsRequired = false, EmitDefaultValue = false)]
        public FileContent InsertedContent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class.
        /// </summary>
        public Replacement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replacement" /> class from the supplied values.
        /// </summary>
        /// <param name="deletedRegion">
        /// An initialization value for the <see cref="P: DeletedRegion" /> property.
        /// </param>
        /// <param name="insertedContent">
        /// An initialization value for the <see cref="P: InsertedContent" /> property.
        /// </param>
        public Replacement(Region deletedRegion, FileContent insertedContent)
        {
            Init(deletedRegion, insertedContent);
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

            Init(other.DeletedRegion, other.InsertedContent);
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

        private void Init(Region deletedRegion, FileContent insertedContent)
        {
            if (deletedRegion != null)
            {
                DeletedRegion = new Region(deletedRegion);
            }

            if (insertedContent != null)
            {
                InsertedContent = new FileContent(insertedContent);
            }
        }
    }
}