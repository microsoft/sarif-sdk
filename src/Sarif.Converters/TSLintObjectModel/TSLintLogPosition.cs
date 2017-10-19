// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel
{
    [DataContract]
    public class TSLintLogPosition
    {
        [DataMember(Name = "line", IsRequired = false, EmitDefaultValue = true)]
        public int Line { get; set; }

        [DataMember(Name = "character", IsRequired = false, EmitDefaultValue = true)]
        public int Character { get; set; }

        [DataMember(Name = "position", IsRequired = false, EmitDefaultValue = true)]
        public int Position { get; set; }
    }
}
