// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class MemoryStreamSarifLogger : SarifLogger
    {
        protected StreamWriter writer;
        protected bool disposed;

        public MemoryStreamSarifLogger(FilePersistenceOptions logFilePersistenceOptions = FilePersistenceOptions.PrettyPrint,
                                       OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
                                       OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
                                       Run run = null,
                                       IEnumerable<string> analysisTargets = null,
                                       IEnumerable<string> invocationTokensToRedact = null,
                                       IEnumerable<string> invocationPropertiesToLog = null,
                                       string defaultFileEncoding = null,
                                       FailureLevelSet levels = null,
                                       ResultKindSet kinds = null,
                                       IEnumerable<string> insertProperties = null) : this(new StreamWriter(new MemoryStream()),
                                                                                           logFilePersistenceOptions,
                                                                                           dataToInsert,
                                                                                           dataToRemove,
                                                                                           run,
                                                                                           analysisTargets,
                                                                                           invocationTokensToRedact,
                                                                                           invocationPropertiesToLog,
                                                                                           defaultFileEncoding,
                                                                                           levels,
                                                                                           kinds,
                                                                                           insertProperties)
        {
        }

        public MemoryStreamSarifLogger(StreamWriter writer,
                                       FilePersistenceOptions logFilePersistenceOptions = FilePersistenceOptions.PrettyPrint,
                                       OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
                                       OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
                                       Run run = null,
                                       IEnumerable<string> analysisTargets = null,
                                       IEnumerable<string> invocationTokensToRedact = null,
                                       IEnumerable<string> invocationPropertiesToLog = null,
                                       string defaultFileEncoding = null,
                                       FailureLevelSet levels = null,
                                       ResultKindSet kinds = null,
                                       IEnumerable<string> insertProperties = null) : base(writer,
                                                                                           logFilePersistenceOptions,
                                                                                           dataToInsert,
                                                                                           dataToRemove,
                                                                                           run,
                                                                                           analysisTargets,
                                                                                           invocationTokensToRedact,
                                                                                           invocationPropertiesToLog,
                                                                                           defaultFileEncoding,
                                                                                           closeWriterOnDispose: false,
                                                                                           levels,
                                                                                           kinds,
                                                                                           insertProperties)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }


            this.writer = writer;
            this.writer.AutoFlush = true;
        }

        public SarifLog ToSarifLog()
        {
            if (!this.disposed)
            {
                this.Dispose();
            }

            this.writer.BaseStream.Position = 0;

            SarifLog log = SarifLog.Load(this.writer.BaseStream);
            return log;
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            base.Dispose();
            this.writer.Flush();

            this.disposed = true;
        }
    }
}
