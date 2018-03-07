using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
