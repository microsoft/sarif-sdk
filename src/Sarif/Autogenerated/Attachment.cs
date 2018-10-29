// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A file relevant to a tool invocation or to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    public partial class Attachment : PropertyBagHolder, ISarifNode
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
        /// An array of regions of interest within the attachment.
        /// </summary>
        [DataMember(Name = "regions", IsRequired = false, EmitDefaultValue = false)]
        public IList<Region> Regions { get; set; }

        /// <summary>
        /// An array of rectangles specifying areas of interest within the image.
        /// </summary>
        [DataMember(Name = "rectangles", IsRequired = false, EmitDefaultValue = false)]
        public IList<Rectangle> Rectangles { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the attachment.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

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
        /// <param name="regions">
        /// An initialization value for the <see cref="P: Regions" /> property.
        /// </param>
        /// <param name="rectangles">
        /// An initialization value for the <see cref="P: Rectangles" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Attachment(Message description, FileLocation fileLocation, IEnumerable<Region> regions, IEnumerable<Rectangle> rectangles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(description, fileLocation, regions, rectangles, properties);
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

            Init(other.Description, other.FileLocation, other.Regions, other.Rectangles, other.Properties);
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

        private void Init(Message description, FileLocation fileLocation, IEnumerable<Region> regions, IEnumerable<Rectangle> rectangles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (description != null)
            {
                Description = new Message(description);
            }

            if (fileLocation != null)
            {
                FileLocation = new FileLocation(fileLocation);
            }

            if (regions != null)
            {
                var destination_0 = new List<Region>();
                foreach (var value_0 in regions)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Region(value_0));
                    }
                }

                Regions = destination_0;
            }

            if (rectangles != null)
            {
                var destination_1 = new List<Rectangle>();
                foreach (var value_1 in rectangles)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Rectangle(value_1));
                    }
                }

                Rectangles = destination_1;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}