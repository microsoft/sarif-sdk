// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A file relevant to a tool invocation or to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class Attachment : ISarifNode
    {
        public static IEqualityComparer<Attachment> ValueComparer => AttachmentEqualityComparer.Instance;

        public bool ValueEquals(Attachment other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Attachment;
            }
        }

        /// <summary>
        /// A message describing the role played by the attachment.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public Message Description { get; set; }

        /// <summary>
        /// The location of the attachment.
        /// </summary>
        [DataMember(Name = "fileLocation", IsRequired = true)]
        public FileLocation FileLocation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment" /> class.
        /// </summary>
        public Attachment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P: Description" /> property.
        /// </param>
        /// <param name="fileLocation">
        /// An initialization value for the <see cref="P: FileLocation" /> property.
        /// </param>
        public Attachment(Message description, FileLocation fileLocation)
        {
            Init(description, fileLocation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Attachment(Attachment other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.FileLocation);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Attachment DeepClone()
        {
            return (Attachment)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Attachment(this);
        }

        private void Init(Message description, FileLocation fileLocation)
        {
            if (description != null)
            {
                Description = new Message(description);
            }

            if (fileLocation != null)
            {
                FileLocation = new FileLocation(fileLocation);
            }
        }
    }
}