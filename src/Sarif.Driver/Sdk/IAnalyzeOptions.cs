// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public interface IAnalyzeOptions
    {
        IEnumerable<string> TargetFileSpecifiers { get; }

        string OutputFilePath { get; }

        bool Verbose { get; }

        bool Recurse { get; }

        string PolicyFilePath { get; }

        bool Statistics { get; }

        bool ComputeTargetsHash { get; }

        IList<string> PlugInFilePaths { get; }
    }
}