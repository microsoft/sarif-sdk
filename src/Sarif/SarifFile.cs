// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A static holder class analagous to System.IO.File
    /// that provide helpers for and writing SARIF files.
    /// </summary>
    public static class SarifFile
    {
        public static ResultLog Open(string sarifLogFilePath)
        {
            string logText = File.ReadAllText(sarifLogFilePath);

            return CreateFromText(logText);
        }

        public static ResultLog CreateFromText(string sarifText)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
            };

            return JsonConvert.DeserializeObject<ResultLog>(sarifText, settings);
        }

        public const string OpenFileTitle = "Open Static Analysis Results Interchange Format (SARIF) file";
        public const string OpenFileFilter = "SARIF log files (*.sarif;*.sarif.json)|*.sarif;*.sarif.json";
    }
}
