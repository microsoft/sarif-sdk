// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>Values referring to a source format to convert to the static analysis results interchange format.</summary>
    public static class ToolFormat
    {
        /// <summary>An unset tool format value.</summary>
        public const string None = nameof(None);

        /// <summary>Android Studio's file format.</summary>
        public const string AndroidStudio = nameof(AndroidStudio);

        /// <summary>Clang analyzer's file format.</summary>
        public const string ClangAnalyzer = nameof(ClangAnalyzer);

        /// <summary>CppCheck's file format.</summary>
        public const string CppCheck = nameof(CppCheck);

        /// <summary>Fortify's report file format.</summary>
        public const string Fortify = nameof(Fortify);

        /// <summary>Fortify's FPR file format.</summary>
        public const string FortifyFpr = nameof(FortifyFpr);

        /// <summary>FxCop's file format.</summary>
        public const string FxCop = nameof(FxCop);

        /// <summary>PREfast's file format.</summary>
        public const string PREfast = nameof(PREfast);

        /// <summary>Semmle's file format.</summary>
        public const string SemmleQL = nameof(SemmleQL);

        /// <summary>TSLint's file format.</summary>
        public const string TSLint = nameof(TSLint);

        /// <summary>Static Driver Verifier's file format.</summary>
        public const string StaticDriverVerifier = nameof(StaticDriverVerifier);
    }
}
