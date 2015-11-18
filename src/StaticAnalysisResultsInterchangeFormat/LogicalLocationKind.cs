// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat
{
    /// <summary>
    /// Possible values for the property <see cref="LogicalLocationComponent.LocationKind"/>
    /// </summary>
    public static class LogicalLocationKind
    {
        /// <summary>The MIME type for android modules.</summary>
        public static readonly string AndroidModule = "android-module";

        /// <summary>The MIME type for namespaces in the CLR.</summary>
        public static readonly string ClrNamespace = "namespace";
        /// <summary>The MIME type for functions in the CLR.</summary>
        public static readonly string ClrFunction = "method";
        /// <summary>The MIME type for types in the CLR.</summary>
        public static readonly string ClrType = "type";
        /// <summary>The MIME type for embedded resources in the CLR.</summary>
        public static readonly string ClrResource = "resource";
        /// <summary>The MIME type for modules in the CLR.</summary>
        public static readonly string ClrModule = "module";

        /// <summary>The MIME type for packages in the JVM.</summary>
        public static readonly string JvmPackage = "package";
        /// <summary>The MIME type for functions in the JVM.</summary>
        public static readonly string JvmFunction = "method";
        /// <summary>The MIME type for classes in the JVM.</summary>
        public static readonly string JvmType = "type";
    }
}
