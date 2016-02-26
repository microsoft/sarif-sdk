// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class SarifLoggerTests
    {
        [TestMethod]
        public void SarifLogger_RedactedCommandLine()
        {
            var sb = new StringBuilder();

            // Sample test execution command-line. We will redact the 'TestExecution' role data
            //
            // "C:\PROGRAM FILES (X86)\MICROSOFT VISUAL STUDIO 14.0\COMMON7\IDE\COMMONEXTENSIONS\MICROSOFT\TESTWINDOW\te.processhost.managed.exe"
            // /role=TestExecution /wexcommunication_connectionid=2B1B7D58-C573-45E8-8968-ED321963F0F6
            // /stackframecount=50 /wexcommunication_protocol=ncalrpc
            using (var textWriter = new StringWriter(sb))
            {
                string pathToExe = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

                // The calling assembly lives in an \Extensions directory that hangs off
                // the directory of the test driver (the location of which we can't retrieve
                // from Assembly.GetEntryAssembly() as we are running in an AppDomain).
                pathToExe = pathToExe.Substring(0, pathToExe.Length - @"\Extensions".Length);

                string[] tokensToRedact = { "TestExecution", pathToExe };

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    verbose: false,
                    analysisTargets: null,
                    computeTargetsHash: false,
                    prereleaseInfo: null,
                    invocationInfoTokensToRedact: tokensToRedact,
                    runInfo: null,
                    toolInfo: null)) { }

                string result = sb.ToString();
                result.Split(new string[] { SarifConstants.RemovedMarker }, StringSplitOptions.None)
                    .Length.Should().Be(tokensToRedact.Length + 1, "redacting n tokens gives you n+1 removal markers");
            }
        }
    }
}
