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
    /// The analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Tool : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Tool> ValueComparer => ToolEqualityComparer.Instance;

        public bool ValueEquals(Tool other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Tool;
            }
        }

        /// <summary>
        /// The analysis tool that was run.
        /// </summary>
        [DataMember(Name = "driver", IsRequired = true)]
        public virtual ToolComponent Driver { get; set; }

        /// <summary>
        /// Tool extensions that contributed to or reconfigured the analysis tool that was run.
        /// </summary>
        [DataMember(Name = "extensions", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ToolComponent> Extensions { get; set; }

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool" /> class from the supplied values.
        /// </summary>
        /// <param name="driver">
        /// An initialization value for the <see cref="P:Driver" /> property.
        /// </param>
        /// <param name="extensions">
        /// An initialization value for the <see cref="P:Extensions" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Tool(ToolComponent driver, IEnumerable<ToolComponent> extensions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(driver, extensions, properties);
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

            Init(other.Driver, other.Extensions, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Tool DeepClone()
        {
            return (Tool)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Tool(this);
        }

        protected virtual void Init(ToolComponent driver, IEnumerable<ToolComponent> extensions, IDictionary<string, SerializedPropertyInfo> properties)
        {
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