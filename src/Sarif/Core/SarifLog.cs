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

            using (JsonPositionedTextReader jptr = new JsonPositionedTextReader(sarifFilePath))
            {
                return serializer.Deserialize<SarifLog>(jptr);
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
            using (Stream stream = File.OpenRead(sarifFilePath))
            {
                return Load(stream);
            }
        }

        /// <summary>
        ///  Load a SARIF stream into a SarifLog object model instance.
        ///  [File is fully loaded; more RAM but faster]
        /// </summary>
        /// <param name="source">Stream with SARIF to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog Load(Stream source, bool deferred = false)
        {
            JsonSerializer serializer = new JsonSerializer();

            if (deferred)
            {
                serializer.ContractResolver = new SarifDeferredContractResolver();
            }

            using (StreamReader sr = new StreamReader(source))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                // NOTE: Load with JsonSerializer.Deserialize, not JsonConvert.DeserializeObject, to avoid a string of the whole file in memory.
                return serializer.Deserialize<SarifLog>(jtr);
            }
        }

        /// <summary>
        ///  Write a SARIF log to disk as a file.
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to write to</param>
        public void Save(string sarifFilePath)
        {
            using (FileStream stream = File.Create(sarifFilePath))
            {
                this.Save(stream);
            }
        }

        /// <summary>
        ///  Write a SARIF log to a destination stream.
        /// </summary>
        /// <param name="streamWriter">Stream to write SARIF to</param>
        public void Save(Stream stream)
        {
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                this.Save(streamWriter);
            }
        }

        /// <summary>
        ///  Write a SARIF log to a destination StreamWriter.
        /// </summary>
        /// <param name="streamWriter">StreamWriter to write SARIF to</param>
        public void Save(StreamWriter streamWriter)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}
