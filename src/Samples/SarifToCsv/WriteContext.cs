// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AzureDevOpsCrawlers.Common.IO;

using Microsoft.CodeAnalysis.Sarif;

namespace SarifToCsv
{
    /// <summary>
    ///  WriteContext contains the properties passed from SarifToCsv to the individual column writers.
    /// </summary>
    public class WriteContext
    {
        public CsvWriter Writer;
        public Run Run;
        public Result Result;
        public PhysicalLocation PLoc;

        public int RunIndex;
        public int ResultIndex;
    }
}
