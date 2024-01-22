// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            var serializer = new JsonSerializer();
            serializer.ContractResolver = new SarifDeferredContractResolver();

            using var jsonPositionedTextReader = new JsonPositionedTextReader(sarifFilePath);
            return serializer.Deserialize<SarifLog>(jsonPositionedTextReader);
        }

        /// <summary>
        ///  Load a SARIF file into a SarifLog object model instance.
        ///  [File is fully loaded; more RAM but faster]
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog Load(string sarifFilePath)
        {
            using Stream stream = File.OpenRead(sarifFilePath);
            return Load(stream);
        }

        /// <summary>
        ///  Load a SARIF stream into a SarifLog object model instance.
        ///  [File is fully loaded; more RAM but faster]
        /// </summary>
        /// <param name="source">Stream with SARIF to load</param>
        /// <returns>SarifLog instance for file</returns>
        public static SarifLog Load(Stream source, bool deferred = false)
        {
            var serializer = new JsonSerializer();

            if (deferred)
            {
                serializer.ContractResolver = new SarifDeferredContractResolver();
                var jsonPositionedTextReader = JsonPositionedTextReader.FromStream(Stream.Synchronized(source));
                return serializer.Deserialize<SarifLog>(jsonPositionedTextReader);
            }

            using var streamReader = new StreamReader(source);
            using var jsonTextReader = new JsonTextReader(streamReader);
            // NOTE: Load with JsonSerializer.Deserialize, not JsonConvert.DeserializeObject, to avoid a string of the whole file in memory.
            return serializer.Deserialize<SarifLog>(jsonTextReader);
        }

        /// <summary>
        /// Post the SARIF log file to a URI.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if filePath does not exist.</exception>
        /// <exception cref="ArgumentNullException">Thrown if postUri/filePath/stream/httpClient is null.</exception>
        /// <exception cref="HttpRequestException">Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.</exception>
        /// <param name="postUri"></param>
        /// <param name="filePath"></param>
        /// <param name="fileSystem"></param>
        /// <param name="httpClient"></param>
        /// <returns>If the SarifLog has been posted.</returns>
        public static async Task<bool> Post(Uri postUri,
                                      string filePath,
                                      IFileSystem fileSystem,
                                      HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!fileSystem.FileExists(filePath))
            {
                throw new ArgumentException($"File path does not exist: '{filePath}'", nameof(filePath));
            }

            byte[] fileBytes = fileSystem.FileReadAllBytes(filePath);
            SarifLog sarifLog = Load(new MemoryStream(fileBytes));

            if (!ShouldSendLog(sarifLog))
            {
                Console.WriteLine($"Post of log file to {postUri} was skipped because the log contains no results.");
                return false;
            }

            await Post(postUri, new MemoryStream(fileBytes), httpClient);
            Console.WriteLine($"Posted log file successfully to: {postUri}");
            return true;
        }

        /// <summary>
        /// Post the SARIF stream to a URI.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if postUri/stream/httpClient is null.</exception>
        /// <exception cref="HttpRequestException">Throws an exception if the IsSuccessStatusCode property for the HTTP response is false.</exception>
        /// <param name="postUri"></param>
        /// <param name="stream"></param>
        /// <param name="httpClient"></param>
        public static async Task<HttpResponseMessage> Post(Uri postUri, Stream stream, HttpClient httpClient)
        {
            if (postUri == null)
            {
                throw new ArgumentNullException(nameof(postUri));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using var streamContent = new StreamContent(stream);
            return await httpClient.PostAsync(postUri, streamContent);
        }

        /// <summary>
        ///  Write a SARIF log to disk as a file.
        /// </summary>
        /// <param name="sarifFilePath">File Path to Sarif file to write to</param>
        public void Save(string sarifFilePath)
        {
            using FileStream stream = File.Create(sarifFilePath);
            this.Save(stream);
        }

        /// <summary>
        ///  Write a SARIF log to a destination stream.
        /// </summary>
        /// <param name="stream">Stream to write SARIF to</param>
        public void Save(Stream stream)
        {
            // These are defaults takens from legacy mscorlib reference source.
            // https://referencesource.microsoft.com/#mscorlib/system/io/streamwriter.cs,62bd8ad495f57b21
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
            this.Save(streamWriter);
        }

        /// <summary>
        ///  Write a SARIF log to a destination StreamWriter.
        /// </summary>
        /// <param name="streamWriter">StreamWriter to write SARIF to</param>
        public void Save(StreamWriter streamWriter)
        {
            var serializer = new JsonSerializer();

            var writer = new JsonTextWriter(streamWriter);
            serializer.Serialize(writer, this);
            writer.Flush();
        }

        /// <summary>
        /// Enumerate all results from all runs in the SARIF log.
        /// </summary>
        /// <returns>A Result enumerator for the SARIF log.</returns>
        public IEnumerable<Result> Results()
        {
            if (this.Runs?.Count > 0 == false) { yield break; }

            foreach (Run run in this.Runs)
            {
                if (run.Results?.Count > 0 == false) { continue; }

                foreach (Result result in run.Results)
                {
                    result.Run = run;
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Applies the policies contained in each run
        /// </summary>
        public void ApplyPolicies(Dictionary<string, FailureLevel> policiesCache = null)
        {
            if (this.Runs == null)
            {
                return;
            }

            foreach (Run run in this.Runs)
            {
                run.ApplyPolicies(policiesCache);
            }
        }

        /// <summary>
        /// Applies the policies contained from a sarif file
        /// </summary>
        public void ApplyPolicies(string sarifLogPath)
        {
            SarifLog sarifLog = Load(sarifLogPath);
            if (sarifLog == null || sarifLog.Runs == null)
            {
                return;
            }

            var localCache = new Dictionary<string, FailureLevel>();

            foreach (Run run in sarifLog.Runs)
            {
                ComputePolicies(localCache, run);
            }

            ApplyPolicies(localCache);
        }

        internal static void ComputePolicies(Dictionary<string, FailureLevel> localCache, Run run)
        {
            // checking if we have policies
            if (run.Policies == null || run.Policies.Count == 0)
            {
                return;
            }

            foreach (ToolComponent policy in run.Policies)
            {
                foreach (ReportingDescriptor rule in policy.Rules)
                {
                    localCache[rule.Id] = rule.DefaultConfiguration.Level;
                }
            }
        }

        internal static bool ShouldSendLog(SarifLog sarifLog)
        {
            if (sarifLog?.Runs?.Count > 0)
            {
                foreach (Run run in sarifLog.Runs)
                {
                    if (run.Results?.Count > 0)
                    {
                        return true;
                    }

                    if (run.Invocations?.Count > 0)
                    {
                        foreach (Invocation invocation in run.Invocations)
                        {
                            if (invocation.ToolExecutionNotifications?.Count > 0)
                            {
                                foreach (Notification notification in invocation.ToolExecutionNotifications)
                                {
                                    if (notification.Level == FailureLevel.Error)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
