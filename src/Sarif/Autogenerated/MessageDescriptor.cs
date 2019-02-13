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
    /// Describes a specific message raised by the tool, as part of the analysis it provides, its configuration or runtime execution.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class MessageDescriptor : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<MessageDescriptor> ValueComparer => MessageDescriptorEqualityComparer.Instance;

        public bool ValueEquals(MessageDescriptor other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.MessageDescriptor;
            }
        }

        /// <summary>
        /// A stable, opaque identifier for the message.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// An array of stable, opaque identifiers by which this message was known in some previous version of the analysis tool.
        /// </summary>
        [DataMember(Name = "deprecatedIds", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> DeprecatedIds { get; set; }

        /// <summary>
        /// A message identifier that is understandable to an end user.
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
        public IDictionary<string, MultiformatMessageString> MessageStrings { get; set; }

        /// <summary>
        /// Default rule information.
        /// </summary>
        [DataMember(Name = "defaultConfiguration", IsRequired = false, EmitDefaultValue = false)]
        public RuleConfiguration DefaultConfiguration { get; set; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        [DataMember(Name = "helpUri", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
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
        /// Initializes a new instance of the <see cref="MessageDescriptor" /> class.
        /// </summary>
        public MessageDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDescriptor" /> class from the supplied values.
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
        /// <param name="defaultConfiguration">
        /// An initialization value for the <see cref="P:DefaultConfiguration" /> property.
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
        public MessageDescriptor(string id, IEnumerable<string> deprecatedIds, Message name, Message shortDescription, Message fullDescription, IDictionary<string, MultiformatMessageString> messageStrings, RuleConfiguration defaultConfiguration, Uri helpUri, Message help, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, deprecatedIds, name, shortDescription, fullDescription, messageStrings, defaultConfiguration, helpUri, help, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDescriptor" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public MessageDescriptor(MessageDescriptor other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.DeprecatedIds, other.Name, other.ShortDescription, other.FullDescription, other.MessageStrings, other.DefaultConfiguration, other.HelpUri, other.Help, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public MessageDescriptor DeepClone()
        {
            return (MessageDescriptor)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new MessageDescriptor(this);
        }

        private void Init(string id, IEnumerable<string> deprecatedIds, Message name, Message shortDescription, Message fullDescription, IDictionary<string, MultiformatMessageString> messageStrings, RuleConfiguration defaultConfiguration, Uri helpUri, Message help, IDictionary<string, SerializedPropertyInfo> properties)
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
                MessageStrings = new Dictionary<string, MultiformatMessageString>();
                foreach (var value_1 in messageStrings)
                {
                    MessageStrings.Add(value_1.Key, new MultiformatMessageString(value_1.Value));
                }
            }

            if (defaultConfiguration != null)
            {
                DefaultConfiguration = new RuleConfiguration(defaultConfiguration);
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