// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A result produced by an analysis tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.15.0.0")]
    public partial class Result : ISarifNode, IEquatable<Result>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Result;
            }
        }

        /// <summary>
        /// A stable, opaque identifier for the rule that was evaluated to produce the result.
        /// </summary>
        [DataMember(Name = "ruleId", IsRequired = false, EmitDefaultValue = false)]
        public string RuleId { get; set; }

        /// <summary>
        /// The kind of observation this result represents. If this property is not present, its implied value is 'warning'.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public ResultKind Kind { get; set; }

        /// <summary>
        /// A string that describes the result.
        /// </summary>
        [DataMember(Name = "fullMessage", IsRequired = false, EmitDefaultValue = false)]
        public string FullMessage { get; set; }

        /// <summary>
        /// A string that describes the result, displayed when visible space is limited to a single line of text.
        /// </summary>
        [DataMember(Name = "shortMessage", IsRequired = false, EmitDefaultValue = false)]
        public string ShortMessage { get; set; }

        /// <summary>
        /// A 'formattedMessage' object that can be used to construct a formatted message that describes the result. If the 'formattedMessage' property is present on a result, the 'fullMessage' property shall not be present. If the 'fullMessage' property is present on an result, the 'formattedMessage' property shall not be present
        /// </summary>
        [DataMember(Name = "formattedMessage", IsRequired = false, EmitDefaultValue = false)]
        public FormattedMessage FormattedMessage { get; set; }

        /// <summary>
        /// One or more locations where the result occurred. Specify only one location unless the problem indicated by the result can only be corrected by making a change at every specified location.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = false, EmitDefaultValue = false)]
        public ISet<Location> Locations { get; set; }

        /// <summary>
        /// A string that contributes to the unique identity of the result.
        /// </summary>
        [DataMember(Name = "toolFingerprint", IsRequired = false, EmitDefaultValue = false)]
        public string ToolFingerprint { get; set; }

        /// <summary>
        /// An array of 'stack' objects relevant to the result.
        /// </summary>
        [DataMember(Name = "stacks", IsRequired = false, EmitDefaultValue = false)]
        public ISet<Stack> Stacks { get; set; }

        /// <summary>
        /// An array of arrays of 'annotatedCodeLocation` objects, each inner array of which comprises a code flow (a possible execution path through the code).
        /// </summary>
        [DataMember(Name = "codeFlows", IsRequired = false, EmitDefaultValue = false)]
        public IList<IList<AnnotatedCodeLocation>> CodeFlows { get; set; }

        /// <summary>
        /// A grouped set of locations and messages, if available, that represent code areas that are related to this result.
        /// </summary>
        [DataMember(Name = "relatedLocations", IsRequired = false, EmitDefaultValue = false)]
        public ISet<AnnotatedCodeLocation> RelatedLocations { get; set; }

        /// <summary>
        /// A flag indicating whether or not this result was suppressed in source code.
        /// </summary>
        [DataMember(Name = "isSuppressedInSource", IsRequired = false, EmitDefaultValue = false)]
        public bool? IsSuppressedInSource { get; set; }

        /// <summary>
        /// An array of 'fix' objects, each of which represents a proposed fix to the problem indicated by the result.
        /// </summary>
        [DataMember(Name = "fixes", IsRequired = false, EmitDefaultValue = false)]
        public ISet<Fix> Fixes { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the result.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the result.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public ISet<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Result);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (RuleId != null)
                {
                    result = (result * 31) + RuleId.GetHashCode();
                }

                result = (result * 31) + Kind.GetHashCode();
                if (FullMessage != null)
                {
                    result = (result * 31) + FullMessage.GetHashCode();
                }

                if (ShortMessage != null)
                {
                    result = (result * 31) + ShortMessage.GetHashCode();
                }

                if (FormattedMessage != null)
                {
                    result = (result * 31) + FormattedMessage.GetHashCode();
                }

                if (Locations != null)
                {
                    foreach (var value_0 in Locations)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }

                if (ToolFingerprint != null)
                {
                    result = (result * 31) + ToolFingerprint.GetHashCode();
                }

                if (Stacks != null)
                {
                    foreach (var value_1 in Stacks)
                    {
                        result = result * 31;
                        if (value_1 != null)
                        {
                            result = (result * 31) + value_1.GetHashCode();
                        }
                    }
                }

                if (CodeFlows != null)
                {
                    foreach (var value_2 in CodeFlows)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            foreach (var value_3 in value_2)
                            {
                                result = result * 31;
                                if (value_3 != null)
                                {
                                    result = (result * 31) + value_3.GetHashCode();
                                }
                            }
                        }
                    }
                }

                if (RelatedLocations != null)
                {
                    foreach (var value_4 in RelatedLocations)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
                }

                result = (result * 31) + IsSuppressedInSource.GetHashCode();
                if (Fixes != null)
                {
                    foreach (var value_5 in Fixes)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.GetHashCode();
                        }
                    }
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in Properties)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (Tags != null)
                {
                    foreach (var value_7 in Tags)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(Result other)
        {
            if (other == null)
            {
                return false;
            }

            if (RuleId != other.RuleId)
            {
                return false;
            }

            if (Kind != other.Kind)
            {
                return false;
            }

            if (FullMessage != other.FullMessage)
            {
                return false;
            }

            if (ShortMessage != other.ShortMessage)
            {
                return false;
            }

            if (!Object.Equals(FormattedMessage, other.FormattedMessage))
            {
                return false;
            }

            if (!Object.ReferenceEquals(Locations, other.Locations))
            {
                if (Locations == null || other.Locations == null)
                {
                    return false;
                }

                if (!Locations.SetEquals(other.Locations))
                {
                    return false;
                }
            }

            if (ToolFingerprint != other.ToolFingerprint)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Stacks, other.Stacks))
            {
                if (Stacks == null || other.Stacks == null)
                {
                    return false;
                }

                if (!Stacks.SetEquals(other.Stacks))
                {
                    return false;
                }
            }

            if (!Object.ReferenceEquals(CodeFlows, other.CodeFlows))
            {
                if (CodeFlows == null || other.CodeFlows == null)
                {
                    return false;
                }

                if (CodeFlows.Count != other.CodeFlows.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < CodeFlows.Count; ++index_0)
                {
                    if (!Object.ReferenceEquals(CodeFlows[index_0], other.CodeFlows[index_0]))
                    {
                        if (CodeFlows[index_0] == null || other.CodeFlows[index_0] == null)
                        {
                            return false;
                        }

                        if (CodeFlows[index_0].Count != other.CodeFlows[index_0].Count)
                        {
                            return false;
                        }

                        for (int index_1 = 0; index_1 < CodeFlows[index_0].Count; ++index_1)
                        {
                            if (!Object.Equals(CodeFlows[index_0][index_1], other.CodeFlows[index_0][index_1]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (!Object.ReferenceEquals(RelatedLocations, other.RelatedLocations))
            {
                if (RelatedLocations == null || other.RelatedLocations == null)
                {
                    return false;
                }

                if (!RelatedLocations.SetEquals(other.RelatedLocations))
                {
                    return false;
                }
            }

            if (IsSuppressedInSource != other.IsSuppressedInSource)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Fixes, other.Fixes))
            {
                if (Fixes == null || other.Fixes == null)
                {
                    return false;
                }

                if (!Fixes.SetEquals(other.Fixes))
                {
                    return false;
                }
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

                if (!Tags.SetEquals(other.Tags))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result" /> class.
        /// </summary>
        public Result()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result" /> class from the supplied values.
        /// </summary>
        /// <param name="ruleId">
        /// An initialization value for the <see cref="P: RuleId" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P: Kind" /> property.
        /// </param>
        /// <param name="fullMessage">
        /// An initialization value for the <see cref="P: FullMessage" /> property.
        /// </param>
        /// <param name="shortMessage">
        /// An initialization value for the <see cref="P: ShortMessage" /> property.
        /// </param>
        /// <param name="formattedMessage">
        /// An initialization value for the <see cref="P: FormattedMessage" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P: Locations" /> property.
        /// </param>
        /// <param name="toolFingerprint">
        /// An initialization value for the <see cref="P: ToolFingerprint" /> property.
        /// </param>
        /// <param name="stacks">
        /// An initialization value for the <see cref="P: Stacks" /> property.
        /// </param>
        /// <param name="codeFlows">
        /// An initialization value for the <see cref="P: CodeFlows" /> property.
        /// </param>
        /// <param name="relatedLocations">
        /// An initialization value for the <see cref="P: RelatedLocations" /> property.
        /// </param>
        /// <param name="isSuppressedInSource">
        /// An initialization value for the <see cref="P: IsSuppressedInSource" /> property.
        /// </param>
        /// <param name="fixes">
        /// An initialization value for the <see cref="P: Fixes" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Result(string ruleId, ResultKind kind, string fullMessage, string shortMessage, FormattedMessage formattedMessage, ISet<Location> locations, string toolFingerprint, ISet<Stack> stacks, IEnumerable<IEnumerable<AnnotatedCodeLocation>> codeFlows, ISet<AnnotatedCodeLocation> relatedLocations, bool? isSuppressedInSource, ISet<Fix> fixes, IDictionary<string, string> properties, ISet<string> tags)
        {
            Init(ruleId, kind, fullMessage, shortMessage, formattedMessage, locations, toolFingerprint, stacks, codeFlows, relatedLocations, isSuppressedInSource, fixes, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Result(Result other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.RuleId, other.Kind, other.FullMessage, other.ShortMessage, other.FormattedMessage, other.Locations, other.ToolFingerprint, other.Stacks, other.CodeFlows, other.RelatedLocations, other.IsSuppressedInSource, other.Fixes, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Result DeepClone()
        {
            return (Result)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Result(this);
        }

        private void Init(string ruleId, ResultKind kind, string fullMessage, string shortMessage, FormattedMessage formattedMessage, ISet<Location> locations, string toolFingerprint, ISet<Stack> stacks, IEnumerable<IEnumerable<AnnotatedCodeLocation>> codeFlows, ISet<AnnotatedCodeLocation> relatedLocations, bool? isSuppressedInSource, ISet<Fix> fixes, IDictionary<string, string> properties, ISet<string> tags)
        {
            RuleId = ruleId;
            Kind = kind;
            FullMessage = fullMessage;
            ShortMessage = shortMessage;
            if (formattedMessage != null)
            {
                FormattedMessage = new FormattedMessage(formattedMessage);
            }

            if (locations != null)
            {
                var destination_0 = new HashSet<Location>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Location(value_0));
                    }
                }

                Locations = destination_0;
            }

            ToolFingerprint = toolFingerprint;
            if (stacks != null)
            {
                var destination_1 = new HashSet<Stack>();
                foreach (var value_1 in stacks)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Stack(value_1));
                    }
                }

                Stacks = destination_1;
            }

            if (codeFlows != null)
            {
                var destination_2 = new List<IList<AnnotatedCodeLocation>>();
                foreach (var value_2 in codeFlows)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        var destination_3 = new List<AnnotatedCodeLocation>();
                        foreach (var value_3 in value_2)
                        {
                            if (value_3 == null)
                            {
                                destination_3.Add(null);
                            }
                            else
                            {
                                destination_3.Add(new AnnotatedCodeLocation(value_3));
                            }
                        }

                        destination_2.Add(destination_3);
                    }
                }

                CodeFlows = destination_2;
            }

            if (relatedLocations != null)
            {
                var destination_4 = new HashSet<AnnotatedCodeLocation>();
                foreach (var value_4 in relatedLocations)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new AnnotatedCodeLocation(value_4));
                    }
                }

                RelatedLocations = destination_4;
            }

            IsSuppressedInSource = isSuppressedInSource;
            if (fixes != null)
            {
                var destination_5 = new HashSet<Fix>();
                foreach (var value_5 in fixes)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new Fix(value_5));
                    }
                }

                Fixes = destination_5;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_6 = new HashSet<string>();
                foreach (var value_6 in tags)
                {
                    destination_6.Add(value_6);
                }

                Tags = destination_6;
            }
        }
    }
}