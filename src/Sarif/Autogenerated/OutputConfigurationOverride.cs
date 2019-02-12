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
    /// Information about how a specific tool output was reconfigured at runtime.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class OutputConfigurationOverride : ISarifNode
    {
        public static IEqualityComparer<OutputConfigurationOverride> ValueComparer => OutputConfigurationOverrideEqualityComparer.Instance;

        public bool ValueEquals(OutputConfigurationOverride other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.OutputConfigurationOverride;
            }
        }

        /// <summary>
        /// Specifies whether the notification may produce an output during the scan.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public OutputConfiguration Configuration { get; set; }

        /// <summary>
        /// The index within the tool component notifications descriptors array of the output descriptor associated with this override.
        /// </summary>
        [DataMember(Name = "notificationIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int NotificationIndex { get; set; }

        /// <summary>
        /// The index within the tool component rule descriptors array of the output descriptor associated with this override.
        /// </summary>
        [DataMember(Name = "ruleIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int RuleIndex { get; set; }

        /// <summary>
        /// The index within the run.tool.extensions array of the tool component object which describes the plug-in or tool extension that produced the result.
        /// </summary>
        [DataMember(Name = "extensionIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int ExtensionIndex { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the notification configuration.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputConfigurationOverride" /> class.
        /// </summary>
        public OutputConfigurationOverride()
        {
            NotificationIndex = -1;
            RuleIndex = -1;
            ExtensionIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputConfigurationOverride" /> class from the supplied values.
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
        public OutputConfigurationOverride(OutputConfiguration configuration, int notificationIndex, int ruleIndex, int extensionIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(configuration, notificationIndex, ruleIndex, extensionIndex, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputConfigurationOverride" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public OutputConfigurationOverride(OutputConfigurationOverride other)
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
        public OutputConfigurationOverride DeepClone()
        {
            return (OutputConfigurationOverride)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new OutputConfigurationOverride(this);
        }

        private void Init(OutputConfiguration configuration, int notificationIndex, int ruleIndex, int extensionIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (configuration != null)
            {
                Configuration = new OutputConfiguration(configuration);
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