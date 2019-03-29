// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Information about how a specific tool report was reconfigured at runtime.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ConfigurationOverride : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ConfigurationOverride> ValueComparer => ConfigurationOverrideEqualityComparer.Instance;

        public bool ValueEquals(ConfigurationOverride other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ConfigurationOverride;
            }
        }

        /// <summary>
        /// Specifies how the report was configured during the scan.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public ReportingConfiguration Configuration { get; set; }

        /// <summary>
        /// The index within the toolComponent.notificationDescriptors array of the reportingDescriptor associated with this override.
        /// </summary>
        [DataMember(Name = "notificationIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int NotificationIndex { get; set; }

        /// <summary>
        /// The index within the toolComponent.ruleDescriptors array of the reportingDescriptor associated with this override.
        /// </summary>
        [DataMember(Name = "ruleIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int RuleIndex { get; set; }

        /// <summary>
        /// The index within the run.tool.extensions array of the toolComponent object which describes the plug-in or tool extension that produced the report.
        /// </summary>
        [DataMember(Name = "extensionIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int ExtensionIndex { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the reporting configuration.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationOverride" /> class.
        /// </summary>
        public ConfigurationOverride()
        {
            NotificationIndex = -1;
            RuleIndex = -1;
            ExtensionIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationOverride" /> class from the supplied values.
        /// </summary>
        /// <param name="configuration">
        /// An initialization value for the <see cref="P:Configuration" /> property.
        /// </param>
        /// <param name="notificationIndex">
        /// An initialization value for the <see cref="P:NotificationIndex" /> property.
        /// </param>
        /// <param name="ruleIndex">
        /// An initialization value for the <see cref="P:RuleIndex" /> property.
        /// </param>
        /// <param name="extensionIndex">
        /// An initialization value for the <see cref="P:ExtensionIndex" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ConfigurationOverride(ReportingConfiguration configuration, int notificationIndex, int ruleIndex, int extensionIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(configuration, notificationIndex, ruleIndex, extensionIndex, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationOverride" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ConfigurationOverride(ConfigurationOverride other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Configuration, other.NotificationIndex, other.RuleIndex, other.ExtensionIndex, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ConfigurationOverride DeepClone()
        {
            return (ConfigurationOverride)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ConfigurationOverride(this);
        }

        private void Init(ReportingConfiguration configuration, int notificationIndex, int ruleIndex, int extensionIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (configuration != null)
            {
                Configuration = new ReportingConfiguration(configuration);
            }

            NotificationIndex = notificationIndex;
            RuleIndex = ruleIndex;
            ExtensionIndex = extensionIndex;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}