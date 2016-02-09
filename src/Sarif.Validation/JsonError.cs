// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

namespace Microsoft.CodeAnalysis.Sarif.Validation
{
    public enum JsonErrorKind
    {
        Syntax,
        Validation
    }

    public enum JsonErrorLocation
    {
        InstanceDocument,
        Schema
    }

    public class JsonError
    {
        public JsonErrorLocation Location { get; set; }

        public JsonErrorKind Kind { get; set; }

        public string Message { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public override string ToString()
        {
            return $"{Location}: {Kind}: ({Start}, {Length}): {Message}";
        }
    }
}
