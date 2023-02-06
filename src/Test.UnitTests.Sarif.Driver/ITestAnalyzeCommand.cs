// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal interface ITestAnalyzeCommand
    {
        IEnumerable<Assembly> DefaultPluginAssemblies { get; set; }

        Exception ExecutionException { get; set; }

        int Run(AnalyzeOptionsBase options);

        void CheckIncompatibleRules(IEnumerable<Skimmer<TestAnalysisContext>> skimmers, TestAnalysisContext context, ISet<string> disabledSkimmers);
    }
}
