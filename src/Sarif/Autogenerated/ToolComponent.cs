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
    /// A component, such as a plug-in or the driver, of the analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
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
        /// A unique identifer for the tool component in the form of a GUID.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public string Guid { get; set; }

        /// <summary>
        /// The name of the tool component.
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// The organization or company that produced the tool component.
        /// </summary>
        [DataMember(Name = "organization", IsRequired = false, EmitDefaultValue = false)]
        public string Organization { get; set; }

        /// <summary>
        /// A product suite to which the tool component belongs.
        /// </summary>
        [DataMember(Name = "product", IsRequired = false, EmitDefaultValue = false)]
        public string Product { get; set; }

        /// <summary>
        /// A brief description of the tool component.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public MultiformatMessageString ShortDescription { get; set; }

        /// <summary>
        /// A comprehensive description of the tool component.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public MultiformatMessageString FullDescription { get; set; }

        /// <summary>
        /// The name of the tool component along with its version and any other useful identifying information, such as its locale.
        /// </summary>
        [DataMember(Name = "fullName", IsRequired = false, EmitDefaultValue = false)]
        public string FullName { get; set; }

        /// <summary>
        /// The tool component version, in whatever format the component natively provides.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// The tool component version in the format specified by Semantic Versioning 2.0.
        /// </summary>
        [DataMember(Name = "semanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string SemanticVersion { get; set; }

        /// <summary>
        /// The binary version of the tool component's primary executable file expressed as four non-negative integers separated by a period (for operating systems that express file versions in this way).
        /// </summary>
        [DataMember(Name = "dottedQuadFileVersion", IsRequired = false, EmitDefaultValue = false)]
        public string DottedQuadFileVersion { get; set; }

        /// <summary>
        /// The absolute URI from which the tool component can be downloaded.
        /// </summary>
        [DataMember(Name = "downloadUri", IsRequired = false, EmitDefaultValue = false)]
        public Uri DownloadUri { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a resource identifier and each of whose values is a multiformatMessageString object, which holds message strings in plain text and (optionally) Markdown format. The strings can include placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "globalMessageStrings", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, MultiformatMessageString> GlobalMessageStrings { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to the notifications related to the configuration and runtime execution of the tool component.
        /// </summary>
        [DataMember(Name = "notificationDescriptors", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptor> NotificationDescriptors { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to the analysis performed by the tool component.
        /// </summary>
        [DataMember(Name = "ruleDescriptors", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptor> RuleDescriptors { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to the definitions of both standard and per tool taxonomies.
        /// </summary>
        [DataMember(Name = "taxa", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<ReportingDescriptor> Taxa { get; set; }

        /// <summary>
        /// The indices within the run artifacts array of the artifact objects associated with the tool component.
        /// </summary>
        [DataMember(Name = "artifactIndices", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<int> ArtifactIndices { get; set; }

        /// <summary>
        /// The language of the the localized strings defined in this component (expressed as an ISO 649 two-letter lowercase culture code) and region (expressed as an ISO 3166 two-letter uppercase subculture code associated with a country or region).
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        public string Language { get; set; }

        /// <summary>
        /// The kinds of data contained in this object.
        /// </summary>
        [DataMember(Name = "contents", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(FlagsEnumConverter))]
        public ToolComponentContents Contents { get; set; }

        /// <summary>
        /// true if this object contains a complete definition of the localizable and/or non-localizable data for this component.
        /// </summary>
        [DataMember(Name = "isComprehensive", IsRequired = false, EmitDefaultValue = false)]
        public bool IsComprehensive { get; set; }

        /// <summary>
        /// The semantic version of the localized strings defined in this component; used by components that define translations.
        /// </summary>
        [DataMember(Name = "localizedDataSemanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string LocalizedDataSemanticVersion { get; set; }

        /// <summary>
        /// The minimum value of localizedDataSemanticVersion required in translations consumed by this component; used by components that consume translations.
        /// </summary>
        [DataMember(Name = "minimumRequiredLocalizedDataSemanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public string MinimumRequiredLocalizedDataSemanticVersion { get; set; }

        /// <summary>
        /// The component for which the current component is a translation or a plugin.
        /// </summary>
        [DataMember(Name = "associatedComponent", IsRequired = false, EmitDefaultValue = false)]
        public ToolComponentReference AssociatedComponent { get; set; }

        /// <summary>
        /// Translation metadata, required for a translation, forbidden for other component types.
        /// </summary>
        [DataMember(Name = "translationMetadata", IsRequired = false, EmitDefaultValue = false)]
        public TranslationMetadata TranslationMetadata { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the tool component.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponent" /> class.
        /// </summary>
        public ToolComponent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponent" /> class from the supplied values.
        /// </summary>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
        /// </param>
        /// <param name="organization">
        /// An initialization value for the <see cref="P:Organization" /> property.
        /// </param>
        /// <param name="product">
        /// An initialization value for the <see cref="P:Product" /> property.
        /// </param>
        /// <param name="shortDescription">
        /// An initialization value for the <see cref="P:ShortDescription" /> property.
        /// </param>
        /// <param name="fullDescription">
        /// An initialization value for the <see cref="P:FullDescription" /> property.
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
        /// <param name="taxa">
        /// An initialization value for the <see cref="P:Taxa" /> property.
        /// </param>
        /// <param name="artifactIndices">
        /// An initialization value for the <see cref="P:ArtifactIndices" /> property.
        /// </param>
        /// <param name="language">
        /// An initialization value for the <see cref="P:Language" /> property.
        /// </param>
        /// <param name="contents">
        /// An initialization value for the <see cref="P:Contents" /> property.
        /// </param>
        /// <param name="isComprehensive">
        /// An initialization value for the <see cref="P:IsComprehensive" /> property.
        /// </param>
        /// <param name="localizedDataSemanticVersion">
        /// An initialization value for the <see cref="P:LocalizedDataSemanticVersion" /> property.
        /// </param>
        /// <param name="minimumRequiredLocalizedDataSemanticVersion">
        /// An initialization value for the <see cref="P:MinimumRequiredLocalizedDataSemanticVersion" /> property.
        /// </param>
        /// <param name="associatedComponent">
        /// An initialization value for the <see cref="P:AssociatedComponent" /> property.
        /// </param>
        /// <param name="translationMetadata">
        /// An initialization value for the <see cref="P:TranslationMetadata" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ToolComponent(string guid, string name, string organization, string product, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, string fullName, string version, string semanticVersion, string dottedQuadFileVersion, Uri downloadUri, IDictionary<string, MultiformatMessageString> globalMessageStrings, IEnumerable<ReportingDescriptor> notificationDescriptors, IEnumerable<ReportingDescriptor> ruleDescriptors, IEnumerable<ReportingDescriptor> taxa, IEnumerable<int> artifactIndices, string language, ToolComponentContents contents, bool isComprehensive, string localizedDataSemanticVersion, string minimumRequiredLocalizedDataSemanticVersion, ToolComponentReference associatedComponent, TranslationMetadata translationMetadata, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(guid, name, organization, product, shortDescription, fullDescription, fullName, version, semanticVersion, dottedQuadFileVersion, downloadUri, globalMessageStrings, notificationDescriptors, ruleDescriptors, taxa, artifactIndices, language, contents, isComprehensive, localizedDataSemanticVersion, minimumRequiredLocalizedDataSemanticVersion, associatedComponent, translationMetadata, properties);
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

            Init(other.Guid, other.Name, other.Organization, other.Product, other.ShortDescription, other.FullDescription, other.FullName, other.Version, other.SemanticVersion, other.DottedQuadFileVersion, other.DownloadUri, other.GlobalMessageStrings, other.NotificationDescriptors, other.RuleDescriptors, other.Taxa, other.ArtifactIndices, other.Language, other.Contents, other.IsComprehensive, other.LocalizedDataSemanticVersion, other.MinimumRequiredLocalizedDataSemanticVersion, other.AssociatedComponent, other.TranslationMetadata, other.Properties);
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

        private void Init(string guid, string name, string organization, string product, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, string fullName, string version, string semanticVersion, string dottedQuadFileVersion, Uri downloadUri, IDictionary<string, MultiformatMessageString> globalMessageStrings, IEnumerable<ReportingDescriptor> notificationDescriptors, IEnumerable<ReportingDescriptor> ruleDescriptors, IEnumerable<ReportingDescriptor> taxa, IEnumerable<int> artifactIndices, string language, ToolComponentContents contents, bool isComprehensive, string localizedDataSemanticVersion, string minimumRequiredLocalizedDataSemanticVersion, ToolComponentReference associatedComponent, TranslationMetadata translationMetadata, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Guid = guid;
            Name = name;
            Organization = organization;
            Product = product;
            if (shortDescription != null)
            {
                ShortDescription = new MultiformatMessageString(shortDescription);
            }

            if (fullDescription != null)
            {
                FullDescription = new MultiformatMessageString(fullDescription);
            }

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

            if (taxa != null)
            {
                var destination_2 = new List<ReportingDescriptor>();
                foreach (var value_3 in taxa)
                {
                    if (value_3 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new ReportingDescriptor(value_3));
                    }
                }

                Taxa = destination_2;
            }

            if (artifactIndices != null)
            {
                var destination_3 = new List<int>();
                foreach (var value_4 in artifactIndices)
                {
                    destination_3.Add(value_4);
                }

                ArtifactIndices = destination_3;
            }

            Language = language;
            Contents = contents;
            IsComprehensive = isComprehensive;
            LocalizedDataSemanticVersion = localizedDataSemanticVersion;
            MinimumRequiredLocalizedDataSemanticVersion = minimumRequiredLocalizedDataSemanticVersion;
            if (associatedComponent != null)
            {
                AssociatedComponent = new ToolComponentReference(associatedComponent);
            }

            if (translationMetadata != null)
            {
                TranslationMetadata = new TranslationMetadata(translationMetadata);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}