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
    /// Container for items that require localization.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class Resources : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Resources> ValueComparer => ResourcesEqualityComparer.Instance;

        public bool ValueEquals(Resources other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Resources;
            }
        }

        /// <summary>
        /// A dictionary, each of whose keys is a resource identifier and each of whose values is a localized string.
        /// </summary>
        [DataMember(Name = "messageStrings", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> MessageStrings { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a string and each of whose values is a 'rule' object, that describe all rules associated with an analysis tool or a specific run of an analysis tool.
        /// </summary>
        [DataMember(Name = "rules", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.RuleDictionaryConverter))]
        public IDictionary<string, Rule> Rules { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the resources.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resources" /> class.
        /// </summary>
        public Resources()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resources" /> class from the supplied values.
        /// </summary>
        /// <param name="messageStrings">
        /// An initialization value for the <see cref="P:MessageStrings" /> property.
        /// </param>
        /// <param name="rules">
        /// An initialization value for the <see cref="P:Rules" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Resources(IDictionary<string, string> messageStrings, IDictionary<string, Rule> rules, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(messageStrings, rules, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resources" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Resources(Resources other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.MessageStrings, other.Rules, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Resources DeepClone()
        {
            return (Resources)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Resources(this);
        }

        private void Init(IDictionary<string, string> messageStrings, IDictionary<string, Rule> rules, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (messageStrings != null)
            {
                MessageStrings = new Dictionary<string, string>(messageStrings);
            }

            if (rules != null)
            {
                Rules = new Dictionary<string, Rule>();
                foreach (var value_0 in rules)
                {
                    Rules.Add(value_0.Key, new Rule(value_0.Value));
                }
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}