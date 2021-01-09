// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    public class TestTextWriter : TextWriter
    {
        public override Encoding Encoding => throw new NotImplementedException();
        public override void WriteLine(string value)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(char[] buffer)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(object value)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(char[] buffer, int index, int count)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(string value)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void WriteLine()
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(string format, params object[] arg)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(string format, object arg0)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(bool value)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(string format, object arg0, object arg1)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            throw new InvalidOperationException("Console was called");
        }

        public override void WriteLine(string format, params object[] arg)
        {
            throw new InvalidOperationException("Console was called");
        }
    }
}
