// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Static Analysis Results Format (SARIF) Version 1.0.0 JSON Schema: a standard format for the output of static analysis and other tools.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class SarifLogVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<SarifLogVersionOne> ValueComparer => SarifLogVersionOneEqualityComparer.Instance;

        public bool ValueEquals(SarifLogVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.SarifLogVersionOne;
            }
        }

        /// <summary>
        /// The URI of the JSON schema corresponding to the version.
        /// </summary>
        [DataMember(Name = "$schema", IsRequired = false, EmitDefaultValue = false)]
        public Uri SchemaUri { get; set; }

        /// <summary>
        /// The SARIF format version of this log file.
        /// </summary>
        [DataMember(Name = "version", IsRequired = true)]
        public SarifVersionVersionOne Version { get; set; }

        /// <summary>
        /// The set of runs contained in this log file.
        /// </summary>
        [DataMember(Name = "runs", IsRequired = true)]
        public IList<RunVersionOne> Runs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLogVersionOne" /> class.
        /// </summary>
        public SarifLogVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLogVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="schemaUri">
        /// An initialization value for the <see cref="P: SchemaUri" /> property.
        /// </param>
        /// <param name="version">
        /// An initialization value for the <see cref="P: Version" /> property.
        /// </param>
        /// <param name="runs">
        /// An initialization value for the <see cref="P: Runs" /> property.
        /// </param>
        public SarifLogVersionOne(Uri schemaUri, SarifVersionVersionOne version, IEnumerable<RunVersionOne> runs)
        {
            Init(schemaUri, version, runs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLogVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public SarifLogVersionOne(SarifLogVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.SchemaUri, other.Version, other.Runs);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public SarifLogVersionOne DeepClone()
        {
            return (SarifLogVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new SarifLogVersionOne(this);
        }

        private void Init(Uri schemaUri, SarifVersionVersionOne version, IEnumerable<RunVersionOne> runs)
        {
            if (schemaUri != null)
            {
                SchemaUri = new Uri(schemaUri.OriginalString, schemaUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            Version = version;
            if (runs != null)
            {
                var destination_0 = new List<RunVersionOne>();
                foreach (var value_0 in runs)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new RunVersionOne(value_0));
                    }
                }

                Runs = destination_0;
            }
        }
    }
}