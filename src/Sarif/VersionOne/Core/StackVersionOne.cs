// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    public enum StackFormatVersionOne
    {
        Default,
        TrailingNewLine
    }

    /// <summary>
    /// A call stack relevant to a SARIF result.
    /// </summary>
    public partial class StackVersionOne
    {
        /// <summary>
        /// Create one or more StackVersionOne instances from a .NET exception. Captures
        /// inner exceptions and handles aggregated exceptions.
        /// </summary>
        /// <param name="exception"></param>
        public static ISet<StackVersionOne> CreateStacks(Exception exception)
        {
            HashSet<StackVersionOne> stacks;
            Queue<Exception> exceptions;

            stacks = new HashSet<StackVersionOne>();
            exceptions = new Queue<Exception>(new Exception[] { exception });

            while (exceptions.Count > 0)
            {
                StackVersionOne stack;
                Exception current;

                current = exceptions.Dequeue();

                var aggregated = current as AggregateException;
                if (aggregated != null)
                {
                    foreach (Exception e in aggregated.InnerExceptions)
                    {
                        exceptions.Enqueue(e);
                    }
                }
                else
                {
                    // AggregatedExceptions surface the first exception
                    // in the aggregation as InnerException, so we don't
                    // reexamine this property for that exception type (as
                    // it is already enqueued from inspecting InnerExceptions).
                    if (current.InnerException != null)
                    {
                        exceptions.Enqueue(current.InnerException);
                    }
                }

                stack = Create(current.StackTrace);

                stack.Message = current.FormatMessage();

                stacks.Add(stack);
            }

            return stacks;
        }

        /// <summary>
        /// Creates a SARIF StackVersionOne instance from a .NET StackTrace instance
        /// </summary>
        /// <param name="stackTrace"></param>
        public StackVersionOne(StackTrace stackTrace)
        {
            if (stackTrace.FrameCount == 0)
            {
                return;
            }

            this.Frames = new StackFrameVersionOne[stackTrace.FrameCount];

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                this.Frames[i] = StackFrameVersionOne.Create(stackTrace.GetFrame(i));                
            }
        }

        /// <summary>
        /// Creates a SARIF StackVersionOne instance from a .NET StackTrace
        /// text representation (as returned by StackTrace.ToString())
        /// </summary>
        /// <param name="stackTrace"></param>
        public static StackVersionOne Create(string stackTrace)
        {
            StackVersionOne stack = new StackVersionOne();
            
            if (string.IsNullOrEmpty(stackTrace))
            {
                return stack;
            }

            stack.Frames = new List<StackFrameVersionOne>();

            var regex = new Regex(StackFrameVersionOne.AT + @"([^)]+\))(" + StackFrameVersionOne.IN + @"([^:]+:[^:]+)" + StackFrameVersionOne.LINE + @" (.*))?", RegexOptions.Compiled);

            foreach (string line in stackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                // at Type.Method() in File.cs : line X
                string current = line;

                var stackFrame = new StackFrameVersionOne();

                Match match = regex.Match(line);

                if (match.Success)
                {
                    stackFrame.FullyQualifiedLogicalName = match.Groups[1].Value;

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        string fileName = match.Groups[3].Value;
                        int lineNumber = int.Parse(match.Groups[4].Value);

                        stackFrame.Uri = new Uri(fileName);
                        stackFrame.Line = lineNumber;
                    }
                }
                stack.Frames.Add(stackFrame);
            }

            return stack;
        }

        public override string ToString()
        {
            return ToString(StackFormatVersionOne.Default);
        }

        public string ToString(StackFormatVersionOne stackFormat)
        {           
            if (this.Frames == null) { return "[No frames]"; }

            StringBuilder sb = new StringBuilder(255);

            for (int i = 0; i < this.Frames.Count; i++)
            {
                StackFrameVersionOne sf = this.Frames[i];

                if (i > 0) { sb.AppendLine(); }

                sb.Append(sf.ToString());
            }

            if (stackFormat == StackFormatVersionOne.TrailingNewLine)
            {
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
