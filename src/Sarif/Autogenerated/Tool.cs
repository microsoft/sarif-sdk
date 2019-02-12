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
    /// The analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class Tool : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Tool> ValueComparer => ToolEqualityComparer.Instance;

        public bool ValueEquals(Tool other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Tool;
            }
        }

        /// <summary>
        /// The tool language (expressed as an ISO 649 two-letter lowercase culture code) and region (expressed as an ISO 3166 two-letter uppercase subculture code associated with a country or region).
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("en-US")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Language { get; set; }

        /// <summary>
        /// The analysis tool that was run.
        /// </summary>
        [DataMember(Name = "driver", IsRequired = true)]
        public ToolComponent Driver { get; set; }

        /// <summary>
        /// Tool extensions that contributed to or reconfigured the analysis tool that was run.
        /// </summary>
        [DataMember(Name = "extensions", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ToolComponent> Extensions { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the tool.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class.
        /// </summary>
        public Tool()
        {
            Language = "en-US";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class from the supplied values.
        /// </summary>
        /// <param name="language">
        /// An initialization value for the <see cref="P:Language" /> property.
        /// </param>
        /// <param name="driver">
        /// An initialization value for the <see cref="P:Driver" /> property.
        /// </param>
        /// <param name="extensions">
        /// An initialization value for the <see cref="P:Extensions" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Tool(string language, ToolComponent driver, IEnumerable<ToolComponent> extensions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(language, driver, extensions, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Tool(Tool other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Language, other.Driver, other.Extensions, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Tool DeepClone()
        {
            return (Tool)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Tool(this);
        }

        private void Init(string language, ToolComponent driver, IEnumerable<ToolComponent> extensions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Language = language;
            if (driver != null)
            {
                Driver = new ToolComponent(driver);
            }

            if (extensions != null)
            {
                var destination_0 = new List<ToolComponent>();
                foreach (var value_0 in extensions)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ToolComponent(value_0));
                    }
                }

                Extensions = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}