// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A proposed fix for the problem represented by a result object. A fix specifies a set of file to modify. For each file, it specifies a set of bytes to remove, and provides a set of new bytes to replace them.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class Fix : ISarifNode
    {
        public static IEqualityComparer<Fix> ValueComparer => FixEqualityComparer.Instance;

        public bool ValueEquals(Fix other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Fix;
            }
        }

        /// <summary>
        /// A string that describes the proposed fix, enabling viewers to present a proposed change to an end user.
        /// </summary>
        [DataMember(Name = "description", IsRequired = true)]
        public string Description { get; set; }

        /// <summary>
        /// One or more file changes that comprise a fix for a result.
        /// </summary>
        [DataMember(Name = "fileChanges", IsRequired = true)]
        public IList<FileChange> FileChanges { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fix" /> class.
        /// </summary>
        public Fix()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fix" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P: Description" /> property.
        /// </param>
        /// <param name="fileChanges">
        /// An initialization value for the <see cref="P: FileChanges" /> property.
        /// </param>
        public Fix(string description, IEnumerable<FileChange> fileChanges)
        {
            Init(description, fileChanges);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fix" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Fix(Fix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.FileChanges);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Fix DeepClone()
        {
            return (Fix)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Fix(this);
        }

        private void Init(string description, IEnumerable<FileChange> fileChanges)
        {
            Description = description;
            if (fileChanges != null)
            {
                var destination_0 = new List<FileChange>();
                foreach (var value_0 in fileChanges)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new FileChange(value_0));
                    }
                }

                FileChanges = destination_0;
            }
        }
    }
}