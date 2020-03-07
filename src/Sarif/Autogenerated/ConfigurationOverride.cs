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
    /// Information about how a specific rule or notification was reconfigured at runtime.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ConfigurationOverride : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ConfigurationOverride> ValueComparer => ConfigurationOverrideEqualityComparer.Instance;

        public bool ValueEquals(ConfigurationOverride other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ConfigurationOverride;
            }
        }

        /// <summary>
        /// Specifies how the rule or notification was configured during the scan.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = true)]
        public virtual ReportingConfiguration Configuration { get; set; }

        /// <summary>
        /// A reference used to locate the descriptor whose configuration was overridden.
        /// </summary>
        [DataMember(Name = "descriptor", IsRequired = true)]
        public virtual ReportingDescriptorReference Descriptor { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the configuration override.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationOverride" /> class.
        /// </summary>
        public ConfigurationOverride()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationOverride" /> class from the supplied values.
        /// </summary>
        /// <param name="configuration">
        /// An initialization value for the <see cref="P:Configuration" /> property.
        /// </param>
        /// <param name="descriptor">
        /// An initialization value for the <see cref="P:Descriptor" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ConfigurationOverride(ReportingConfiguration configuration, ReportingDescriptorReference descriptor, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(configuration, descriptor, properties);
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

            Init(other.Configuration, other.Descriptor, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ConfigurationOverride DeepClone()
        {
            return (ConfigurationOverride)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ConfigurationOverride(this);
        }

        protected virtual void Init(ReportingConfiguration configuration, ReportingDescriptorReference descriptor, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (configuration != null)
            {
                Configuration = new ReportingConfiguration(configuration);
            }

            if (descriptor != null)
            {
                Descriptor = new ReportingDescriptorReference(descriptor);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}