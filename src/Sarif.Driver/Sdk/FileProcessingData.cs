// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class FileProcessingData
    {
        public string InputFilePath { get; set; }

        public string OutputFilePath { get; set; }

        public SarifLog SarifLog { get; set; }
    }
}
