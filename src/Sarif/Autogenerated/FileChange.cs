// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A change to a single file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class FileChange : ISarifNode
    {
        public static IEqualityComparer<FileChange> ValueComparer => FileChangeEqualityComparer.Instance;

        public bool ValueEquals(FileChange other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.FileChange;
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
        public IList<Replacement> Replacements { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChange" /> class.
        /// </summary>
        public FileChange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChange" /> class from the supplied values.
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
        public FileChange(Uri uri, string uriBaseId, IEnumerable<Replacement> replacements)
        {
            Init(uri, uriBaseId, replacements);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChange" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FileChange(FileChange other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Uri, other.UriBaseId, other.Replacements);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FileChange DeepClone()
        {
            return (FileChange)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new FileChange(this);
        }

        private void Init(Uri uri, string uriBaseId, IEnumerable<Replacement> replacements)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            UriBaseId = uriBaseId;
            if (replacements != null)
            {
                var destination_0 = new List<Replacement>();
                foreach (var value_0 in replacements)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Replacement(value_0));
                    }
                }

                Replacements = destination_0;
            }
        }
    }
}