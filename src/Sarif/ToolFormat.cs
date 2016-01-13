// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>Values referring to a source format to convert to the static analysis results interchange format.</summary>
    public enum ToolFormat
    {
        /// <summary>An unset tool format value.</summary>
        None = 0,
        /// <summary>Android Studio's file format.</summary>
        AndroidStudio,
        /// <summary>Clang analyzer's file format.</summary>
        ClangAnalyzer,
        /// <summary>CppCheck's file format.</summary>
        CppCheck,
        /// <summary>Fortify's file format.</summary>
        Fortify,
        /// <summary>FxCop's file format.</summary>
        FxCop,
        /// <summary>PREfast's file format.</summary>
        PREfast,
    }
}
