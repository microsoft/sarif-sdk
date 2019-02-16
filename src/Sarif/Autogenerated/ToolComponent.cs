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
    /// A component, such as a plug-in or the default driver, of the analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class ToolComponent : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ToolComponent> ValueComparer => ToolComponentEqualityComparer.Instance;

        public bool ValueEquals(ToolComponent other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ToolComponent;
            }
        }

        /// <summary>
        /// The name of the component.
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// The name of the component along with its version and any other useful identifying information, such as its locale.
        /// </summary>
        [DataMember(Name = "fullName", IsRequired = false, EmitDefaultValue = false)]
        public string FullName { get; set; }

        /// <summary>
        /// The component version, in whatever format the component natively provides.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// The component version in the format specified by Semantic Versioning 2.0.
        /// </summary>
        [DataMember(Name = "semanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string SemanticVersion { get; set; }

        /// <summary>
        /// The binary version of the component's primary executable file expressed as four non-negative integers separated by a period (for operating systems that express file versions in this way).
        /// </summary>
        [DataMember(Name = "dottedQuadFileVersion", IsRequired = false, EmitDefaultValue = false)]
        public string DottedQuadFileVersion { get; set; }

        /// <summary>
        /// The absolute URI from which the component can be downloaded.
        /// </summary>
        [DataMember(Name = "downloadUri", IsRequired = false, EmitDefaultValue = false)]
        public Uri DownloadUri { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a resource identifier and each of whose values is a multiformatMessageString object, which holds message strings in plain text and (optionally) Markdown format. The strings can include placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "globalMessageStrings", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, MultiformatMessageString> GlobalMessageStrings { get; set; }

        /// <summary>
        /// An array of reportDescriptor objects relevant to the notifications related to the configuration and runtime execution of the component.
        /// </summary>
        [DataMember(Name = "notificationDescriptors", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptor> NotificationDescriptors { get; set; }

        /// <summary>
        /// An array of reportDescriptor objects relevant to the analysis performed by the component.
        /// </summary>
        [DataMember(Name = "ruleDescriptors", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptor> RuleDescriptors { get; set; }

        /// <summary>
        /// The index within the run files array of the file object associated with the component.
        /// </summary>
        [DataMember(Name = "artifactIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int ArtifactIndex { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the component.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponent" /> class.
        /// </summary>
        public ToolComponent()
        {
            ArtifactIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponent" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
        /// </param>
        /// <param name="fullName">
        /// An initialization value for the <see cref="P:FullName" /> property.
        /// </param>
        /// <param name="version">
        /// An initialization value for the <see cref="P:Version" /> property.
        /// </param>
        /// <param name="semanticVersion">
        /// An initialization value for the <see cref="P:SemanticVersion" /> property.
        /// </param>
        /// <param name="dottedQuadFileVersion">
        /// An initialization value for the <see cref="P:DottedQuadFileVersion" /> property.
        /// </param>
        /// <param name="downloadUri">
        /// An initialization value for the <see cref="P:DownloadUri" /> property.
        /// </param>
        /// <param name="globalMessageStrings">
        /// An initialization value for the <see cref="P:GlobalMessageStrings" /> property.
        /// </param>
        /// <param name="notificationDescriptors">
        /// An initialization value for the <see cref="P:NotificationDescriptors" /> property.
        /// </param>
        /// <param name="ruleDescriptors">
        /// An initialization value for the <see cref="P:RuleDescriptors" /> property.
        /// </param>
        /// <param name="artifactIndex">
        /// An initialization value for the <see cref="P:ArtifactIndex" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ToolComponent(string name, string fullName, string version, string semanticVersion, string dottedQuadFileVersion, Uri downloadUri, IDictionary<string, MultiformatMessageString> globalMessageStrings, IEnumerable<ReportingDescriptor> notificationDescriptors, IEnumerable<ReportingDescriptor> ruleDescriptors, int artifactIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(name, fullName, version, semanticVersion, dottedQuadFileVersion, downloadUri, globalMessageStrings, notificationDescriptors, ruleDescriptors, artifactIndex, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponent" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ToolComponent(ToolComponent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.FullName, other.Version, other.SemanticVersion, other.DottedQuadFileVersion, other.DownloadUri, other.GlobalMessageStrings, other.NotificationDescriptors, other.RuleDescriptors, other.ArtifactIndex, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ToolComponent DeepClone()
        {
            return (ToolComponent)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ToolComponent(this);
        }

        private void Init(string name, string fullName, string version, string semanticVersion, string dottedQuadFileVersion, Uri downloadUri, IDictionary<string, MultiformatMessageString> globalMessageStrings, IEnumerable<ReportingDescriptor> notificationDescriptors, IEnumerable<ReportingDescriptor> ruleDescriptors, int artifactIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Name = name;
            FullName = fullName;
            Version = version;
            SemanticVersion = semanticVersion;
            DottedQuadFileVersion = dottedQuadFileVersion;
            if (downloadUri != null)
            {
                DownloadUri = new Uri(downloadUri.OriginalString, downloadUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (globalMessageStrings != null)
            {
                GlobalMessageStrings = new Dictionary<string, MultiformatMessageString>();
                foreach (var value_0 in globalMessageStrings)
                {
                    GlobalMessageStrings.Add(value_0.Key, new MultiformatMessageString(value_0.Value));
                }
            }

            if (notificationDescriptors != null)
            {
                var destination_0 = new List<ReportingDescriptor>();
                foreach (var value_1 in notificationDescriptors)
                {
                    if (value_1 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ReportingDescriptor(value_1));
                    }
                }

                NotificationDescriptors = destination_0;
            }

            if (ruleDescriptors != null)
            {
                var destination_1 = new List<ReportingDescriptor>();
                foreach (var value_2 in ruleDescriptors)
                {
                    if (value_2 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new ReportingDescriptor(value_2));
                    }
                }

                RuleDescriptors = destination_1;
            }

            ArtifactIndex = artifactIndex;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}