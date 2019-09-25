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
    /// Specifies the information necessary to retrieve a desired revision from a version control system.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class VersionControlDetails : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<VersionControlDetails> ValueComparer => VersionControlDetailsEqualityComparer.Instance;

        public bool ValueEquals(VersionControlDetails other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.VersionControlDetails;
            }
        }

        /// <summary>
        /// The absolute URI of the repository.
        /// </summary>
        [DataMember(Name = "repositoryUri", IsRequired = true)]
        [JsonConverter(typeof(UriConverter))]
        public virtual Uri RepositoryUri { get; set; }

        /// <summary>
        /// A string that uniquely and permanently identifies the revision within the repository.
        /// </summary>
        [DataMember(Name = "revisionId", IsRequired = false, EmitDefaultValue = false)]
        public virtual string RevisionId { get; set; }

        /// <summary>
        /// The name of a branch containing the revision.
        /// </summary>
        [DataMember(Name = "branch", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Branch { get; set; }

        /// <summary>
        /// A tag that has been applied to the revision.
        /// </summary>
        [DataMember(Name = "revisionTag", IsRequired = false, EmitDefaultValue = false)]
        public virtual string RevisionTag { get; set; }

        /// <summary>
        /// A Coordinated Universal Time (UTC) date and time that can be used to synchronize an enlistment to the state of the repository at that time.
        /// </summary>
        [DataMember(Name = "asOfTimeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public virtual DateTime AsOfTimeUtc { get; set; }

        /// <summary>
        /// The location in the local file system to which the root of the repository was mapped at the time of the analysis.
        /// </summary>
        [DataMember(Name = "mappedTo", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactLocation MappedTo { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the version control details.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionControlDetails" /> class.
        /// </summary>
        public VersionControlDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionControlDetails" /> class from the supplied values.
        /// </summary>
        /// <param name="repositoryUri">
        /// An initialization value for the <see cref="P:RepositoryUri" /> property.
        /// </param>
        /// <param name="revisionId">
        /// An initialization value for the <see cref="P:RevisionId" /> property.
        /// </param>
        /// <param name="branch">
        /// An initialization value for the <see cref="P:Branch" /> property.
        /// </param>
        /// <param name="revisionTag">
        /// An initialization value for the <see cref="P:RevisionTag" /> property.
        /// </param>
        /// <param name="asOfTimeUtc">
        /// An initialization value for the <see cref="P:AsOfTimeUtc" /> property.
        /// </param>
        /// <param name="mappedTo">
        /// An initialization value for the <see cref="P:MappedTo" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public VersionControlDetails(Uri repositoryUri, string revisionId, string branch, string revisionTag, DateTime asOfTimeUtc, ArtifactLocation mappedTo, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(repositoryUri, revisionId, branch, revisionTag, asOfTimeUtc, mappedTo, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionControlDetails" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public VersionControlDetails(VersionControlDetails other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.RepositoryUri, other.RevisionId, other.Branch, other.RevisionTag, other.AsOfTimeUtc, other.MappedTo, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual VersionControlDetails DeepClone()
        {
            return (VersionControlDetails)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new VersionControlDetails(this);
        }

        protected virtual void Init(Uri repositoryUri, string revisionId, string branch, string revisionTag, DateTime asOfTimeUtc, ArtifactLocation mappedTo, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (repositoryUri != null)
            {
                RepositoryUri = new Uri(repositoryUri.OriginalString, repositoryUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            RevisionId = revisionId;
            Branch = branch;
            RevisionTag = revisionTag;
            AsOfTimeUtc = asOfTimeUtc;
            if (mappedTo != null)
            {
                MappedTo = new ArtifactLocation(mappedTo);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}