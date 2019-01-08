﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class JsonTests
    {
        protected static readonly Run DefaultRun = new Run();
        protected static readonly Tool DefaultTool = new Tool() { Name = "DefaultTool" };
        protected static readonly Result DefaultResult = new Result { Message = new Message { Text = "Some testing occurred." } };

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
    }
}