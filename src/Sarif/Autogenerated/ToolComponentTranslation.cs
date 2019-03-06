// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Provides localized message strings for a tool component in a single language.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ToolComponentTranslation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ToolComponentTranslation> ValueComparer => ToolComponentTranslationEqualityComparer.Instance;

        public bool ValueEquals(ToolComponentTranslation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ToolComponentTranslation;
            }
        }

        /// <summary>
        /// The unique identifier for the tool component in the form of a GUID, matching toolComponent.guid.
        /// </summary>
        [DataMember(Name = "toolComponentGuid", IsRequired = false, EmitDefaultValue = false)]
        public string ToolComponentGuid { get; set; }

        /// <summary>
        /// The location of the translation.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactLocation Location { get; set; }

        /// <summary>
        /// The semantic version of the tool component for which the translation was made.
        /// </summary>
        [DataMember(Name = "semanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string SemanticVersion { get; set; }

        /// <summary>
        /// True if this object contains a subset of the strings defined by the tool component.
        /// </summary>
        [DataMember(Name = "partialTranslation", IsRequired = false, EmitDefaultValue = false)]
        public bool PartialTranslation { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a message identifier and each of whose values is a multiformatMessageString object, which holds message strings in plain text and (optionally) Markdown format. The strings can include placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments. The property names are a subset of the property names in the globalMessageStrings property of the toolComponent object to which this translation belongs.
        /// </summary>
        [DataMember(Name = "globalMessageStrings", IsRequired = false, EmitDefaultValue = false)]
        public object GlobalMessageStrings { get; set; }

        /// <summary>
        /// Provides an array of translations for a reporting descriptor in a available languages.
        /// </summary>
        [DataMember(Name = "reportingDescriptors", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptorTranslation> ReportingDescriptors { get; set; }

        /// <summary>
        /// Provides an array of translations for a notification descriptor in a available languages.
        /// </summary>
        [DataMember(Name = "notificationDescriptors", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptorTranslation> NotificationDescriptors { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the translationComponentTranslation.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponentTranslation" /> class.
        /// </summary>
        public ToolComponentTranslation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponentTranslation" /> class from the supplied values.
        /// </summary>
        /// <param name="toolComponentGuid">
        /// An initialization value for the <see cref="P:ToolComponentGuid" /> property.
        /// </param>
        /// <param name="location">
        /// An initialization value for the <see cref="P:Location" /> property.
        /// </param>
        /// <param name="semanticVersion">
        /// An initialization value for the <see cref="P:SemanticVersion" /> property.
        /// </param>
        /// <param name="partialTranslation">
        /// An initialization value for the <see cref="P:PartialTranslation" /> property.
        /// </param>
        /// <param name="globalMessageStrings">
        /// An initialization value for the <see cref="P:GlobalMessageStrings" /> property.
        /// </param>
        /// <param name="reportingDescriptors">
        /// An initialization value for the <see cref="P:ReportingDescriptors" /> property.
        /// </param>
        /// <param name="notificationDescriptors">
        /// An initialization value for the <see cref="P:NotificationDescriptors" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ToolComponentTranslation(string toolComponentGuid, ArtifactLocation location, string semanticVersion, bool partialTranslation, object globalMessageStrings, IEnumerable<ReportingDescriptorTranslation> reportingDescriptors, IEnumerable<ReportingDescriptorTranslation> notificationDescriptors, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(toolComponentGuid, location, semanticVersion, partialTranslation, globalMessageStrings, reportingDescriptors, notificationDescriptors, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponentTranslation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ToolComponentTranslation(ToolComponentTranslation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.ToolComponentGuid, other.Location, other.SemanticVersion, other.PartialTranslation, other.GlobalMessageStrings, other.ReportingDescriptors, other.NotificationDescriptors, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ToolComponentTranslation DeepClone()
        {
            return (ToolComponentTranslation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ToolComponentTranslation(this);
        }

        private void Init(string toolComponentGuid, ArtifactLocation location, string semanticVersion, bool partialTranslation, object globalMessageStrings, IEnumerable<ReportingDescriptorTranslation> reportingDescriptors, IEnumerable<ReportingDescriptorTranslation> notificationDescriptors, IDictionary<string, SerializedPropertyInfo> properties)
        {
            ToolComponentGuid = toolComponentGuid;
            if (location != null)
            {
                Location = new ArtifactLocation(location);
            }

            SemanticVersion = semanticVersion;
            PartialTranslation = partialTranslation;
            GlobalMessageStrings = globalMessageStrings;
            if (reportingDescriptors != null)
            {
                var destination_0 = new List<ReportingDescriptorTranslation>();
                foreach (var value_0 in reportingDescriptors)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ReportingDescriptorTranslation(value_0));
                    }
                }

                ReportingDescriptors = destination_0;
            }

            if (notificationDescriptors != null)
            {
                var destination_1 = new List<ReportingDescriptorTranslation>();
                foreach (var value_1 in notificationDescriptors)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new ReportingDescriptorTranslation(value_1));
                    }
                }

                NotificationDescriptors = destination_1;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}