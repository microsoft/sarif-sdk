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
    /// An area within an image.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Rectangle : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Rectangle> ValueComparer => RectangleEqualityComparer.Instance;

        public bool ValueEquals(Rectangle other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Rectangle;
            }
        }

        /// <summary>
        /// The Y coordinate of the top edge of the rectangle, measured in the image's natural units.
        /// </summary>
        [DataMember(Name = "top", IsRequired = false, EmitDefaultValue = false)]
        public virtual double Top { get; set; }

        /// <summary>
        /// The X coordinate of the left edge of the rectangle, measured in the image's natural units.
        /// </summary>
        [DataMember(Name = "left", IsRequired = false, EmitDefaultValue = false)]
        public virtual double Left { get; set; }

        /// <summary>
        /// The Y coordinate of the bottom edge of the rectangle, measured in the image's natural units.
        /// </summary>
        [DataMember(Name = "bottom", IsRequired = false, EmitDefaultValue = false)]
        public virtual double Bottom { get; set; }

        /// <summary>
        /// The X coordinate of the right edge of the rectangle, measured in the image's natural units.
        /// </summary>
        [DataMember(Name = "right", IsRequired = false, EmitDefaultValue = false)]
        public virtual double Right { get; set; }

        /// <summary>
        /// A message relevant to the rectangle.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Message { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the rectangle.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle" /> class.
        /// </summary>
        public Rectangle()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle" /> class from the supplied values.
        /// </summary>
        /// <param name="top">
        /// An initialization value for the <see cref="P:Top" /> property.
        /// </param>
        /// <param name="left">
        /// An initialization value for the <see cref="P:Left" /> property.
        /// </param>
        /// <param name="bottom">
        /// An initialization value for the <see cref="P:Bottom" /> property.
        /// </param>
        /// <param name="right">
        /// An initialization value for the <see cref="P:Right" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Rectangle(double top, double left, double bottom, double right, Message message, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(top, left, bottom, right, message, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Rectangle(Rectangle other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Top, other.Left, other.Bottom, other.Right, other.Message, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Rectangle DeepClone()
        {
            return (Rectangle)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Rectangle(this);
        }

        protected virtual void Init(double top, double left, double bottom, double right, Message message, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
            if (message != null)
            {
                Message = new Message(message);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}