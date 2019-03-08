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
    /// Provides localized strings for the current run in a single language.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Translation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Translation> ValueComparer => TranslationEqualityComparer.Instance;

        public bool ValueEquals(Translation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Translation;
            }
        }

        /// <summary>
        /// The translation language in ISO 639 format, e.g., 'en-US'.
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

        /// <summary>
        /// Provides localized message strings for a single tool component in a single language.
        /// </summary>
        [DataMember(Name = "toolComponentTranslations", IsRequired = false, EmitDefaultValue = false)]
        public IList<ToolComponentTranslation> ToolComponentTranslations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the translation.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation" /> class.
        /// </summary>
        public Translation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation" /> class from the supplied values.
        /// </summary>
        /// <param name="language">
        /// An initialization value for the <see cref="P:Language" /> property.
        /// </param>
        /// <param name="toolComponentTranslations">
        /// An initialization value for the <see cref="P:ToolComponentTranslations" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Translation(string language, IEnumerable<ToolComponentTranslation> toolComponentTranslations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(language, toolComponentTranslations, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Translation(Translation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Language, other.ToolComponentTranslations, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Translation DeepClone()
        {
            return (Translation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Translation(this);
        }

        private void Init(string language, IEnumerable<ToolComponentTranslation> toolComponentTranslations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Language = language;
            if (toolComponentTranslations != null)
            {
                var destination_0 = new List<ToolComponentTranslation>();
                foreach (var value_0 in toolComponentTranslations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ToolComponentTranslation(value_0));
                    }
                }

                ToolComponentTranslations = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}