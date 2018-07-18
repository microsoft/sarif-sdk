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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
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
        /// The location of the file to change.
        /// </summary>
        [DataMember(Name = "fileLocation", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation FileLocation { get; set; }

        /// <summary>
        /// An array of replacement objects, each of which represents the replacement of a single range of bytes in a single file specified by 'fileLocation'.
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
        /// <param name="fileLocation">
        /// An initialization value for the <see cref="P: FileLocation" /> property.
        /// </param>
        /// <param name="replacements">
        /// An initialization value for the <see cref="P: Replacements" /> property.
        /// </param>
        public FileChange(FileLocation fileLocation, IEnumerable<Replacement> replacements)
        {
            Init(fileLocation, replacements);
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

            Init(other.FileLocation, other.Replacements);
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

        private void Init(FileLocation fileLocation, IEnumerable<Replacement> replacements)
        {
            if (fileLocation != null)
            {
                FileLocation = new FileLocation(fileLocation);
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