// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public static class EntryPointUtilities
    {
        public static string[] GenerateArguments(
            string[] args,
            IFileSystem fileSystem,
            IEnvironmentVariables environmentVariables)
        {
            List<string> expandedArguments = new List<string>();

            foreach (string argument in args)
            {

                if (!IsResponseFileArgument(argument))
                {
                    expandedArguments.Add(argument);
                    continue;
                }

                string responseFile = argument.Trim('"').Substring(1);

                responseFile = environmentVariables.ExpandEnvironmentVariables(responseFile);
                responseFile = fileSystem.GetFullPath(responseFile);

                string[] responseFileLines = fileSystem.ReadAllLines(responseFile);

                ExpandResponseFile(responseFileLines, expandedArguments);
            }

            return expandedArguments.ToArray();
        }

        private static bool IsResponseFileArgument(string argument)
        {
            return argument[0] == '@' && argument.Length > 1;
        }

        private static void ExpandResponseFile(string[] responseFileLines, List<string> expandedArguments)
        {
            foreach (string responseFileLine in responseFileLines)
            {
                int argumentCount;
                IntPtr pointer;

                pointer = CommandLineToArgvW(responseFileLine.Trim(), out argumentCount);

                if (pointer == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Could not parse response file line:" + responseFileLine);
                }

                try
                {
                    // Copy each of these strings into our split argument array.
                    for (int i = 0; i < argumentCount; i++)
                    {
                        expandedArguments.Add(Marshal.PtrToStringUni(Marshal.ReadIntPtr(pointer, i * IntPtr.Size)));
                    }

                }
                finally
                {
                    LocalFree(pointer);
                }
            }
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);
    }
}
