// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        private static Graph EmptyGraph = new Graph();
        private static FileData EmptyFile = new FileData();
        private static Invocation EmptyInvocation = new Invocation();
        private static LogicalLocation EmptyLogicalLocation = new LogicalLocation();

        public bool ShouldSerializeFiles() { return this.Files != null && this.Files.Values.Any(); }

        public bool ShouldSerializeGraphs() { return this.Graphs != null && this.Graphs.Values.Any(); }

        public bool ShouldSerializeInvocations() { return this.Invocations != null && this.Invocations.Any((e) => e != null && !e.ValueEquals(EmptyInvocation)); }

        public bool ShouldSerializeLogicalLocations() { return this.LogicalLocations != null && this.LogicalLocations.Values.Any(); }
    }
}
