// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Possible values for the property <see cref="LogicalLocationComponent.LocationKind"/>
    /// </summary>
    public static class LogicalLocationKind
    {
        public static readonly string Declaration = "declaration";
        public static readonly string Function = "function";
        public static readonly string Member = "member";
        public static readonly string Module = "module";
        public static readonly string Namespace = "namespace";
        public static readonly string Package = "package";
        public static readonly string Resource = "resource";
        public static readonly string Type = "type";
    }
}
