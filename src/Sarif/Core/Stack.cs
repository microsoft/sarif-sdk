// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DotNetStackFrame = System.Diagnostics.StackFrame;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A call stack relevant to a SARIF result.
    /// </summary>
    public partial class Stack
    {
        /// <summary>
        /// Creates a SARIF Stack instance from a .NET StackTrace instance
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        public static Stack Create(StackTrace stackTrace)
        {
            Stack stack = new Stack();
            stack.Frames = new StackFrame[stackTrace.FrameCount];

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                int ilOffset;
                string fileName;
                PhysicalLocation physicalLocation;
                DotNetStackFrame dotNetStackFrame;

                physicalLocation = null;
                dotNetStackFrame = stackTrace.GetFrame(i);
                fileName = dotNetStackFrame.GetFileName();

                ilOffset = dotNetStackFrame.GetILOffset();

                if (!string.IsNullOrEmpty(fileName))
                {
                    physicalLocation = new PhysicalLocation()
                    {
                        Uri = new Uri(fileName),
                        Region = new Region()
                        {
                            StartLine = dotNetStackFrame.GetFileLineNumber(),
                            StartColumn = dotNetStackFrame.GetFileColumnNumber(),
                            Offset = ilOffset
                        }                         
                    };
                }
                else
                {
                    physicalLocation = new PhysicalLocation()
                    {
                        Region = new Region() { Offset = ilOffset }
                    };
                }

                stack.Frames[i] = new StackFrame()
                {
                    FullyQualifiedLogicalName = CreateFullyQualifiedName(dotNetStackFrame.GetMethod()),
                    SourceFile = physicalLocation,
                   
                };
            }
            return stack;
        }

        private static string CreateFullyQualifiedName(MethodBase method)
        {
            return method.ReflectedType.FullName + "." + method.Name);
        }
    }
}
