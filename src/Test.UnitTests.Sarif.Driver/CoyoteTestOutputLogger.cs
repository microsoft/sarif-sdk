﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Microsoft.Coyote.Logging;

using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// Logger that writes to the test output.
    /// </summary>
    public sealed class CoyoteTestOutputLogger : TextWriter, ILogger
    {
        /// <summary>
        /// Underlying test output.
        /// </summary>
        private readonly ITestOutputHelper TestOutput;

        /// <summary>
        /// Saves the log until the end of the test.
        /// </summary>
        private readonly StringBuilder Log;

        /// <summary>
        /// Serializes access to the string writer.
        /// </summary>
        private readonly object Lock;

        /// <inheritdoc/>
        public TextWriter TextWriter => this;

        /// <inheritdoc/>
        public override Encoding Encoding => Encoding.Unicode;

        /// <summary>
        /// True if this logger is disposed, else false.
        /// </summary>
        private bool IsDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestOutputLogger"/> class.
        /// </summary>
        /// <param name="output">The test output helper.</param>
        public CoyoteTestOutputLogger(ITestOutputHelper output)
        {
            this.TestOutput = output;
            this.Log = new StringBuilder();
            this.Lock = new object();
            this.IsDisposed = false;
        }

        /// <inheritdoc/>
        public override void Write(string value) =>
            this.Write(LogSeverity.Info, value);

        /// <inheritdoc/>
        public override void Write(string format, params object[] args) =>
            this.Write(LogSeverity.Info, string.Format(format, args));

        public void Write(LogSeverity severity, string value)
        {
            lock (this.Lock)
            {
                if (!this.IsDisposed)
                {
                    this.Log.Append(value);
                }
            }
        }

        /// <inheritdoc/>
        public void Write(LogSeverity severity, string format, params object[] args) =>
            this.Write(severity, string.Format(format, args));

        /// <inheritdoc/>
        public override void WriteLine(string value) => this.WriteLine(LogSeverity.Info, value);

        /// <inheritdoc/>
        public override void WriteLine(string format, params object[] args) =>
            this.WriteLine(LogSeverity.Info, string.Format(format, args));

        /// <inheritdoc/>
        public void WriteLine(LogSeverity severity, string value)
        {
            lock (this.Lock)
            {
                if (!this.IsDisposed)
                {
                    this.Log.AppendLine(value);
                }
            }
        }

        /// <inheritdoc/>
        public void WriteLine(LogSeverity severity, string format, params object[] args) =>
            this.WriteLine(severity, string.Format(format, args));

        /// <summary>
        /// Write all buffered log to the test output logger.
        /// </summary>
        private void FlushLog()
        {
            if (this.Log.Length > 0)
            {
                this.TestOutput.WriteLine(this.Log.ToString());
                this.Log.Clear();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (this.Lock)
            {
                if (!this.IsDisposed)
                {
                    this.FlushLog();
                    this.IsDisposed = true;
                }
            }

            base.Dispose(disposing);
        }

        public void Write(LogSeverity severity, string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void Write(LogSeverity severity, string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void Write(LogSeverity severity, string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(LogSeverity severity, string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(LogSeverity severity, string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(LogSeverity severity, string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }
    }
}
