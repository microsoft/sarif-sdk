// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class CommandUtilities
    {
        internal static string GetTransformedOutputFileName(SingleFileOptionsBase options)
        {
            string filePath = Path.GetFullPath(options.InputFilePath);

            if (options.Inline)
            {
                return filePath;
            }

            if (!string.IsNullOrEmpty(options.OutputFilePath))
            {
                return options.OutputFilePath;
            }

            const string TransformedExtension = ".transformed.sarif";
            string extension = Path.GetExtension(filePath);

            // For an input file named MyFile.sarif, returns MyFile.transformed.sarif.
            if (extension.Equals(SarifConstants.SarifFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + TransformedExtension);
            }

            // For an input file named MyFile.json, return MyFile.json.transformed.sarif.
            return Path.GetFullPath(options.InputFilePath + TransformedExtension);
        }
    }
}
