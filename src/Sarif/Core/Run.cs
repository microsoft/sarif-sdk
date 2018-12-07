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

        public bool ShouldSerializeColumnKind()
        {
            // This serialization helper does two things. 
            // 
            // First, if ColumnKind has not been 
            // explicitly set, we will set it to the value that works for the Microsoft 
            // platform (which is not the specified SARIF default). This makes sure that
            // the value is set appropriate for code running on the Microsoft platform, 
            // even if the SARIF producer is not aware of this rather obscure value. 
            if (this.ColumnKind == ColumnKind.None)
            {
                this.ColumnKind = ColumnKind.Utf16CodeUnits;
            }
            
            // Second, we do not persist this value if it is set to its default.
            return this.ColumnKind != ColumnKind.UnicodeCodePoints;
        }

        public bool ShouldSerializeFiles() { return this.Files != null && this.Files.Values.Any(); }

        public bool ShouldSerializeGraphs() { return this.Graphs != null && this.Graphs.Values.Any(); }

        public bool ShouldSerializeInvocations() { return this.Invocations != null && this.Invocations.Any((e) => e != null && !e.ValueEquals(EmptyInvocation)); }

        public bool ShouldSerializeLogicalLocations() { return this.LogicalLocations != null && this.LogicalLocations.Values.Any(); }

        public bool ShouldSerializeNewlineSequences() { return this.NewlineSequences != null && this.NewlineSequences.Any((s) => s != null); }
    }
}
