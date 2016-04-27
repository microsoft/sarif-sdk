// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Static Analysis Results Format (SARIF) Version 1.0 JSON Schema (Draft 1.0.0-beta.4): a standard format for the output of static analysis and other tools.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.16.0.0")]
    public partial class SarifLog : ISarifNode, IEquatable<SarifLog>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.SarifLog;
            }
        }

        /// <summary>
        /// The SARIF format version of this log file.
        /// </summary>
        [DataMember(Name = "version", IsRequired = true)]
        public SarifVersion Version { get; set; }

        /// <summary>
        /// The set of runs contained in this log file.
        /// </summary>
        [DataMember(Name = "runs", IsRequired = true)]
        public IList<Run> Runs { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as SarifLog);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + Version.GetHashCode();
                if (Runs != null)
                {
                    foreach (var value_0 in Runs)
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

        public bool Equals(SarifLog other)
        {
            if (other == null)
            {
                return false;
            }

            if (Version != other.Version)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Runs, other.Runs))
            {
                if (Runs == null || other.Runs == null)
                {
                    return false;
                }

                if (Runs.Count != other.Runs.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Runs.Count; ++index_0)
                {
                    if (!Object.Equals(Runs[index_0], other.Runs[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLog" /> class.
        /// </summary>
        public SarifLog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLog" /> class from the supplied values.
        /// </summary>
        /// <param name="version">
        /// An initialization value for the <see cref="P: Version" /> property.
        /// </param>
        /// <param name="runs">
        /// An initialization value for the <see cref="P: Runs" /> property.
        /// </param>
        public SarifLog(SarifVersion version, IEnumerable<Run> runs)
        {
            Init(version, runs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifLog" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public SarifLog(SarifLog other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Version, other.Runs);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public SarifLog DeepClone()
        {
            return (SarifLog)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new SarifLog(this);
        }

        private void Init(SarifVersion version, IEnumerable<Run> runs)
        {
            Version = version;
            if (runs != null)
            {
                var destination_0 = new List<Run>();
                foreach (var value_0 in runs)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Run(value_0));
                    }
                }

                Runs = destination_0;
            }
        }
    }
}