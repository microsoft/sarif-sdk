using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
                pathToExe = pathToExe.Substring(0, pathToExe.Length - @"\Extensions".Length);
                string[] tokensToRedact = { "TestExecution", pathToExe};
                string invocationInfo = Environment.CommandLine;

                var sarifLogger = new SarifLogger(
                    textWriter,
                    verbose: false,
                    analysisTargets: null,
                    computeTargetsHash: false,
                    prereleaseInfo: null,
                    invocationInfoTokensToRedact: tokensToRedact,
                    runInfo: null,
                    toolInfo: null);

                string result = sb.ToString();
                Assert.AreEqual<int>(
                    result.Split(new string[] { SarifConstants.RemovedMarker }, StringSplitOptions.None).Length,
                    3,
                    "Did not observe expected argument redaction.");
            }
        }
    }
}
