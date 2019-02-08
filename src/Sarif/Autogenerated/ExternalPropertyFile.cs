// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class ExternalPropertyFile : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExternalPropertyFile> ValueComparer => ExternalPropertyFileEqualityComparer.Instance;

        public bool ValueEquals(ExternalPropertyFile other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExternalPropertyFile;
            }
        }

        /// <summary>
        /// The location of the external property file.
        /// </summary>
        [DataMember(Name = "fileLocation", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation FileLocation { get; set; }

        /// <summary>
        /// A stable, unique identifer for the external property file in the form of a GUID.
        /// </summary>
        [DataMember(Name = "instanceGuid", IsRequired = false, EmitDefaultValue = false)]
        public string InstanceGuid { get; set; }

        /// <summary>
        /// A non-negative integer specifying the number of items contained in the external property file.
        /// </summary>
        [DataMember(Name = "itemCount", IsRequired = false, EmitDefaultValue = false)]
        public int ItemCount { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the external property file.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFile" /> class.
        /// </summary>
        public ExternalPropertyFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFile" /> class from the supplied values.
        /// </summary>
        /// <param name="fileLocation">
        /// An initialization value for the <see cref="P:FileLocation" /> property.
        /// </param>
        /// <param name="instanceGuid">
        /// An initialization value for the <see cref="P:InstanceGuid" /> property.
        /// </param>
        /// <param name="itemCount">
        /// An initialization value for the <see cref="P:ItemCount" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ExternalPropertyFile(FileLocation fileLocation, string instanceGuid, int itemCount, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(fileLocation, instanceGuid, itemCount, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPropertyFile" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExternalPropertyFile(ExternalPropertyFile other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.FileLocation, other.InstanceGuid, other.ItemCount, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ExternalPropertyFile DeepClone()
        {
            return (ExternalPropertyFile)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExternalPropertyFile(this);
        }

        private void Init(FileLocation fileLocation, string instanceGuid, int itemCount, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (fileLocation != null)
            {
                FileLocation = new FileLocation(fileLocation);
            }

            InstanceGuid = instanceGuid;
            ItemCount = itemCount;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}