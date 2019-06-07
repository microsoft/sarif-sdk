// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class VisualStudioBuildLogConverter : ToolFileConverterBase
    {
        public override string ToolName => ToolFormat.VisualStudioBuildLog;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            IList<Result> results = GetResults(input);

            PersistResults(output, results);
        }

        private static IList<Result> GetResults(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                return GetResults(reader);
            }
        }

        private static IList<Result> GetResults(TextReader reader)
        {
            IList<Result> results = new List<Result>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Result result = GetResultFromLine(line);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        private static Result GetResultFromLine(string line)
        {
            return null;
        }
    }
}
