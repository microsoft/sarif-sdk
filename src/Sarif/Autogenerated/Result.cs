// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A result produced by an analysis tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.42.0.0")]
    public partial class Result : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Result> ValueComparer => ResultEqualityComparer.Instance;

        public bool ValueEquals(Result other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

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
        /// The stable, unique identifier of the rule (if any) to which this notification is relevant. If 'ruleKey' is not specified, this member can be used to retrieve rule metadata from the rules dictionary, if it exists.
        /// </summary>
        [DataMember(Name = "ruleId", IsRequired = false, EmitDefaultValue = false)]
        public string RuleId { get; set; }

        /// <summary>
        /// A key used to retrieve the rule metadata from the rules dictionary that is relevant to the notificationn.
        /// </summary>
        [DataMember(Name = "ruleKey", IsRequired = false, EmitDefaultValue = false)]
        public string RuleKey { get; set; }

        /// <summary>
        /// A value specifying the severity level of the result. If this property is not present, its implied value is 'warning'.
        /// </summary>
        [DataMember(Name = "level", IsRequired = false, EmitDefaultValue = false)]
        public ResultLevel Level { get; set; }

        /// <summary>
        /// A string that describes the result. The first sentence of the message only will be displayed when visible space is limited.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// A 'formattedRuleMessage' object that can be used to construct a formatted message that describes the result. If the 'formattedMessage' property is present on a result, the 'fullMessage' property shall not be present. If the 'fullMessage' property is present on an result, the 'formattedMessage' property shall not be present
        /// </summary>
        [DataMember(Name = "formattedRuleMessage", IsRequired = false, EmitDefaultValue = false)]
        public FormattedRuleMessage FormattedRuleMessage { get; set; }

        /// <summary>
        /// One or more locations where the result occurred. Specify only one location unless the problem indicated by the result can only be corrected by making a change at every specified location.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = false, EmitDefaultValue = false)]
        public IList<Location> Locations { get; set; }

        /// <summary>
        /// A source code or other file fragment that illustrates the result.
        /// </summary>
        [DataMember(Name = "snippet", IsRequired = false, EmitDefaultValue = false)]
        public string Snippet { get; set; }

        /// <summary>
        /// A unique identifer for the result.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// A string that contributes to the unique identity of the result.
        /// </summary>
        [DataMember(Name = "toolFingerprintContribution", IsRequired = false, EmitDefaultValue = false)]
        public string ToolFingerprintContribution { get; set; }

        /// <summary>
        /// An array of 'stack' objects relevant to the result.
        /// </summary>
        [DataMember(Name = "stacks", IsRequired = false, EmitDefaultValue = false)]
        public IList<Stack> Stacks { get; set; }

        /// <summary>
        /// An array of 'codeFlow' objects relevant to the result.
        /// </summary>
        [DataMember(Name = "codeFlows", IsRequired = false, EmitDefaultValue = false)]
        public IList<CodeFlow> CodeFlows { get; set; }

        /// <summary>
        /// A grouped set of locations and messages, if available, that represent code areas that are related to this result.
        /// </summary>
        [DataMember(Name = "relatedLocations", IsRequired = false, EmitDefaultValue = false)]
        public IList<AnnotatedCodeLocation> RelatedLocations { get; set; }
        [DataMember(Name = "suppressionStates", IsRequired = false, EmitDefaultValue = false)]
        public SuppressionStates SuppressionStates { get; set; }

        /// <summary>
        /// The state of a result relative to a baseline of a previous run.
        /// </summary>
        [DataMember(Name = "baselineState", IsRequired = false, EmitDefaultValue = false)]
        public BaselineState BaselineState { get; set; }

        /// <summary>
        /// An array of 'fix' objects, each of which represents a proposed fix to the problem indicated by the result.
        /// </summary>
        [DataMember(Name = "fixes", IsRequired = false, EmitDefaultValue = false)]
        public IList<Fix> Fixes { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the result.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

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
        /// <param name="ruleKey">
        /// An initialization value for the <see cref="P: RuleKey" /> property.
        /// </param>
        /// <param name="level">
        /// An initialization value for the <see cref="P: Level" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="formattedRuleMessage">
        /// An initialization value for the <see cref="P: FormattedRuleMessage" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P: Locations" /> property.
        /// </param>
        /// <param name="snippet">
        /// An initialization value for the <see cref="P: Snippet" /> property.
        /// </param>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="toolFingerprintContribution">
        /// An initialization value for the <see cref="P: ToolFingerprintContribution" /> property.
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
        /// <param name="suppressionStates">
        /// An initialization value for the <see cref="P: SuppressionStates" /> property.
        /// </param>
        /// <param name="baselineState">
        /// An initialization value for the <see cref="P: BaselineState" /> property.
        /// </param>
        /// <param name="fixes">
        /// An initialization value for the <see cref="P: Fixes" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Result(string ruleId, string ruleKey, ResultLevel level, string message, FormattedRuleMessage formattedRuleMessage, IEnumerable<Location> locations, string snippet, string id, string toolFingerprintContribution, IEnumerable<Stack> stacks, IEnumerable<CodeFlow> codeFlows, IEnumerable<AnnotatedCodeLocation> relatedLocations, SuppressionStates suppressionStates, BaselineState baselineState, IEnumerable<Fix> fixes, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(ruleId, ruleKey, level, message, formattedRuleMessage, locations, snippet, id, toolFingerprintContribution, stacks, codeFlows, relatedLocations, suppressionStates, baselineState, fixes, properties);
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

            Init(other.RuleId, other.RuleKey, other.Level, other.Message, other.FormattedRuleMessage, other.Locations, other.Snippet, other.Id, other.ToolFingerprintContribution, other.Stacks, other.CodeFlows, other.RelatedLocations, other.SuppressionStates, other.BaselineState, other.Fixes, other.Properties);
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

        private void Init(string ruleId, string ruleKey, ResultLevel level, string message, FormattedRuleMessage formattedRuleMessage, IEnumerable<Location> locations, string snippet, string id, string toolFingerprintContribution, IEnumerable<Stack> stacks, IEnumerable<CodeFlow> codeFlows, IEnumerable<AnnotatedCodeLocation> relatedLocations, SuppressionStates suppressionStates, BaselineState baselineState, IEnumerable<Fix> fixes, IDictionary<string, SerializedPropertyInfo> properties)
        {
            RuleId = ruleId;
            RuleKey = ruleKey;
            Level = level;
            Message = message;
            if (formattedRuleMessage != null)
            {
                FormattedRuleMessage = new FormattedRuleMessage(formattedRuleMessage);
            }

            if (locations != null)
            {
                var destination_0 = new List<Location>();
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

            Snippet = snippet;
            Id = id;
            ToolFingerprintContribution = toolFingerprintContribution;
            if (stacks != null)
            {
                var destination_1 = new List<Stack>();
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
                var destination_2 = new List<CodeFlow>();
                foreach (var value_2 in codeFlows)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new CodeFlow(value_2));
                    }
                }

                CodeFlows = destination_2;
            }

            if (relatedLocations != null)
            {
                var destination_3 = new List<AnnotatedCodeLocation>();
                foreach (var value_3 in relatedLocations)
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

                RelatedLocations = destination_3;
            }

            SuppressionStates = suppressionStates;
            BaselineState = baselineState;
            if (fixes != null)
            {
                var destination_4 = new List<Fix>();
                foreach (var value_4 in fixes)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Fix(value_4));
                    }
                }

                Fixes = destination_4;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}