// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
                Location = new Location()
            };

            if (!string.IsNullOrWhiteSpace(fullyQualifiedName))
            {
                stackFrame.Location = new Location
                {
                    LogicalLocation = new LogicalLocation
                    {
                        FullyQualifiedName = fullyQualifiedName
                    }
                };
            }

            if (fileName != null)
            {
                if (stackFrame.Location == null)
                {
                    stackFrame.Location = new Location();
                }

                stackFrame.Location.PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri(fileName)
                    },
                    Region = new Region
                    {
                        StartLine = dotNetStackFrame.GetFileLineNumber(),
                        StartColumn = dotNetStackFrame.GetFileColumnNumber()
                    }
                };
            }

            if (ilOffset != -1)
            {
                if (stackFrame.Location == null)
                {
                    stackFrame.Location = new Location();
                }

                if (stackFrame.Location.PhysicalLocation == null)
                {
                    stackFrame.Location.PhysicalLocation = new PhysicalLocation();
                }

                stackFrame.Location.PhysicalLocation.Address = new Address
                {
                    OffsetFromParent = ilOffset
                };
            }

            if (nativeOffset != -1)
            {
                stackFrame.SetProperty("NativeOffset", nativeOffset.ToString(CultureInfo.InvariantCulture));
            }

            return stackFrame;
        }

        public override string ToString()
        {
            string result = AT + this.Location?.LogicalLocation?.FullyQualifiedName;

            if (this.Location?.PhysicalLocation?.ArtifactLocation?.Uri != null)
            {
                string fileName = this.Location.PhysicalLocation.ArtifactLocation.Uri.LocalPath;
                result += IN + fileName;

                if (this.Location?.PhysicalLocation?.Region != null)
                {
                    string lineNumber = this.Location.PhysicalLocation.Region.StartLine.ToString(CultureInfo.InvariantCulture);
                    result += LINE + " " + lineNumber;
                }
            }

            return result;
        }

        private static string CreateFullyQualifiedName(MethodBase methodBase)
        {
            if (methodBase == null) { return null; }

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

                string typeName = "<UnknownType>";
                if (parameterInfos[j].ParameterType != null)
                {
                    typeName = parameterInfos[j].ParameterType.Name;
                }
                sb.Append(typeName + " " + parameterInfos[j].Name);
            }
            sb.Append(")");

            return sb.ToString();
        }

        public bool ShouldSerializeParameters() { return this.Parameters.HasAtLeastOneNonNullValue(); }
    }
}
