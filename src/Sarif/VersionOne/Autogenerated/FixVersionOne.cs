// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A proposed fix for the problem represented by a result object. A fix specifies a set of file to modify. For each file, it specifies a set of bytes to remove, and provides a set of new bytes to replace them.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class FixVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<FixVersionOne> ValueComparer => FixVersionOneEqualityComparer.Instance;

        public bool ValueEquals(FixVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.FixVersionOne;
            }
        }

        /// <summary>
        /// A string that describes the proposed fix, enabling viewers to present a proposed change to an end user.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// One or more file changes that comprise a fix for a result.
        /// </summary>
        [DataMember(Name = "fileChanges", IsRequired = true)]
        public IList<FileChangeVersionOne> FileChanges { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixVersionOne" /> class.
        /// </summary>
        public FixVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P: Description" /> property.
        /// </param>
        /// <param name="fileChanges">
        /// An initialization value for the <see cref="P: FileChanges" /> property.
        /// </param>
        public FixVersionOne(string description, IEnumerable<FileChangeVersionOne> fileChanges)
        {
            Init(description, fileChanges);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FixVersionOne(FixVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.FileChanges);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FixVersionOne DeepClone()
        {
            return (FixVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new FixVersionOne(this);
        }

        private void Init(string description, IEnumerable<FileChangeVersionOne> fileChanges)
        {
            Description = description;
            if (fileChanges != null)
            {
                var destination_0 = new List<FileChangeVersionOne>();
                foreach (var value_0 in fileChanges)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new FileChangeVersionOne(value_0));
                    }
                }

                FileChanges = destination_0;
            }
        }
    }
}