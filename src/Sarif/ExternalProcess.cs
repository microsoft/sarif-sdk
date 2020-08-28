// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ExternalProcess
    {
        public IConsoleCapture StdOut { get; private set; }

        public IConsoleCapture StdErr { get; private set; }

        public ExternalProcess(
            string workingDirectory,
            string exePath,
            string arguments,
            IConsoleCapture stdOut = null,
            int[] acceptableReturnCodes = null)
        {
            acceptableReturnCodes = acceptableReturnCodes ?? new int[] { 0 };

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = exePath;
            psi.Arguments = arguments;
            psi.WorkingDirectory = workingDirectory;

            psi.ErrorDialog = false;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            using (Process process = Process.Start(psi))
            {
                StdErr = new ConsoleStreamCapture();
                StdOut = stdOut ?? new ConsoleStreamCapture();

                var tasks = new Task<string>[2];

                tasks[0] = StdOut.Capture(process.StandardOutput, CancellationToken.None);
                tasks[1] = StdErr.Capture(process.StandardError, CancellationToken.None);

                process.WaitForExit();

                Task.WaitAll(tasks);

                if (!acceptableReturnCodes.Contains(process.ExitCode))
                {
                    Console.WriteLine("Command execution FAILED.");
                    Console.WriteLine();
                    Console.WriteLine($"Command-line     : {psi.FileName} {psi.Arguments}");
                    Console.WriteLine($"Working directory: {Environment.CurrentDirectory}");
                    Console.WriteLine();
                    Console.WriteLine(StdErr.Text);
                    Console.WriteLine();
                    throw new InvalidOperationException();
                }
            }
        }
    }
}