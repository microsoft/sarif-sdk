using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
{
    class MemoryStreamToStringBuilder : MemoryStream
    {
        private StringBuilder OutputTo { get; set; }

        public MemoryStreamToStringBuilder(StringBuilder outputTo)
        {
            OutputTo = outputTo;
        }

        protected override void Dispose(bool disposing)
        {
            OutputTo.Append(Encoding.UTF8.GetString(this.ToArray()));
            base.Dispose(disposing);
        }
    }
}
