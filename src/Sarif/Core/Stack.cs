// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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
        /// <summary>
        /// Create one or more Stack instances from a .NET exception. Captures
        /// inner exceptions and handles aggregated exceptions.
        /// </summary>
        /// <param name="exception"></param>
        public static ISet<Stack> CreateStacks(Exception exception)
        {
            HashSet<Stack> stacks;
            Queue<Exception> exceptions;

            stacks = new HashSet<Stack>();
            exceptions = new Queue<Exception>(new Exception[] { exception });

            while (exceptions.Count > 0)
            {
                Stack stack;
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

                stack.Message = new Message { Text = current.FormatMessage() };

                stacks.Add(stack);
            }

            return stacks;
        }

        /// <summary>
        /// Creates a SARIF Stack instance from a .NET StackTrace instance
        /// </summary>
        /// <param name="stackTrace"></param>
        public Stack(StackTrace stackTrace)
        {
            if (stackTrace.FrameCount == 0)
            {
                return;
            }

            this.Frames = new StackFrame[stackTrace.FrameCount];

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                this.Frames[i] = StackFrame.Create(stackTrace.GetFrame(i));
            }
        }

        /// <summary>
        /// Creates a SARIF Stack instance from a .NET StackTrace
        /// text representation (as returned by StackTrace.ToString())
        /// </summary>
        /// <param name="stackTrace"></param>
        public static Stack Create(string stackTrace)
        {
            Stack stack = new Stack();

            if (string.IsNullOrEmpty(stackTrace))
            {
                return stack;
            }

            stack.Frames = new List<StackFrame>();

            var regex = new Regex(StackFrame.AT + @"([^)]+\))(" + StackFrame.IN + @"([^:]+:[^:]+)" + StackFrame.LINE + @" (.*))?", RegexOptions.Compiled);

            foreach (string line in stackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                // at Type.Method() in File.cs : line X
                string current = line;

                var stackFrame = new StackFrame();

                Match match = regex.Match(line);

                if (match.Success)
                {
                    stackFrame.Location = new Location();
                    if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        stackFrame.Location.LogicalLocation = new LogicalLocation
                        {
                            FullyQualifiedName = match.Groups[1].Value
                        };
                    }

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        string fileName = match.Groups[3].Value;
                        int lineNumber = int.Parse(match.Groups[4].Value);

                        stackFrame.Location.PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(fileName)
                            },
                            Region = new Region
                            {
                                StartLine = lineNumber
                            }
                        };
                    }
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
            if (this.Frames == null) { return "[No frames]"; }

            StringBuilder sb = new StringBuilder(255);

            for (int i = 0; i < this.Frames.Count; i++)
            {
                StackFrame sf = this.Frames[i];

                if (i > 0) { sb.AppendLine(); }

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
