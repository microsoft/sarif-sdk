// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// A SARIF logger that works by generating a SARIF v2 log file which is transformed into
    /// SARIF v1 when the instance is disposed. The file location used to produced the preliminary
    /// v2 content is overwritten in place to produce the transformed v1 file.
    /// </summary>
    public class SarifOneZeroZeroLogger : SarifLogger
    {
        private readonly string _outputFilePath;

        public SarifOneZeroZeroLogger(
            string outputFilePath,
            FilePersistenceOptions logFilePersistenceOptions = SarifLogger.DefaultLogFilePersistenceOptions,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null,
            IEnumerable<FailureLevel> levels = null,
            IEnumerable<ResultKind> kinds = null,
            IEnumerable<string> insertProperties = null)
            : base(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  logFilePersistenceOptions: logFilePersistenceOptions,
                  dataToInsert: dataToInsert,
                  dataToRemove: dataToRemove,
                  defaultFileEncoding: defaultFileEncoding,
                  run: run,
                  analysisTargets: analysisTargets,
                  invocationTokensToRedact: invocationTokensToRedact,
                  invocationPropertiesToLog: invocationPropertiesToLog,
                  levels: levels,
                  kinds: kinds,
                  insertProperties: insertProperties)
        {
            _outputFilePath = outputFilePath;
        }

        public override void Dispose()
        {
            base.Dispose();

            string logText = File.ReadAllText(_outputFilePath);

            SarifLog v2Log = JsonConvert.DeserializeObject<SarifLog>(logText);

            var transformer = new SarifCurrentToVersionOneVisitor();
            transformer.VisitSarifLog(v2Log);

            JsonSerializerSettings v1Settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolverVersionOne.Instance,
                Formatting = PrettyPrint ? Formatting.Indented : Formatting.None
            };

            File.WriteAllText(_outputFilePath, JsonConvert.SerializeObject(transformer.SarifLogVersionOne, v1Settings));
        }
    }
}
