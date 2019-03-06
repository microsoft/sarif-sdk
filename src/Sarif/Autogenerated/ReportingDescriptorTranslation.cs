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
    /// Provides localized message strings for a reporting descriptor in a single language.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ReportingDescriptorTranslation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ReportingDescriptorTranslation> ValueComparer => ReportingDescriptorTranslationEqualityComparer.Instance;

        public bool ValueEquals(ReportingDescriptorTranslation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ReportingDescriptorTranslation;
            }
        }

        /// <summary>
        /// The stable, opaque identifier of the reporting descriptor to which this translation belongs, matching reportingDescriptor.id.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// The unique identifier in the form of a GUID of the reporting descriptor to which this translation belongs, matching reportingDescriptor.guid.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public string Guid { get; set; }

        /// <summary>
        /// A concise description of the report. Should be a single sentence that is understandable when visible space is limited to a single line of text.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public MultiformatMessageString ShortDescription { get; set; }

        /// <summary>
        /// A description of the report. Should, as far as possible, provide details sufficient to enable resolution of any problem indicated by the result.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public MultiformatMessageString FullDescription { get; set; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. Each value is a multiformatMessageString object, which holds message strings in plain text and (optionally) Markdown format. The property names are a subset of the property names in the messageStrings property of the reportingDescriptor object to which this translation belongs.
        /// </summary>
        [DataMember(Name = "messageStrings", IsRequired = false, EmitDefaultValue = false)]
        public object MessageStrings { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about reportingDescriptorTranslation.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorTranslation" /> class.
        /// </summary>
        public ReportingDescriptorTranslation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorTranslation" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="shortDescription">
        /// An initialization value for the <see cref="P:ShortDescription" /> property.
        /// </param>
        /// <param name="fullDescription">
        /// An initialization value for the <see cref="P:FullDescription" /> property.
        /// </param>
        /// <param name="messageStrings">
        /// An initialization value for the <see cref="P:MessageStrings" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ReportingDescriptorTranslation(string id, string guid, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, object messageStrings, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, guid, shortDescription, fullDescription, messageStrings, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorTranslation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ReportingDescriptorTranslation(ReportingDescriptorTranslation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Guid, other.ShortDescription, other.FullDescription, other.MessageStrings, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ReportingDescriptorTranslation DeepClone()
        {
            return (ReportingDescriptorTranslation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ReportingDescriptorTranslation(this);
        }

        private void Init(string id, string guid, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, object messageStrings, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            Guid = guid;
            if (shortDescription != null)
            {
                ShortDescription = new MultiformatMessageString(shortDescription);
            }

            if (fullDescription != null)
            {
                FullDescription = new MultiformatMessageString(fullDescription);
            }

            MessageStrings = messageStrings;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}