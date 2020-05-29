// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class JsonTests
    {
        protected static readonly Run DefaultRun = new Run();
        protected static readonly Tool DefaultTool = new Tool() { Driver = new ToolComponent { Name = "DefaultTool" } };
        protected static readonly Result DefaultResult = new Result { Message = new Message { Text = "Some testing occurred." } };

        protected static readonly ResourceExtractor ResourceExtractor = new ResourceExtractor(typeof(JsonTests));

        protected static string GetJson(Action<ResultLogJsonWriter> testContent)
        {
            StringBuilder result = new StringBuilder();
            using (var str = new StringWriter(result))
            using (var json = new JsonTextWriter(str) { Formatting = Formatting.Indented, DateTimeZoneHandling = DateTimeZoneHandling.Utc })
            using (var uut = new ResultLogJsonWriter(json))
            {
                testContent(uut);
            }

            return result.ToString();
        }

        protected static SarifLog CreateCurrentV2SarifLog(int resultCount = 0, bool createResultsTag = false)
        {
            var sarifLog = new SarifLog
            {
                Runs = new List<Run>
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "DefaultTool"
                            }
                        },
                    }
                }
            };

            if (createResultsTag || resultCount > 0)
            {
                sarifLog.Runs[0].Results = new List<Result>();
            }

            for (int i = 0; i < resultCount; i++)
            {
                var result = new Result
                {
                    Message = new Message
                    {
                        Text = "Some testing occurred."
                    },
                };
                sarifLog.Runs[0].Results.Add(result);
            }

            return sarifLog;
        }

        protected static string CreateCurrentV2SarifLogText(int resultCount = 0, Action<SarifLog> sarifLogAction = null, bool createResultsTag = false)
        {
            SarifLog sarifLog = CreateCurrentV2SarifLog(resultCount, createResultsTag);

            sarifLogAction?.Invoke(sarifLog);

            return JsonConvert.SerializeObject(sarifLog, Formatting.Indented);
        }
    }
}