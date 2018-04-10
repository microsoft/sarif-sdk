// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
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
        /// A message that describes the result. The first sentence of the message only will be displayed when visible space is limited.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public Message Message { get; set; }

        /// <summary>
        /// A string that identifies the message within the metadata for the rule used in this result.
        /// </summary>
        [DataMember(Name = "ruleMessageId", IsRequired = false, EmitDefaultValue = false)]
        public string RuleMessageId { get; set; }

        /// <summary>
        /// Identifies the file that the analysis tool was instructed to scan. This need not be the same as the file where the result actually occurred.
        /// </summary>
        [DataMember(Name = "analysisTarget", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation AnalysisTarget { get; set; }

        /// <summary>
        /// A source code or other file fragment that illustrates the result.
        /// </summary>
        [DataMember(Name = "snippet", IsRequired = false, EmitDefaultValue = false)]
        public FileContent Snippet { get; set; }

        /// <summary>
        /// One or more locations where the result occurred. Specify only one location unless the problem indicated by the result can only be corrected by making a change at every specified location.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = false, EmitDefaultValue = false)]
        public IList<Location> Locations { get; set; }

        /// <summary>
        /// A unique identifer for the result.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// A set of strings that contribute to the unique identity of the result.
        /// </summary>
        [DataMember(Name = "toolFingerprintContributions", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> ToolFingerprintContributions { get; set; }

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
        /// A set of locations relevant to this result.
        /// </summary>
        [DataMember(Name = "relatedLocations", IsRequired = false, EmitDefaultValue = false)]
        public IList<Location> RelatedLocations { get; set; }
        [DataMember(Name = "suppressionStates", IsRequired = false, EmitDefaultValue = false)]
        public SuppressionStates SuppressionStates { get; set; }

        /// <summary>
        /// A set of files relevant to the result.
        /// </summary>
        [DataMember(Name = "attachments", IsRequired = false, EmitDefaultValue = false)]
        public IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// The state of a result relative to a baseline of a previous run.
        /// </summary>
        [DataMember(Name = "baselineState", IsRequired = false, EmitDefaultValue = false)]
        public BaselineState BaselineState { get; set; }

        /// <summary>
        /// An array of analysisToolLogFileContents objects which specify the portions of an analysis tool's output that a converter transformed into the result object.
        /// </summary>
        [DataMember(Name = "conversionProvenance", IsRequired = false, EmitDefaultValue = false)]
        public IList<PhysicalLocation> ConversionProvenance { get; set; }

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
        /// <param name="ruleMessageId">
        /// An initialization value for the <see cref="P: RuleMessageId" /> property.
        /// </param>
        /// <param name="analysisTarget">
        /// An initialization value for the <see cref="P: AnalysisTarget" /> property.
        /// </param>
        /// <param name="snippet">
        /// An initialization value for the <see cref="P: Snippet" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P: Locations" /> property.
        /// </param>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="toolFingerprintContributions">
        /// An initialization value for the <see cref="P: ToolFingerprintContributions" /> property.
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
        /// <param name="attachments">
        /// An initialization value for the <see cref="P: Attachments" /> property.
        /// </param>
        /// <param name="baselineState">
        /// An initialization value for the <see cref="P: BaselineState" /> property.
        /// </param>
        /// <param name="conversionProvenance">
        /// An initialization value for the <see cref="P: ConversionProvenance" /> property.
        /// </param>
        /// <param name="fixes">
        /// An initialization value for the <see cref="P: Fixes" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Result(string ruleId, string ruleKey, ResultLevel level, Message message, string ruleMessageId, FileLocation analysisTarget, FileContent snippet, IEnumerable<Location> locations, string id, IDictionary<string, string> toolFingerprintContributions, IEnumerable<Stack> stacks, IEnumerable<CodeFlow> codeFlows, IEnumerable<Location> relatedLocations, SuppressionStates suppressionStates, IEnumerable<Attachment> attachments, BaselineState baselineState, IEnumerable<PhysicalLocation> conversionProvenance, IEnumerable<Fix> fixes, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(ruleId, ruleKey, level, message, ruleMessageId, analysisTarget, snippet, locations, id, toolFingerprintContributions, stacks, codeFlows, relatedLocations, suppressionStates, attachments, baselineState, conversionProvenance, fixes, properties);
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

            Init(other.RuleId, other.RuleKey, other.Level, other.Message, other.RuleMessageId, other.AnalysisTarget, other.Snippet, other.Locations, other.Id, other.ToolFingerprintContributions, other.Stacks, other.CodeFlows, other.RelatedLocations, other.SuppressionStates, other.Attachments, other.BaselineState, other.ConversionProvenance, other.Fixes, other.Properties);
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

        private void Init(string ruleId, string ruleKey, ResultLevel level, Message message, string ruleMessageId, FileLocation analysisTarget, FileContent snippet, IEnumerable<Location> locations, string id, IDictionary<string, string> toolFingerprintContributions, IEnumerable<Stack> stacks, IEnumerable<CodeFlow> codeFlows, IEnumerable<Location> relatedLocations, SuppressionStates suppressionStates, IEnumerable<Attachment> attachments, BaselineState baselineState, IEnumerable<PhysicalLocation> conversionProvenance, IEnumerable<Fix> fixes, IDictionary<string, SerializedPropertyInfo> properties)
        {
            RuleId = ruleId;
            RuleKey = ruleKey;
            Level = level;
            if (message != null)
            {
                Message = new Message(message);
            }

            RuleMessageId = ruleMessageId;
            if (analysisTarget != null)
            {
                AnalysisTarget = new FileLocation(analysisTarget);
            }

            if (snippet != null)
            {
                Snippet = new FileContent(snippet);
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

            Id = id;
            if (toolFingerprintContributions != null)
            {
                ToolFingerprintContributions = new Dictionary<string, string>(toolFingerprintContributions);
            }

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
                var destination_3 = new List<Location>();
                foreach (var value_3 in relatedLocations)
                {
                    if (value_3 == null)
                    {
                        destination_3.Add(null);
                    }
                    else
                    {
                        destination_3.Add(new Location(value_3));
                    }
                }

                RelatedLocations = destination_3;
            }

            SuppressionStates = suppressionStates;
            if (attachments != null)
            {
                var destination_4 = new List<Attachment>();
                foreach (var value_4 in attachments)
                {
                    if (value_4 == null)
                    {
                        destination_4.Add(null);
                    }
                    else
                    {
                        destination_4.Add(new Attachment(value_4));
                    }
                }

                Attachments = destination_4;
            }

            BaselineState = baselineState;
            if (conversionProvenance != null)
            {
                var destination_5 = new List<PhysicalLocation>();
                foreach (var value_5 in conversionProvenance)
                {
                    if (value_5 == null)
                    {
                        destination_5.Add(null);
                    }
                    else
                    {
                        destination_5.Add(new PhysicalLocation(value_5));
                    }
                }

                ConversionProvenance = destination_5;
            }

            if (fixes != null)
            {
                var destination_6 = new List<Fix>();
                foreach (var value_6 in fixes)
                {
                    if (value_6 == null)
                    {
                        destination_6.Add(null);
                    }
                    else
                    {
                        destination_6.Add(new Fix(value_6));
                    }
                }

                Fixes = destination_6;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}