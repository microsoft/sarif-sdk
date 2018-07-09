// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public static class MultitoolExtensionMethods
    {
        public static LoggingOptions ConvertToLoggingOptions(this MultitoolOptionsBase multitoolOptions)
        {
            LoggingOptions loggingOptions = LoggingOptions.None;

            if (multitoolOptions.ComputeFileHashes) { loggingOptions |= LoggingOptions.ComputeFileHashes; }
            if (multitoolOptions.PersistBinaryContents) { loggingOptions |= LoggingOptions.PersistBinaryContents; }
            if (multitoolOptions.PersistTextFileContents) { loggingOptions |= LoggingOptions.PersistTextFileContents; }

            return loggingOptions;
        }
    }
}
