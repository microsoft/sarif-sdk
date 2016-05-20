// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Reflection;
using System.Text;

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
            int ilOffset = dotNetStackFrame.GetILOffset();
            string fileName = dotNetStackFrame.GetFileName();
            int nativeOffset = dotNetStackFrame.GetNativeOffset();
            MethodBase methodBase = dotNetStackFrame.GetMethod();
            Assembly assembly = methodBase?.DeclaringType.Assembly;
            string fullyQualifiedName = CreateFullyQualifiedName(methodBase);

            StackFrame stackFrame = new StackFrame
            {
                Module = assembly?.GetName().Name,
                FullyQualifiedLogicalName = fullyQualifiedName
            };

            if (fileName != null)
            {
                stackFrame.Uri = new Uri(fileName);
                stackFrame.Line = dotNetStackFrame.GetFileLineNumber();
                stackFrame.Column = dotNetStackFrame.GetFileColumnNumber();
            }

            if (ilOffset != -1)
            {
                stackFrame.Offset = ilOffset;
            }

            if (nativeOffset != -1)
            {
                stackFrame.SetProperty("NativeOffset", nativeOffset.ToString(CultureInfo.InvariantCulture));
            }

            return stackFrame;
        }

        public override string ToString()
        {
            string result = AT + this.FullyQualifiedLogicalName;

            if (this.Uri != null)
            {
                string lineNumber = this.Line.ToString(CultureInfo.InvariantCulture);
                string fileName = this.Uri.LocalPath;

                result += IN + fileName + LINE + " " + lineNumber;
            }

            return result;
        }

        private static string CreateFullyQualifiedName(MethodBase methodBase)
        {
            if (methodBase == null) { return null;  }

            var sb = new StringBuilder();

            Type type = methodBase.DeclaringType;
            // if there is a type (non global method) print it
            if (type != null)
            {
                sb.Append(type.FullName.Replace('+', '.'));
                sb.Append(".");
            }
            sb.Append(methodBase.Name);

            // deal with the generic portion of the method
            if (methodBase is MethodInfo && ((MethodInfo)methodBase).IsGenericMethod)
            {
                Type[] typeArguments = ((MethodInfo)methodBase).GetGenericArguments();
                sb.Append("[");
                int k = 0;
                bool firstTypeParameter = true;
                while (k < typeArguments.Length)
                {
                    if (firstTypeParameter == false)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        firstTypeParameter = false;
                    }

                    sb.Append(typeArguments[k].Name);
                    k++;
                }
                sb.Append("]");
            }

            // arguments printing
            sb.Append("(");
            ParameterInfo[] parameterInfos = methodBase.GetParameters();
            bool firstParameterInfo = true;
            for (int j = 0; j < parameterInfos.Length; j++)
            {
                if (firstParameterInfo == false)
                {
                    sb.Append(", ");
                }
                else
                {
                    firstParameterInfo = false;
                }

                String typeName = "<UnknownType>";
                if (parameterInfos[j].ParameterType != null)
                {
                    typeName = parameterInfos[j].ParameterType.Name;
                }
                sb.Append(typeName + " " + parameterInfos[j].Name);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
