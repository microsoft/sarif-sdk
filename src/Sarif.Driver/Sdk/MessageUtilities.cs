// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class MessageUtilities
    {
        public static string BuildMessage(IAnalysisContext context, string messageFormatString, params string[] arguments)
        {
            // By convention, the first argument is always the target name, 
            // which we retrieve from the context
            Debug.Assert(File.Exists(context.TargetUri.LocalPath));
            string targetName = Path.GetFileName(context.TargetUri.LocalPath);

            string[] fullArguments = new string[arguments != null ? arguments.Length + 1 : 1];
            fullArguments[0] = targetName;

            if (fullArguments.Length > 1)
            {
                arguments.CopyTo(fullArguments, 1);
            }

            return String.Format(CultureInfo.InvariantCulture,
                messageFormatString, fullArguments);
        }


        public static string BuildTargetNotAnalyzedMessage(string targetPath, string ruleName, string reason)
        {
            targetPath = Path.GetFileName(targetPath);

            // Image '{0}' was not evaluated for check '{1}' as the analysis
            // is not relevant based on observed metadata: {2}
            return String.Format(
                CultureInfo.InvariantCulture,
                SdkResources.TargetNotAnalyzed_NotApplicable,
                targetPath,
                ruleName,
                reason);
        }

        public static string BuildRuleDisabledDueToMissingPolicyMessage(string ruleName, string reason)
        {
            // BinSkim command-line using the --policy argument (recommended), or 
            // pass --defaultPolicy to invoke built-in settings. Invoke the 
            // BinSkim.exe 'export' command to produce an initial policy file 
            // that can be edited if required and passed back into the tool.
            return String.Format(
                CultureInfo.InvariantCulture,
                SdkResources.RuleWasDisabledDueToMissingPolicy,
                ruleName,
                reason);
        }
    }
}
