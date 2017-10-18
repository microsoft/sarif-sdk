// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel
{
    [DataContract]
    public class TSLintLogEntry
    {
        [DataMember(Name = "name", IsRequired = true, EmitDefaultValue = true)]
        public string Name { get; set; }

        [DataMember(Name = "failure", IsRequired = true, EmitDefaultValue = true)]
        public string Failure { get; set; }

        [DataMember(Name = "ruleName", IsRequired = true, EmitDefaultValue = true)]
        public string RuleName { get; set; }

        [DataMember(Name = "ruleSeverity", IsRequired = false, EmitDefaultValue = true)]
        public string RuleSeverity { get; set; }

        [DataMember(Name = "startPosition", IsRequired = false, EmitDefaultValue = true)]
        public TSLintLogPosition StartPosition { get; set; }

        [DataMember(Name = "endPosition", IsRequired = false, EmitDefaultValue = true)]
        public TSLintLogPosition EndPosition { get; set; }

        [DataMember(Name = "fix", IsRequired = false, EmitDefaultValue = true)]
        public IList<TSLintLogFix> Fixes { get; set; }
    }
}
