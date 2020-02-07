// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Describes an analysis rule.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class RuleVersionOne : PropertyBagHolderVersionOne, IRuleVersionOne, ISarifNodeVersionOne
    {
        public static IEqualityComparer<RuleVersionOne> ValueComparer => RuleVersionOneEqualityComparer.Instance;

        public bool ValueEquals(RuleVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.RuleVersionOne;
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
        /// A value specifying whether a rule is enabled.
        /// </summary>
        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public RuleConfigurationVersionOne Configuration { get; set; }

        /// <summary>
        /// A value specifying the default severity level of the result.
        /// </summary>
        [DataMember(Name = "defaultLevel", IsRequired = false, EmitDefaultValue = false)]
        public ResultLevelVersionOne DefaultLevel { get; set; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        [DataMember(Name = "helpUri", IsRequired = false, EmitDefaultValue = false)]
        public Uri HelpUri { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the rule.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVersionOne" /> class.
        /// </summary>
        public RuleVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVersionOne" /> class from the supplied values.
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
        /// <param name="configuration">
        /// An initialization value for the <see cref="P: Configuration" /> property.
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
        public RuleVersionOne(string id, string name, string shortDescription, string fullDescription, IDictionary<string, string> messageFormats, RuleConfigurationVersionOne configuration, ResultLevelVersionOne defaultLevel, Uri helpUri, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, name, shortDescription, fullDescription, messageFormats, configuration, defaultLevel, helpUri, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public RuleVersionOne(RuleVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Name, other.ShortDescription, other.FullDescription, other.MessageFormats, other.Configuration, other.DefaultLevel, other.HelpUri, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public RuleVersionOne DeepClone()
        {
            return (RuleVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new RuleVersionOne(this);
        }

        private void Init(string id, string name, string shortDescription, string fullDescription, IDictionary<string, string> messageFormats, RuleConfigurationVersionOne configuration, ResultLevelVersionOne defaultLevel, Uri helpUri, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            Name = name;
            ShortDescription = shortDescription;
            FullDescription = fullDescription;
            if (messageFormats != null)
            {
                MessageFormats = new Dictionary<string, string>(messageFormats);
            }

            Configuration = configuration;
            DefaultLevel = defaultLevel;
            if (helpUri != null)
            {
                HelpUri = new Uri(helpUri.OriginalString, helpUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}