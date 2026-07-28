// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Reports whether any write to <see cref="RuntimeErrors"/> discarded a flag that was set
    /// after the writer read the property, which is what an unsynchronized <c>|=</c> does when two
    /// threads merge at once.
    ///
    /// The getter waits a bounded interval for a second thread to join it, so competing merges
    /// reliably overlap. Serialized merges never find a partner and simply pay the wait, which is
    /// why the stall budget is capped and why <see cref="BeginProbing"/> confines the budget to
    /// the scan phase rather than spending it on single-threaded configuration reads.
    ///
    /// This lives beside <see cref="TestAnalysisContext"/> rather than in the driver unit test
    /// assembly because that assembly is passed to ExportConfigurationCommandBase as a plugin
    /// assembly, which discovers IOptionsProvider implementations by convention and would emit an
    /// extra configuration section for this type.
    /// </summary>
    public sealed class MergeProbingContext : TestAnalysisContext
    {
        private const int RendezvousMilliseconds = 50;
        private const int StallBudget = 40;

        private RuntimeConditions runtimeErrors;
        private int readersInFlight;
        private int stallsRemaining = StallBudget;
        private volatile bool probing;

        public bool ClobberedMerge { get; private set; }

        public void BeginProbing() => this.probing = true;

        public override RuntimeConditions RuntimeErrors
        {
            get
            {
                if (this.probing && Interlocked.Decrement(ref this.stallsRemaining) >= 0) { AwaitPartner(); }
                return this.runtimeErrors;
            }

            set
            {
                if ((this.runtimeErrors & ~value) != RuntimeConditions.None)
                {
                    ClobberedMerge = true;
                }

                this.runtimeErrors = value;
            }
        }

        private void AwaitPartner()
        {
            Interlocked.Increment(ref this.readersInFlight);

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var spin = new SpinWait();

                while (Volatile.Read(ref this.readersInFlight) < 2 &&
                       stopwatch.ElapsedMilliseconds < RendezvousMilliseconds)
                {
                    spin.SpinOnce();
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.readersInFlight);
            }
        }
    }
}
