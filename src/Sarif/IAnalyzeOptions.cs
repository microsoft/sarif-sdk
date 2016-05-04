// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalyzeOptions
    {
        IEnumerable<string> TargetFileSpecifiers { get; }

        string OutputFilePath { get; }

        bool Verbose { get; }

        bool Recurse { get; }

        string ConfigurationFilePath { get; }

        bool Statistics { get; }

        bool ComputeTargetsHash { get; }

        bool LogEnvironment { get; }

        IEnumerable<string> PlugInFilePaths { get; }
    }
}