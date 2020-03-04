// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class MessageUtilities
    {
        public static string BuildMessage(IAnalysisContext context, string messageFormat, params string[] arguments)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            // By convention, the first argument is always the target name, 
            // which we retrieve from the context
            Debug.Assert(File.Exists(context.TargetUri.LocalPath));
            string targetName = context.TargetUri.GetFileName();

            string[] fullArguments = new string[arguments != null ? arguments.Length + 1 : 1];
            fullArguments[0] = targetName;

            if (fullArguments.Length > 1)
            {
                arguments.CopyTo(fullArguments, 1);
            }

            return string.Format(CultureInfo.InvariantCulture,
                messageFormat, fullArguments);
        }

        public static string BuildRuleDisabledDueToMissingPolicyMessage(string ruleName, string reason)
        {
            // BinSkim command-line using the --policy argument (recommended), or 
            // pass --defaultPolicy to invoke built-in settings. Invoke the 
            // BinSkim.exe 'export' command to produce an initial policy file 
            // that can be edited if required and passed back into the tool.
            return string.Format(
                CultureInfo.InvariantCulture,
                SdkResources.ERR997_MissingReportingConfiguration,
                ruleName,
                reason);
        }
    }
}
