// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class SarifLoggerTests : JsonTests
    {
        [TestMethod]
        public void SarifLogger_RedactedCommandLine()
        {
            var sb = new StringBuilder();

            // Sample test execution command-line from within VS. We will redact the 'TestExecution' role data
            //
            // "C:\PROGRAM FILES (X86)\MICROSOFT VISUAL STUDIO 14.0\COMMON7\IDE\COMMONEXTENSIONS\MICROSOFT\TESTWINDOW\te.processhost.managed.exe"
            // /role=TestExecution /wexcommunication_connectionid=2B1B7D58-C573-45E8-8968-ED321963F0F6
            // /stackframecount=50 /wexcommunication_protocol=ncalrpc
            //
            // Sample test execution from command-line when running test script. Will redact hostProcessId
            //
            // "C:\Program Files (x86\\Microsoft Visual Studio 14.0\Common7\IDE\QTAgent32_40.exe\" 
            // /agentKey a144e450-ac06-46d0-8365-c21ea7872d23 /hostProcessId 8024 /hostIpcPortName 
            // eqt -60284c64-6bc1-3ecc-fb5f-a484bb1a2475"

            using (var textWriter = new StringWriter(sb))
            {
                string[] tokensToRedact = new string[] { };
                string pathToExe = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

                if (pathToExe.IndexOf(@"\Extensions", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    // The calling assembly lives in an \Extensions directory that hangs off
                    // the directory of the test driver (the location of which we can't retrieve
                    // from Assembly.GetEntryAssembly() as we are running in an AppDomain).
                    pathToExe = pathToExe.Substring(0, pathToExe.Length - @"\Extensions".Length);
                    tokensToRedact = new string[] { "TestExecution", pathToExe };
                }
                else
                {
                    string argumentToRedact = Environment.CommandLine;
                    argumentToRedact = argumentToRedact.Split(new string[] { @"/agentKey" }, StringSplitOptions.None)[1].Trim();
                    argumentToRedact = argumentToRedact.Split(' ')[0];
                    tokensToRedact = new string[] { argumentToRedact };
                }

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    verbose: false,
                    computeTargetsHash: false,
                    logEnvironment: false,
                    prereleaseInfo: null,
                    invocationTokensToRedact: tokensToRedact)) { }

                string result = sb.ToString();
                result.Split(new string[] { SarifConstants.RemovedMarker }, StringSplitOptions.None)
                    .Length.Should().Be(tokensToRedact.Length + 1, "redacting n tokens gives you n+1 removal markers");
            }
        }

        [TestMethod]
        public void SarifLogger_WritesFileData()
        {
            var sb = new StringBuilder();
            string file;

            using (var tempFile = new TempFile(".cpp"))
            using (var textWriter = new StringWriter(sb))            
            {
                file = tempFile.Name;
                File.WriteAllText(file, "#include \"windows.h\";");

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: new string[] { file },
                    verbose: false,
                    computeTargetsHash: true,
                    logEnvironment: false,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null)) { }
            }

            string result = sb.ToString();

            string fileDataKey = new Uri(file).ToString();

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(result);
            sarifLog.Runs[0].Files[fileDataKey][0].MimeType.Should().Be(MimeType.Cpp);
            sarifLog.Runs[0].Files[fileDataKey][0].Hashes[0].Algorithm.Should().Be(AlgorithmKind.MD5);
            sarifLog.Runs[0].Files[fileDataKey][0].Hashes[0].Value.Should().Be("4B9DC12934390387862CC4AB5E4A2159");
            sarifLog.Runs[0].Files[fileDataKey][0].Hashes[1].Algorithm.Should().Be(AlgorithmKind.Sha1);
            sarifLog.Runs[0].Files[fileDataKey][0].Hashes[1].Value.Should().Be("9B59B1C1E3F5F7013B10F6C6B7436293685BAACE");
            sarifLog.Runs[0].Files[fileDataKey][0].Hashes[2].Algorithm.Should().Be(AlgorithmKind.Sha256);
            sarifLog.Runs[0].Files[fileDataKey][0].Hashes[2].Value.Should().Be("0953D7B3ADA7FED683680D2107EE517A9DBEC2D0AF7594A91F058D104B7A2AEB");
        }
    }
}
