// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The location where an analysis tool produced a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.11.0.0")]
    public partial class Location : ISarifNode, IEquatable<Location>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Location;
            }
        }

        /// <summary>
        /// Identifies the file that the analysis tool was instructed to scan. This need not be the same as the file where the result actually occurred.
        /// </summary>
        [DataMember(Name = "analysisTarget", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocation AnalysisTarget { get; set; }

        /// <summary>
        /// Identifies the file where the analysis tool produced the result.
        /// </summary>
        [DataMember(Name = "resultFile", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocation ResultFile { get; set; }

        /// <summary>
        /// The logical location where the analysis tool produced the result.
        /// </summary>
        [DataMember(Name = "logicalLocation", IsRequired = false, EmitDefaultValue = false)]
        public IList<LogicalLocationComponent> LogicalLocation { get; set; }

        /// <summary>
        /// A string that summarizes the information in logicalLocation in a format consistent with the programming language in which the programmatic constructs expressed by logicalLocation were expressed.
        /// </summary>
        [DataMember(Name = "fullyQualifiedLogicalName", IsRequired = false, EmitDefaultValue = false)]
        public string FullyQualifiedLogicalName { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the location.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Location);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (AnalysisTarget != null)
                {
                    result = (result * 31) + AnalysisTarget.GetHashCode();
                }

                if (ResultFile != null)
                {
                    result = (result * 31) + ResultFile.GetHashCode();
                }

                if (LogicalLocation != null)
                {
                    foreach (var value_0 in LogicalLocation)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }

                if (FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + FullyQualifiedLogicalName.GetHashCode();
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_1 in Properties)
                    {
                        xor_0 ^= value_1.Key.GetHashCode();
                        if (value_1.Value != null)
                        {
                            xor_0 ^= value_1.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (Tags != null)
                {
                    foreach (var value_2 in Tags)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(Location other)
        {
            if (other == null)
            {
                return false;
            }

            if (!Object.Equals(AnalysisTarget, other.AnalysisTarget))
            {
                return false;
            }

            if (!Object.Equals(ResultFile, other.ResultFile))
            {
                return false;
            }

            if (!Object.ReferenceEquals(LogicalLocation, other.LogicalLocation))
            {
                if (LogicalLocation == null || other.LogicalLocation == null)
                {
                    return false;
                }

                if (LogicalLocation.Count != other.LogicalLocation.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < LogicalLocation.Count; ++index_0)
                {
                    if (!Object.Equals(LogicalLocation[index_0], other.LogicalLocation[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (FullyQualifiedLogicalName != other.FullyQualifiedLogicalName)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Properties, other.Properties))
            {
                if (Properties == null || other.Properties == null || Properties.Count != other.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in Properties)
                {
                    string value_1;
                    if (!other.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Tags, other.Tags))
            {
                if (Tags == null || other.Tags == null)
                {
                    return false;
                }

                if (Tags.Count != other.Tags.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < Tags.Count; ++index_1)
                {
                    if (Tags[index_1] != other.Tags[index_1])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class.
        /// </summary>
        public Location()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class from the supplied values.
        /// </summary>
        /// <param name="analysisTarget">
        /// An initialization value for the <see cref="P: AnalysisTarget" /> property.
        /// </param>
        /// <param name="resultFile">
        /// An initialization value for the <see cref="P: ResultFile" /> property.
        /// </param>
        /// <param name="logicalLocation">
        /// An initialization value for the <see cref="P: LogicalLocation" /> property.
        /// </param>
        /// <param name="fullyQualifiedLogicalName">
        /// An initialization value for the <see cref="P: FullyQualifiedLogicalName" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Location(PhysicalLocation analysisTarget, PhysicalLocation resultFile, IEnumerable<LogicalLocationComponent> logicalLocation, string fullyQualifiedLogicalName, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(analysisTarget, resultFile, logicalLocation, fullyQualifiedLogicalName, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Location(Location other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.AnalysisTarget, other.ResultFile, other.LogicalLocation, other.FullyQualifiedLogicalName, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Location DeepClone()
        {
            return (Location)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Location(this);
        }

        private void Init(PhysicalLocation analysisTarget, PhysicalLocation resultFile, IEnumerable<LogicalLocationComponent> logicalLocation, string fullyQualifiedLogicalName, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            if (analysisTarget != null)
            {
                AnalysisTarget = new PhysicalLocation(analysisTarget);
            }

            if (resultFile != null)
            {
                ResultFile = new PhysicalLocation(resultFile);
            }

            if (logicalLocation != null)
            {
                var destination_0 = new List<LogicalLocationComponent>();
                foreach (var value_0 in logicalLocation)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new LogicalLocationComponent(value_0));
                    }
                }

                LogicalLocation = destination_0;
            }

            FullyQualifiedLogicalName = fullyQualifiedLogicalName;
            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_1 = new List<string>();
                foreach (var value_1 in tags)
                {
                    destination_1.Add(value_1);
                }

                Tags = destination_1;
            }
        }
    }
}