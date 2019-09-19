using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class SarifLog
    {
        /// <summary>
        ///  Load a SARIF file into a SarifLog object model instance using deferred loading.
        ///  [Less memory use, but slower; safe for large Sarif]
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog LoadDeferred(string sarifFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new SarifDeferredContractResolver();

            // NOTE: Deferred reading needs JsonPositionedTextReader to identify where deferred collection items are
            // (to deserialize them 'just in time' later) and needs a 'streamProvider' function (how to open the file again)
            // so that deferred collections know how to open the file again to seek to and read elements 'just in time'

            using (JsonPositionedTextReader jtr = new JsonPositionedTextReader(sarifFilePath))
            {
                // NOTE: Load with JsonSerializer.Deserialize, not JsonConvert.DeserializeObject, to avoid a string of the whole file in memory.
                return serializer.Deserialize<SarifLog>(jtr);
            }
        }

        /// <summary>
        ///  Load a SARIF file into a SarifLog object model instance.
        ///  [File is fully loaded; more RAM but faster]
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog Load(string sarifFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (JsonTextReader jtr = new JsonTextReader(File.OpenText(sarifFilePath)))
            {
                // NOTE: Load with JsonSerializer.Deserialize, not JsonConvert.DeserializeObject, to avoid a string of the whole file in memory.
                return serializer.Deserialize<SarifLog>(jtr);
            }
        }
    }
}
