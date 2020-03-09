// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel
{
    [DataContract]
    public class TSLintLogFix
    {
        [DataMember(Name = "innerStart", IsRequired = false, EmitDefaultValue = true)]
        public int InnerStart { get; set; }

        [DataMember(Name = "innerLength", IsRequired = false, EmitDefaultValue = true)]
        public int InnerLength { get; set; }

        [DataMember(Name = "innerText", IsRequired = false, EmitDefaultValue = true)]
        public string InnerText { get; set; }
    }
}
