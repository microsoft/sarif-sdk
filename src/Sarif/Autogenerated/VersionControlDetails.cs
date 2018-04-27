// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class VersionControlDetails : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<VersionControlDetails> ValueComparer => VersionControlDetailsEqualityComparer.Instance;

        public bool ValueEquals(VersionControlDetails other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.VersionControlDetails;
            }
        }

        /// <summary>
        /// The absolute URI of the repository.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = true)]
        public Uri Uri { get; set; }

        /// <summary>
        /// A string that uniquely and permanently identifies the revision within the repository.
        /// </summary>
        [DataMember(Name = "revisionId", IsRequired = false, EmitDefaultValue = false)]
        public string RevisionId { get; set; }

        /// <summary>
        /// The name of a branch containing the revision.
        /// </summary>
        [DataMember(Name = "branch", IsRequired = false, EmitDefaultValue = false)]
        public string Branch { get; set; }

        /// <summary>
        /// A tag that has been applied to the revision.
        /// </summary>
        [DataMember(Name = "tag", IsRequired = false, EmitDefaultValue = false)]
        public string Tag { get; set; }

        /// <summary>
        /// The date and time at which the revision was created.
        /// </summary>
        [DataMember(Name = "timestamp", IsRequired = false, EmitDefaultValue = false)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the revision.
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
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="revisionId">
        /// An initialization value for the <see cref="P: RevisionId" /> property.
        /// </param>
        /// <param name="branch">
        /// An initialization value for the <see cref="P: Branch" /> property.
        /// </param>
        /// <param name="tag">
        /// An initialization value for the <see cref="P: Tag" /> property.
        /// </param>
        /// <param name="timestamp">
        /// An initialization value for the <see cref="P: Timestamp" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public VersionControlDetails(Uri uri, string revisionId, string branch, string tag, DateTime timestamp, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(uri, revisionId, branch, tag, timestamp, properties);
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

            Init(other.Uri, other.RevisionId, other.Branch, other.Tag, other.Timestamp, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public VersionControlDetails DeepClone()
        {
            return (VersionControlDetails)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new VersionControlDetails(this);
        }

        private void Init(Uri uri, string revisionId, string branch, string tag, DateTime timestamp, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            RevisionId = revisionId;
            Branch = branch;
            Tag = tag;
            Timestamp = timestamp;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}