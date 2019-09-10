using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class SarifLog
    {
        /// <summary>
        ///  Load a Sarif file into a SarifLog object model instance using deferred loading.
        ///  [Less memory use, but slower; safe for large Sarif]
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog LoadDeferred(string sarifFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new SarifDeferredContractResolver();

            using (JsonPositionedTextReader jtr = new JsonPositionedTextReader(sarifFilePath))
            {
                return serializer.Deserialize<SarifLog>(jtr);
            }
        }

        /// <summary>
        ///  Load a Sarif file into a SarifLog object model instance.
        ///  [File is fully loaded; more RAM but faster]
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog Load(string sarifFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (JsonTextReader jtr = new JsonTextReader(File.OpenText(sarifFilePath)))
            {
                return serializer.Deserialize<SarifLog>(jtr);
            }
        }
    }
}
