// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Information about a rule that can be configured at runtime.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class RuleConfiguration : ISarifNode
    {
        public static IEqualityComparer<RuleConfiguration> ValueComparer => RuleConfigurationEqualityComparer.Instance;

        public bool ValueEquals(RuleConfiguration other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.RuleConfiguration;
            }
        }

        /// <summary>
        /// Specifies whether the rule will be evaluated during the scan.
        /// </summary>
        [DataMember(Name = "enabled", IsRequired = false, EmitDefaultValue = false)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Specifies the default severity level of the result.
        /// </summary>
        [DataMember(Name = "defaultLevel", IsRequired = false, EmitDefaultValue = false)]
        public RuleConfigurationDefaultLevel DefaultLevel { get; set; }

        /// <summary>
        /// Contains configuration information specific to this rule.
        /// </summary>
        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public object Parameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleConfiguration" /> class.
        /// </summary>
        public RuleConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleConfiguration" /> class from the supplied values.
        /// </summary>
        /// <param name="enabled">
        /// An initialization value for the <see cref="P: Enabled" /> property.
        /// </param>
        /// <param name="defaultLevel">
        /// An initialization value for the <see cref="P: DefaultLevel" /> property.
        /// </param>
        /// <param name="parameters">
        /// An initialization value for the <see cref="P: Parameters" /> property.
        /// </param>
        public RuleConfiguration(bool enabled, RuleConfigurationDefaultLevel defaultLevel, object parameters)
        {
            Init(enabled, defaultLevel, parameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleConfiguration" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public RuleConfiguration(RuleConfiguration other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Enabled, other.DefaultLevel, other.Parameters);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public RuleConfiguration DeepClone()
        {
            return (RuleConfiguration)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new RuleConfiguration(this);
        }

        private void Init(bool enabled, RuleConfigurationDefaultLevel defaultLevel, object parameters)
        {
            Enabled = enabled;
            DefaultLevel = defaultLevel;
            Parameters = parameters;
        }
    }
}