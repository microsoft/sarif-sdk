// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using Microsoft.CodeAnalysis.Sarif.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Sarif.Driver.UnitTests.dll,PublicKey=0024000004800000940000000602000000240000525341310004000001000100433fbf156abe971" +
    "8142bdbd48a440e779a1b708fd21486ee0ae536f4c548edf8a7185c1e3ac89ceef76c15b8cc2497906798779a59402f9b9e27281fb15e7111566cdc9a9f8326301d45320623c52" +
    "22089cf4d0013f365ae729fb0a9c9d15138042825cd511a0f3d4887a7b92f4c2749f81b410813d297b73244cf64995effb1")]
namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintConverter : ToolFileConverterBase
    {
        private readonly ITSLintLoader loader;
        
        public TSLintConverter()
        {
            loader = new TSLintLoader();
        }

        public TSLintConverter(ITSLintLoader loader)
        {
            this.loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }
        
        public override void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));

            output = output ?? throw new ArgumentNullException(nameof(output));

            output.Initialize(id: null, automationId: null);

            TSLintLog tsLintLog = loader.ReadLog(input);

            Tool tool = new Tool
            {
                Name = "TSLint"
            };

            output.WriteTool(tool);

            var results = new List<Result>();
            foreach(TSLintLogEntry entry in tsLintLog)
            {
                results.Add(CreateResult(entry));
            }

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, loggingOptions);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);
            
            if (fileDictionary?.Any() == true)
            {
                output.WriteFiles(fileDictionary);
            }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        internal Result CreateResult(TSLintLogEntry tslEntry)
        {
            tslEntry = tslEntry ?? throw new ArgumentNullException(nameof(tslEntry));

            Result result = new Result()
            {
                RuleId = tslEntry.RuleName,
                Message = tslEntry.Failure
            };

            switch (tslEntry.RuleSeverity)
            {
                case "ERROR":
                    result.Level = ResultLevel.Error;
                    break;
                case "WARN":
                case "WARNING":
                    result.Level = ResultLevel.Warning;
                    break;
                case "DEFAULT":
                    result.Level = ResultLevel.Default;
                    break;
                default:
                    result.Level = ResultLevel.NotApplicable;
                    break;
            }

            result.Locations = new List<Location>();

            Region region = new Region()
            {
                // The TSLint logs have line and column start at 0, Sarif has them starting at 1, so add 1 to each
                StartLine = tslEntry.StartPosition.Line + 1,
                StartColumn = tslEntry.StartPosition.Character + 1,
                EndLine = tslEntry.EndPosition.Line + 1,
                EndColumn = tslEntry.EndPosition.Character + 1,

                Offset = tslEntry.StartPosition.Position
            };

            int length = tslEntry.EndPosition.Position - tslEntry.StartPosition.Position + 1;
            region.Length = length > 0 ? length : 0;

            Uri analysisTargetUri = new Uri(tslEntry.Name, UriKind.Relative);

            PhysicalLocation analysisTarget = new PhysicalLocation(uri: analysisTargetUri, uriBaseId: null, region: region);
            Location location = new Location();
            location.AnalysisTarget = analysisTarget;

            result.Locations.Add(location);

            result.Fixes = new List<Fix>();

            if (tslEntry.Fixes?.Any() == true)
            {
                IList<Replacement> replacements = new List<Replacement>();

                foreach (TSLintLogFix fix in tslEntry.Fixes)
                {
                    Replacement replacement = new Replacement()
                    {
                        Offset = fix.InnerStart,
                        DeletedLength = fix.InnerLength,
                    };

                    var plainTextBytes = Encoding.UTF8.GetBytes(fix.InnerText);
                    replacement.InsertedBytes = System.Convert.ToBase64String(plainTextBytes);

                    replacements.Add(replacement);
                }

                FileChange sarifFileChange = new FileChange(uri: analysisTargetUri, uriBaseId: null, replacements: replacements);

                Fix sarifFix = new Fix(description: null, fileChanges: new List<FileChange>() { sarifFileChange });
                result.Fixes.Add(sarifFix);
            } 

            return result;
        }        
    }
}
