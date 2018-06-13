// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A change to a single file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class FileChangeVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<FileChangeVersionOne> ValueComparer => FileChangeVersionOneEqualityComparer.Instance;

        public bool ValueEquals(FileChangeVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.FileChangeVersionOne;
            }
        }

        /// <summary>
        /// A string that represents the location of the file to change as a valid URI.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = true)]
        public Uri Uri { get; set; }

        /// <summary>
        /// A string that identifies the conceptual base for the 'uri' property (if it is relative), e.g.,'$(SolutionDir)' or '%SRCROOT%'.
        /// </summary>
        [DataMember(Name = "uriBaseId", IsRequired = false, EmitDefaultValue = false)]
        public string UriBaseId { get; set; }

        /// <summary>
        /// An array of replacement objects, each of which represents the replacement of a single range of bytes in a single file specified by 'uri'.
        /// </summary>
        [DataMember(Name = "replacements", IsRequired = true)]
        public IList<ReplacementVersionOne> Replacements { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChangeVersionOne" /> class.
        /// </summary>
        public FileChangeVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChangeVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="uriBaseId">
        /// An initialization value for the <see cref="P: UriBaseId" /> property.
        /// </param>
        /// <param name="replacements">
        /// An initialization value for the <see cref="P: Replacements" /> property.
        /// </param>
        public FileChangeVersionOne(Uri uri, string uriBaseId, IEnumerable<ReplacementVersionOne> replacements)
        {
            Init(uri, uriBaseId, replacements);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChangeVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FileChangeVersionOne(FileChangeVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Uri, other.UriBaseId, other.Replacements);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FileChangeVersionOne DeepClone()
        {
            return (FileChangeVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new FileChangeVersionOne(this);
        }

        private void Init(Uri uri, string uriBaseId, IEnumerable<ReplacementVersionOne> replacements)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            UriBaseId = uriBaseId;
            if (replacements != null)
            {
                var destination_0 = new List<ReplacementVersionOne>();
                foreach (var value_0 in replacements)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ReplacementVersionOne(value_0));
                    }
                }

                Replacements = destination_0;
            }
        }
    }
}