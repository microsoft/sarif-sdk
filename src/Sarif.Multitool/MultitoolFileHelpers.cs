// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Helper functions for the multitool.
    /// </summary>
    static class MultitoolFileHelpers
    {
        public static SarifLog ReadSarifFile(string filePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string logText = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<SarifLog>(logText, settings);
        }

        public static void WriteSarifFile(SarifLog sarifFile, string outputName, Formatting formatting)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = formatting
            };

            File.WriteAllText(outputName, JsonConvert.SerializeObject(sarifFile, settings));
        }
    }
}
