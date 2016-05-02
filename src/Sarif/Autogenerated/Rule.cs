// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes an analysis rule.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.19.0.0")]
    public partial class Rule : IRule, ISarifNode, IEquatable<Rule>
    {
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
        public string Name { get; set; }

        /// <summary>
        /// A concise description of the rule. Should be a single sentence that is understandable when visible space is limited to a single line of text.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public string ShortDescription { get; set; }

        /// <summary>
        /// A string that describes the rule. Should, as far as possible, provide details sufficient to enable resolution of any problem indicated by the result.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public string FullDescription { get; set; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair shall consist of plain text interspersed with placeholders, which can be used to format a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "messageFormats", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> MessageFormats { get; set; }

        /// <summary>
        /// A value specifying the default severity level of the notification.
        /// </summary>
        [DataMember(Name = "defaultLevel", IsRequired = false, EmitDefaultValue = false)]
        public ResultLevel DefaultLevel { get; set; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        [DataMember(Name = "helpUri", IsRequired = false, EmitDefaultValue = false)]
        public Uri HelpUri { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the rule.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the rule.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Rule);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Id != null)
                {
                    result = (result * 31) + Id.GetHashCode();
                }

                if (Name != null)
                {
                    result = (result * 31) + Name.GetHashCode();
                }

                if (ShortDescription != null)
                {
                    result = (result * 31) + ShortDescription.GetHashCode();
                }

                if (FullDescription != null)
                {
                    result = (result * 31) + FullDescription.GetHashCode();
                }

                if (MessageFormats != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_0 in MessageFormats)
                    {
                        xor_0 ^= value_0.Key.GetHashCode();
                        if (value_0.Value != null)
                        {
                            xor_0 ^= value_0.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                result = (result * 31) + DefaultLevel.GetHashCode();
                if (HelpUri != null)
                {
                    result = (result * 31) + HelpUri.GetHashCode();
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_1 in Properties)
                    {
                        xor_1 ^= value_1.Key.GetHashCode();
                        if (value_1.Value != null)
                        {
                            xor_1 ^= value_1.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (Tags != null)
                {
                    foreach (var value_2 in Tags)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(Rule other)
        {
            if (other == null)
            {
                return false;
            }

            if (Id != other.Id)
            {
                return false;
            }

            if (Name != other.Name)
            {
                return false;
            }

            if (ShortDescription != other.ShortDescription)
            {
                return false;
            }

            if (FullDescription != other.FullDescription)
            {
                return false;
            }

            if (!Object.ReferenceEquals(MessageFormats, other.MessageFormats))
            {
                if (MessageFormats == null || other.MessageFormats == null || MessageFormats.Count != other.MessageFormats.Count)
                {
                    return false;
                }

                foreach (var value_0 in MessageFormats)
                {
                    string value_1;
                    if (!other.MessageFormats.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (DefaultLevel != other.DefaultLevel)
            {
                return false;
            }

            if (HelpUri != other.HelpUri)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Properties, other.Properties))
            {
                if (Properties == null || other.Properties == null || Properties.Count != other.Properties.Count)
                {
                    return false;
                }

                foreach (var value_2 in Properties)
                {
                    string value_3;
                    if (!other.Properties.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (value_2.Value != value_3)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Tags, other.Tags))
            {
                if (Tags == null || other.Tags == null)
                {
                    return false;
                }

                if (Tags.Count != other.Tags.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Tags.Count; ++index_0)
                {
                    if (Tags[index_0] != other.Tags[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

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
        /// <param name="messageFormats">
        /// An initialization value for the <see cref="P: MessageFormats" /> property.
        /// </param>
        /// <param name="defaultLevel">
        /// An initialization value for the <see cref="P: DefaultLevel" /> property.
        /// </param>
        /// <param name="helpUri">
        /// An initialization value for the <see cref="P: HelpUri" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Rule(string id, string name, string shortDescription, string fullDescription, IDictionary<string, string> messageFormats, ResultLevel defaultLevel, Uri helpUri, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(id, name, shortDescription, fullDescription, messageFormats, defaultLevel, helpUri, properties, tags);
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

            Init(other.Id, other.Name, other.ShortDescription, other.FullDescription, other.MessageFormats, other.DefaultLevel, other.HelpUri, other.Properties, other.Tags);
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

        private void Init(string id, string name, string shortDescription, string fullDescription, IDictionary<string, string> messageFormats, ResultLevel defaultLevel, Uri helpUri, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Id = id;
            Name = name;
            ShortDescription = shortDescription;
            FullDescription = fullDescription;
            if (messageFormats != null)
            {
                MessageFormats = new Dictionary<string, string>(messageFormats);
            }

            DefaultLevel = defaultLevel;
            if (helpUri != null)
            {
                HelpUri = new Uri(helpUri.OriginalString, helpUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in tags)
                {
                    destination_0.Add(value_0);
                }

                Tags = destination_0;
            }
        }
    }
}