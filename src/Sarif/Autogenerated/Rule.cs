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
    /// Describes an analysis rule.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class Rule : PropertyBagHolder, IRule, ISarifNode
    {
        public static IEqualityComparer<Rule> ValueComparer => RuleEqualityComparer.Instance;

        public bool ValueEquals(Rule other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Rule;
            }
        }

        /// <summary>
        /// A stable, opaque identifier for the rule.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Id { get; set; }

        /// <summary>
        /// An array of stable, opaque identifiers by which this rule was known in some previous version of the analysis tool.
        /// </summary>
        [DataMember(Name = "deprecatedIds", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> DeprecatedIds { get; set; }

        /// <summary>
        /// A rule identifier that is understandable to an end user.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Name { get; set; }

        /// <summary>
        /// A concise description of the rule. Should be a single sentence that is understandable when visible space is limited to a single line of text.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message ShortDescription { get; set; }

        /// <summary>
        /// A description of the rule. Should, as far as possible, provide details sufficient to enable resolution of any problem indicated by the result.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message FullDescription { get; set; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair consists of plain text interspersed with placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "messageStrings", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, string> MessageStrings { get; set; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair consists of rich text interspersed with placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "richMessageStrings", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, string> RichMessageStrings { get; set; }

        /// <summary>
        /// Information about the rule that can be configured at runtime.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public virtual RuleConfiguration Configuration { get; set; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        [DataMember(Name = "helpUri", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(UriConverter))]
        public virtual Uri HelpUri { get; set; }

        /// <summary>
        /// Provides the primary documentation for the rule, useful when there is no online documentation.
        /// </summary>
        [DataMember(Name = "help", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Help { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the rule.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule" /> class.
        /// </summary>
        public Rule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="deprecatedIds">
        /// An initialization value for the <see cref="P:DeprecatedIds" /> property.
        /// </param>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
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
        /// <param name="richMessageStrings">
        /// An initialization value for the <see cref="P:RichMessageStrings" /> property.
        /// </param>
        /// <param name="configuration">
        /// An initialization value for the <see cref="P:Configuration" /> property.
        /// </param>
        /// <param name="helpUri">
        /// An initialization value for the <see cref="P:HelpUri" /> property.
        /// </param>
        /// <param name="help">
        /// An initialization value for the <see cref="P:Help" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Rule(string id, IEnumerable<string> deprecatedIds, Message name, Message shortDescription, Message fullDescription, IDictionary<string, string> messageStrings, IDictionary<string, string> richMessageStrings, RuleConfiguration configuration, Uri helpUri, Message help, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, deprecatedIds, name, shortDescription, fullDescription, messageStrings, richMessageStrings, configuration, helpUri, help, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Rule(Rule other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.DeprecatedIds, other.Name, other.ShortDescription, other.FullDescription, other.MessageStrings, other.RichMessageStrings, other.Configuration, other.HelpUri, other.Help, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Rule DeepClone()
        {
            return (Rule)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Rule(this);
        }

        private void Init(string id, IEnumerable<string> deprecatedIds, Message name, Message shortDescription, Message fullDescription, IDictionary<string, string> messageStrings, IDictionary<string, string> richMessageStrings, RuleConfiguration configuration, Uri helpUri, Message help, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            if (deprecatedIds != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in deprecatedIds)
                {
                    destination_0.Add(value_0);
                }

                DeprecatedIds = destination_0;
            }

            if (name != null)
            {
                Name = new Message(name);
            }

            if (shortDescription != null)
            {
                ShortDescription = new Message(shortDescription);
            }

            if (fullDescription != null)
            {
                FullDescription = new Message(fullDescription);
            }

            if (messageStrings != null)
            {
                MessageStrings = new Dictionary<string, string>(messageStrings);
            }

            if (richMessageStrings != null)
            {
                RichMessageStrings = new Dictionary<string, string>(richMessageStrings);
            }

            if (configuration != null)
            {
                Configuration = new RuleConfiguration(configuration);
            }

            if (helpUri != null)
            {
                HelpUri = new Uri(helpUri.OriginalString, helpUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (help != null)
            {
                Help = new Message(help);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}