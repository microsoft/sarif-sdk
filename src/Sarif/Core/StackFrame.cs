// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using DotNetStackFrame = System.Diagnostics.StackFrame;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A stack frame of a SARIF stack.
    /// </summary>
    public partial class StackFrame
    {
        internal const string IN = " in ";
        internal const string AT = "   at ";
        internal const string LINE = ":line";

        /// <summary>
        /// Creates a SARIF StackFrame instance from a .NET StackFrame instance
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        public static StackFrame Create(System.Diagnostics.StackFrame dotNetStackFrame)
        {
            // This value is -1 if not present
            string fileName = dotNetStackFrame.GetFileName();
            int ilOffset = dotNetStackFrame.GetILOffset();
            int nativeOffset = dotNetStackFrame.GetNativeOffset();
            MethodBase methodBase = dotNetStackFrame.GetMethod();
            Assembly assembly = methodBase?.DeclaringType.Assembly;
            string fullyQualifiedName = CreateFullyQualifiedName(methodBase);

            PhysicalLocation physicalLocation;
            
            if (fileName != null)
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

            StackFrame stackFrame = new StackFrame()
            {
                FullyQualifiedLogicalName = fullyQualifiedName,
                Location = physicalLocation,
                Module = assembly?.GetName().Name,                 
            };

            if (nativeOffset != -1)
            {
                stackFrame.Properties = new Dictionary<string, string>
                {
                    { "NativeOffset", nativeOffset.ToString(CultureInfo.InvariantCulture) }
                };
            }

            return stackFrame;
        }

        public override string ToString()
        {
            string result = AT + this.FullyQualifiedLogicalName;

            if (Location?.Uri != null)
            {
                string lineNumber = Location.Region.StartLine.ToString(CultureInfo.InvariantCulture);
                string fileName = Location.Uri.LocalPath;

                result += IN + fileName + LINE + " " + lineNumber;
            }

            return result;
        }

        public static string CreateFullyQualifiedName(MethodBase methodBase)
        {
            if (methodBase == null) { return null;  }

            var sb = new StringBuilder();

            Type t = methodBase.DeclaringType;
            // if there is a type (non global method) print it
            if (t != null)
            {
                sb.Append(t.FullName.Replace('+', '.'));
                sb.Append(".");
            }
            sb.Append(methodBase.Name);

            // deal with the generic portion of the method
            if (methodBase is MethodInfo && ((MethodInfo)methodBase).IsGenericMethod)
            {
                Type[] typars = ((MethodInfo)methodBase).GetGenericArguments();
                sb.Append("[");
                int k = 0;
                bool fFirstTyParam = true;
                while (k < typars.Length)
                {
                    if (fFirstTyParam == false)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        fFirstTyParam = false;
                    }

                    sb.Append(typars[k].Name);
                    k++;
                }
                sb.Append("]");
            }

            // arguments printing
            sb.Append("(");
            ParameterInfo[] pi = methodBase.GetParameters();
            bool fFirstParam = true;
            for (int j = 0; j < pi.Length; j++)
            {
                if (fFirstParam == false)
                {
                    sb.Append(", ");
                }
                else
                {
                    fFirstParam = false;
                }

                String typeName = "<UnknownType>";
                if (pi[j].ParameterType != null)
                {
                    typeName = pi[j].ParameterType.Name;
                }
                sb.Append(typeName + " " + pi[j].Name);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
