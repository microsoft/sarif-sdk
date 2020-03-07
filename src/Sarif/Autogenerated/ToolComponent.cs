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
    /// A component, such as a plug-in or the driver, of the analysis tool that was run.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ToolComponent : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ToolComponent> ValueComparer => ToolComponentEqualityComparer.Instance;

        public bool ValueEquals(ToolComponent other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
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
        public virtual string Guid { get; set; }

        /// <summary>
        /// The name of the tool component.
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public virtual string Name { get; set; }

        /// <summary>
        /// The organization or company that produced the tool component.
        /// </summary>
        [DataMember(Name = "organization", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Organization { get; set; }

        /// <summary>
        /// A product suite to which the tool component belongs.
        /// </summary>
        [DataMember(Name = "product", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Product { get; set; }

        /// <summary>
        /// A localizable string containing the name of the suite of products to which the tool component belongs.
        /// </summary>
        [DataMember(Name = "productSuite", IsRequired = false, EmitDefaultValue = false)]
        public virtual string ProductSuite { get; set; }

        /// <summary>
        /// A brief description of the tool component.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public virtual MultiformatMessageString ShortDescription { get; set; }

        /// <summary>
        /// A comprehensive description of the tool component.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public virtual MultiformatMessageString FullDescription { get; set; }

        /// <summary>
        /// The name of the tool component along with its version and any other useful identifying information, such as its locale.
        /// </summary>
        [DataMember(Name = "fullName", IsRequired = false, EmitDefaultValue = false)]
        public virtual string FullName { get; set; }

        /// <summary>
        /// The tool component version, in whatever format the component natively provides.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Version { get; set; }

        /// <summary>
        /// The tool component version in the format specified by Semantic Versioning 2.0.
        /// </summary>
        [DataMember(Name = "semanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public virtual string SemanticVersion { get; set; }

        /// <summary>
        /// The binary version of the tool component's primary executable file expressed as four non-negative integers separated by a period (for operating systems that express file versions in this way).
        /// </summary>
        [DataMember(Name = "dottedQuadFileVersion", IsRequired = false, EmitDefaultValue = false)]
        public virtual string DottedQuadFileVersion { get; set; }

        /// <summary>
        /// A string specifying the UTC date (and optionally, the time) of the component's release.
        /// </summary>
        [DataMember(Name = "releaseDateUtc", IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReleaseDateUtc { get; set; }

        /// <summary>
        /// The absolute URI from which the tool component can be downloaded.
        /// </summary>
        [DataMember(Name = "downloadUri", IsRequired = false, EmitDefaultValue = false)]
        public virtual Uri DownloadUri { get; set; }

        /// <summary>
        /// The absolute URI at which information about this version of the tool component can be found.
        /// </summary>
        [DataMember(Name = "informationUri", IsRequired = false, EmitDefaultValue = false)]
        public virtual Uri InformationUri { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is a resource identifier and each of whose values is a multiformatMessageString object, which holds message strings in plain text and (optionally) Markdown format. The strings can include placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        [DataMember(Name = "globalMessageStrings", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, MultiformatMessageString> GlobalMessageStrings { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to the notifications related to the configuration and runtime execution of the tool component.
        /// </summary>
        [DataMember(Name = "notifications", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ReportingDescriptor> Notifications { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to the analysis performed by the tool component.
        /// </summary>
        [DataMember(Name = "rules", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ReportingDescriptor> Rules { get; set; }

        /// <summary>
        /// An array of reportingDescriptor objects relevant to the definitions of both standalone and tool-defined taxonomies.
        /// </summary>
        [DataMember(Name = "taxa", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ReportingDescriptor> Taxa { get; set; }

        /// <summary>
        /// An array of the artifactLocation objects associated with the tool component.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ArtifactLocation> Locations { get; set; }

        /// <summary>
        /// The language of the messages emitted into the log file during this run (expressed as an ISO 639-1 two-letter lowercase language code) and an optional region (expressed as an ISO 3166-1 two-letter uppercase subculture code associated with a country or region). The casing is recommended but not required (in order for this data to conform to RFC5646).
        /// </summary>
        [DataMember(Name = "language", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue("en-US")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual string Language { get; set; }

        /// <summary>
        /// The kinds of data contained in this object.
        /// </summary>
        [DataMember(Name = "contents", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(FlagsEnumConverter))]
        public virtual ToolComponentContents Contents { get; set; }

        /// <summary>
        /// Specifies whether this object contains a complete definition of the localizable and/or non-localizable data for this component, as opposed to including only data that is relevant to the results persisted to this log file.
        /// </summary>
        [DataMember(Name = "isComprehensive", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual bool IsComprehensive { get; set; }

        /// <summary>
        /// The semantic version of the localized strings defined in this component; maintained by components that provide translations.
        /// </summary>
        [DataMember(Name = "localizedDataSemanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public virtual string LocalizedDataSemanticVersion { get; set; }

        /// <summary>
        /// The minimum value of localizedDataSemanticVersion required in translations consumed by this component; used by components that consume translations.
        /// </summary>
        [DataMember(Name = "minimumRequiredLocalizedDataSemanticVersion", IsRequired = false, EmitDefaultValue = false)]
        public virtual string MinimumRequiredLocalizedDataSemanticVersion { get; set; }

        /// <summary>
        /// The component which is strongly associated with this component. For a translation, this refers to the component which has been translated. For an extension, this is the driver that provides the extension's plugin model.
        /// </summary>
        [DataMember(Name = "associatedComponent", IsRequired = false, EmitDefaultValue = false)]
        public virtual ToolComponentReference AssociatedComponent { get; set; }

        /// <summary>
        /// Translation metadata, required for a translation, not populated by other component types.
        /// </summary>
        [DataMember(Name = "translationMetadata", IsRequired = false, EmitDefaultValue = false)]
        public virtual TranslationMetadata TranslationMetadata { get; set; }

        /// <summary>
        /// An array of toolComponentReference objects to declare the taxonomies supported by the tool component.
        /// </summary>
        [DataMember(Name = "supportedTaxonomies", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ToolComponentReference> SupportedTaxonomies { get; set; }

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
            Language = "en-US";
            IsComprehensive = false;
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
        /// <param name="productSuite">
        /// An initialization value for the <see cref="P:ProductSuite" /> property.
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
        /// <param name="releaseDateUtc">
        /// An initialization value for the <see cref="P:ReleaseDateUtc" /> property.
        /// </param>
        /// <param name="downloadUri">
        /// An initialization value for the <see cref="P:DownloadUri" /> property.
        /// </param>
        /// <param name="informationUri">
        /// An initialization value for the <see cref="P:InformationUri" /> property.
        /// </param>
        /// <param name="globalMessageStrings">
        /// An initialization value for the <see cref="P:GlobalMessageStrings" /> property.
        /// </param>
        /// <param name="notifications">
        /// An initialization value for the <see cref="P:Notifications" /> property.
        /// </param>
        /// <param name="rules">
        /// An initialization value for the <see cref="P:Rules" /> property.
        /// </param>
        /// <param name="taxa">
        /// An initialization value for the <see cref="P:Taxa" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P:Locations" /> property.
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
        /// <param name="supportedTaxonomies">
        /// An initialization value for the <see cref="P:SupportedTaxonomies" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ToolComponent(string guid, string name, string organization, string product, string productSuite, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, string fullName, string version, string semanticVersion, string dottedQuadFileVersion, string releaseDateUtc, Uri downloadUri, Uri informationUri, IDictionary<string, MultiformatMessageString> globalMessageStrings, IEnumerable<ReportingDescriptor> notifications, IEnumerable<ReportingDescriptor> rules, IEnumerable<ReportingDescriptor> taxa, IEnumerable<ArtifactLocation> locations, string language, ToolComponentContents contents, bool isComprehensive, string localizedDataSemanticVersion, string minimumRequiredLocalizedDataSemanticVersion, ToolComponentReference associatedComponent, TranslationMetadata translationMetadata, IEnumerable<ToolComponentReference> supportedTaxonomies, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(guid, name, organization, product, productSuite, shortDescription, fullDescription, fullName, version, semanticVersion, dottedQuadFileVersion, releaseDateUtc, downloadUri, informationUri, globalMessageStrings, notifications, rules, taxa, locations, language, contents, isComprehensive, localizedDataSemanticVersion, minimumRequiredLocalizedDataSemanticVersion, associatedComponent, translationMetadata, supportedTaxonomies, properties);
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

            Init(other.Guid, other.Name, other.Organization, other.Product, other.ProductSuite, other.ShortDescription, other.FullDescription, other.FullName, other.Version, other.SemanticVersion, other.DottedQuadFileVersion, other.ReleaseDateUtc, other.DownloadUri, other.InformationUri, other.GlobalMessageStrings, other.Notifications, other.Rules, other.Taxa, other.Locations, other.Language, other.Contents, other.IsComprehensive, other.LocalizedDataSemanticVersion, other.MinimumRequiredLocalizedDataSemanticVersion, other.AssociatedComponent, other.TranslationMetadata, other.SupportedTaxonomies, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ToolComponent DeepClone()
        {
            return (ToolComponent)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ToolComponent(this);
        }

        protected virtual void Init(string guid, string name, string organization, string product, string productSuite, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, string fullName, string version, string semanticVersion, string dottedQuadFileVersion, string releaseDateUtc, Uri downloadUri, Uri informationUri, IDictionary<string, MultiformatMessageString> globalMessageStrings, IEnumerable<ReportingDescriptor> notifications, IEnumerable<ReportingDescriptor> rules, IEnumerable<ReportingDescriptor> taxa, IEnumerable<ArtifactLocation> locations, string language, ToolComponentContents contents, bool isComprehensive, string localizedDataSemanticVersion, string minimumRequiredLocalizedDataSemanticVersion, ToolComponentReference associatedComponent, TranslationMetadata translationMetadata, IEnumerable<ToolComponentReference> supportedTaxonomies, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Guid = guid;
            Name = name;
            Organization = organization;
            Product = product;
            ProductSuite = productSuite;
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
            ReleaseDateUtc = releaseDateUtc;
            if (downloadUri != null)
            {
                DownloadUri = new Uri(downloadUri.OriginalString, downloadUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (informationUri != null)
            {
                InformationUri = new Uri(informationUri.OriginalString, informationUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (globalMessageStrings != null)
            {
                GlobalMessageStrings = new Dictionary<string, MultiformatMessageString>();
                foreach (var value_0 in globalMessageStrings)
                {
                    GlobalMessageStrings.Add(value_0.Key, new MultiformatMessageString(value_0.Value));
                }
            }

            if (notifications != null)
            {
                var destination_0 = new List<ReportingDescriptor>();
                foreach (var value_1 in notifications)
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

                Notifications = destination_0;
            }

            if (rules != null)
            {
                var destination_1 = new List<ReportingDescriptor>();
                foreach (var value_2 in rules)
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

                Rules = destination_1;
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

            if (locations != null)
            {
                var destination_3 = new List<ArtifactLocation>();
                foreach (var value_4 in locations)
                {
                    if (value_4 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new ArtifactLocation(value_4));
                    }
                }

                Locations = destination_3;
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

            if (supportedTaxonomies != null)
            {
                var destination_4 = new List<ToolComponentReference>();
                foreach (var value_5 in supportedTaxonomies)
                {
                    if (value_5 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new ToolComponentReference(value_5));
                    }
                }

                SupportedTaxonomies = destination_4;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}