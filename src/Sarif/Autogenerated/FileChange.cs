// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.11.0.0")]
    public partial class FileChange : ISarifNode, IEquatable<FileChange>
    {
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
        /// An array of replacement objects, each of which represents the replacement of a single range of bytes in a single file specified by 'uri'.
        /// </summary>
        [DataMember(Name = "replacements", IsRequired = true)]
        public IList<Replacement> Replacements { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as FileChange);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Uri != null)
                {
                    result = (result * 31) + Uri.GetHashCode();
                }

                if (Replacements != null)
                {
                    foreach (var value_0 in Replacements)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(FileChange other)
        {
            if (other == null)
            {
                return false;
            }

            if (Uri != other.Uri)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Replacements, other.Replacements))
            {
                if (Replacements == null || other.Replacements == null)
                {
                    return false;
                }

                if (Replacements.Count != other.Replacements.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Replacements.Count; ++index_0)
                {
                    if (!Object.Equals(Replacements[index_0], other.Replacements[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

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
        /// <param name="replacements">
        /// An initialization value for the <see cref="P: Replacements" /> property.
        /// </param>
        public FileChange(Uri uri, IEnumerable<Replacement> replacements)
        {
            Init(uri, replacements);
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

            Init(other.Uri, other.Replacements);
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

        private void Init(Uri uri, IEnumerable<Replacement> replacements)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

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