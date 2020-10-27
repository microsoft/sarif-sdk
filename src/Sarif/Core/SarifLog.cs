﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public enum SarifFormat
    {
        BSOA,
        JSON,
        IndentedJSON,
    }

    public partial class SarifLog
    {
        partial void Init()
        {
            Version = SarifVersion.Current;
            SchemaUri = new Uri(SarifUtilities.SarifSchemaUri);
        }

        public override string ToString()
        {
            return $"{Runs.Sum((run) => run?.Results?.Count ?? 0):n0} {nameof(Result)}s";
        }

        public static SarifFormat FormatForFileName(string filePath)
        {
            return (Path.GetExtension(filePath).ToLowerInvariant() == ".bsoa" ? SarifFormat.BSOA : SarifFormat.JSON);
        }

        /// <summary>
        ///  Load a SARIF file into a SarifLog object model instance using deferred loading.
        ///  [Less memory use, but slower; safe for large Sarif]
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog LoadDeferred(string sarifFilePath)
        {
            // Not supported (yet) with BSOA; fall back to normal load
            return Load(sarifFilePath);
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
                return Load(stream, FormatForFileName(sarifFilePath));
            }
        }

        public static SarifLog Load(Stream stream, string fileName)
        {
            return Load(stream, FormatForFileName(fileName));
        }

        /// <summary>
        ///  Load a SARIF stream into a SarifLog object model instance.
        ///  [File is fully loaded; more RAM but faster]
        /// </summary>
        /// <param name="source">Stream with SARIF to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog Load(Stream source, SarifFormat format = SarifFormat.JSON)
        {
            if (format == SarifFormat.BSOA)
            {
                return SarifLog.ReadBsoa(source);
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();

                using (StreamReader sr = new StreamReader(source))
                using (JsonTextReader jtr = new JsonTextReader(sr))
                {
                    // NOTE: Load with JsonSerializer.Deserialize, not JsonConvert.DeserializeObject, to avoid a string of the whole file in memory.
                    return serializer.Deserialize<SarifLog>(jtr);
                }
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
                this.Save(stream, FormatForFileName(sarifFilePath));
            }
        }

        /// <summary>
        ///  Write a SARIF log to a destination stream.
        /// </summary>
        /// <param name="stream">Stream to write SARIF to</param>
        public void Save(Stream stream, SarifFormat format = SarifFormat.JSON)
        {
            if (format == SarifFormat.BSOA)
            {
                this.WriteBsoa(stream);
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = (format == SarifFormat.IndentedJSON ? Formatting.Indented : Formatting.None);

                using (StreamWriter sw = new StreamWriter(stream))
                using (JsonTextWriter jtw = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jtw, this);
                }
            }
        }

        /// <summary>
        /// Applies the policies contained in each run
        /// </summary>
        public void ApplyPolicies()
        {
            if (this.Runs == null)
            {
                return;
            }

            foreach (Run run in this.Runs)
            {
                run.ApplyPolicies();
            }
        }
    }
}
