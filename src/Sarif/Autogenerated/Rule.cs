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
    /// Describes an analysis rule.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
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
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; }

        /// <summary>
        /// A rule identifier that is understandable to an end user.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public Message Name { get; set; }

        /// <summary>
        /// A concise description of the rule. Should be a single sentence that is understandable when visible space is limited to a single line of text.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public Message ShortDescription { get; set; }

        /// <summary>
        /// A description of the rule. Should, as far as possible, provide details sufficient to enable resolution of any problem indicated by the result.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public Message FullDescription { get; set; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair consists of plain text interspersed with placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "messageStrings", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> MessageStrings { get; set; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair consists of rich text interspersed with placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "richMessageStrings", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> RichMessageStrings { get; set; }

        /// <summary>
        /// Information about the rule that can be configured at runtime.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public RuleConfiguration Configuration { get; set; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        [DataMember(Name = "helpUri", IsRequired = false, EmitDefaultValue = false)]
        public Uri HelpUri { get; set; }

        /// <summary>
        /// Provides the primary documentation for the rule, useful when there is no online documentation.
        /// </summary>
        [DataMember(Name = "help", IsRequired = false, EmitDefaultValue = false)]
        public Message Help { get; set; }

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
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="name">
        /// An initialization value for the <see cref="P: Name" /> property.
        /// </param>
        /// <param name="shortDescription">
        /// An initialization value for the <see cref="P: ShortDescription" /> property.
        /// </param>
        /// <param name="fullDescription">
        /// An initialization value for the <see cref="P: FullDescription" /> property.
        /// </param>
        /// <param name="messageStrings">
        /// An initialization value for the <see cref="P: MessageStrings" /> property.
        /// </param>
        /// <param name="richMessageStrings">
        /// An initialization value for the <see cref="P: RichMessageStrings" /> property.
        /// </param>
        /// <param name="configuration">
        /// An initialization value for the <see cref="P: Configuration" /> property.
        /// </param>
        /// <param name="helpUri">
        /// An initialization value for the <see cref="P: HelpUri" /> property.
        /// </param>
        /// <param name="help">
        /// An initialization value for the <see cref="P: Help" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
<<<<<<< HEAD
        public Rule(string id, string name, string shortDescription, string fullDescription, string richDescription, IDictionary<string, string> messageTemplates, IDictionary<string, string> richMessageTemplates, RuleConfiguration configuration, Uri helpUri, string help, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, name, shortDescription, fullDescription, richDescription, messageTemplates, richMessageTemplates, configuration, helpUri, help, properties);
=======
        public Rule(string id, Message name, Message shortDescription, Message fullDescription, IDictionary<string, string> messageStrings, IDictionary<string, string> richMessageStrings, RuleConfiguration configuration, ResultLevel defaultLevel, Uri helpUri, Message help, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, name, shortDescription, fullDescription, messageStrings, richMessageStrings, configuration, defaultLevel, helpUri, help, properties);
>>>>>>> sarif-v2
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

<<<<<<< HEAD
            Init(other.Id, other.Name, other.ShortDescription, other.FullDescription, other.RichDescription, other.MessageTemplates, other.RichMessageTemplates, other.Configuration, other.HelpUri, other.Help, other.Properties);
=======
            Init(other.Id, other.Name, other.ShortDescription, other.FullDescription, other.MessageStrings, other.RichMessageStrings, other.Configuration, other.DefaultLevel, other.HelpUri, other.Help, other.Properties);
>>>>>>> sarif-v2
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

<<<<<<< HEAD
        private void Init(string id, string name, string shortDescription, string fullDescription, string richDescription, IDictionary<string, string> messageTemplates, IDictionary<string, string> richMessageTemplates, RuleConfiguration configuration, Uri helpUri, string help, IDictionary<string, SerializedPropertyInfo> properties)
=======
        private void Init(string id, Message name, Message shortDescription, Message fullDescription, IDictionary<string, string> messageStrings, IDictionary<string, string> richMessageStrings, RuleConfiguration configuration, ResultLevel defaultLevel, Uri helpUri, Message help, IDictionary<string, SerializedPropertyInfo> properties)
>>>>>>> sarif-v2
        {
            Id = id;
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