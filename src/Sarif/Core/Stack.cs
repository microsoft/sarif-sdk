// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DotNetStackFrame = System.Diagnostics.StackFrame;

namespace Microsoft.CodeAnalysis.Sarif
{
    public enum StackFormat
    {
        Default,
        TrailingNewLine
    }

    /// <summary>
    /// A call stack relevant to a SARIF result.
    /// </summary>
    public partial class Stack
    {
        private const string IN = " in ";
        private const string AT = "   at ";
        private const string LINE = ":line";

        /// <summary>
        /// Creates a SARIF Stack instance from a .NET StackTrace instance
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        public static Stack Create(StackTrace stackTrace)
        {
            Stack stack = new Stack();

            if (stackTrace.FrameCount == 0) { return stack; }

            stack.Frames = new StackFrame[stackTrace.FrameCount];

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                stack.Frames[i] = StackFrame.Create(stackTrace.GetFrame(i));                
            }
            return stack;
        }

        public static IEnumerable<Stack> Create(Exception exception)
        {
            List<Stack> stacks;
            Queue<Exception> exceptions;

            stacks = new List<Stack>();
            exceptions = new Queue<Exception>(new Exception[] { exception });

            while (exceptions.Count > 0)
            {
                Stack stack;
                Exception current;
                
                current = exceptions.Dequeue();

                if (current.InnerException != null)
                {
                    exceptions.Enqueue(current.InnerException);
                }

                stack = Create(current.StackTrace);

                stack.Message = current.Message;

                stacks.Add(stack);
            }

            return stacks;
        }

        public static Stack Create(string stackTrace)
        {
            Stack stack = new Stack();
            
            if (string.IsNullOrEmpty(stackTrace))
            {
                return stack;
            }

            stack.Frames = new List<StackFrame>();

            foreach (string line in stackTrace.Split('\n'))
            {
                // at Type.Method() in File.cs : line X\r
                string current = line;

                StackFrame stackFrame;
                stackFrame = new StackFrame();

                // at Type.Method() in File.cs: line X
                current = line.Trim('\r');

                // Type.Method() in File.cs: line X
                current = current.Substring(AT.Length);
                stackFrame.FullyQualifiedLogicalName = current.Split(')')[0] + ")";

                // in File.cs: line X
                current = current.Substring(stackFrame.FullyQualifiedLogicalName.Length);

                if (current.Length > 0)
                {
                    int lineNumber;
                    string fileName;

                    // File.cs: line X
                    current = current.Substring(IN.Length);

                    // :line X
                    fileName = current.Split(' ')[0];
                    fileName = fileName.Substring(0, fileName.Length - LINE.Length);

                    // X
                    current = current.Substring(fileName.Length + LINE.Length);
                    lineNumber = int.Parse(current);

                    stackFrame.Location = new PhysicalLocation()
                    {
                        Uri = new Uri(fileName),
                        Region = new Region()
                        {
                           StartLine = lineNumber
                        }
                    };
                }
                stack.Frames.Add(stackFrame);
            }

            return stack;
        }

        public override string ToString()
        {
            return ToString(StackFormat.Default);
        }

        public string ToString(StackFormat stackFormat)
        {           
            if (this.Frames == null) { return null; }

            StringBuilder sb = new StringBuilder(255);

            for (int iFrameIndex = 0; iFrameIndex < this.Frames.Count; iFrameIndex++)
            {
                StackFrame sf = this.Frames[iFrameIndex];

                if (iFrameIndex > 0) { sb.AppendLine(); }

                sb.Append(sf.ToString());
            }

            if (stackFormat == StackFormat.TrailingNewLine)
            {
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
