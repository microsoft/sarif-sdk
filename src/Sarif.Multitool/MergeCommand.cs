﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class MergeCommand
    {
        public static int Run(MergeOptions mergeOptions)
        {
	        var sarifFiles = GetSarifFiles(mergeOptions);

	        var allRuns = GetAllRuns(sarifFiles);

	        // Build one SarifLog with all the Runs.
			SarifLog combinedLog = new SarifLog(SarifVersion.TwoZeroZero.ConvertToSchemaUri(),
												SarifVersion.TwoZeroZero, allRuns);

            // Write output to file.
            string outputName = GetOutputFileName(mergeOptions);
            var formatting = mergeOptions.PrettyPrint
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None;
            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = formatting
            };
            LoggingOptions loggingOptions = mergeOptions.ConvertToLoggingOptions();
            SarifLog reformattedLog = new ReformattingVisitor(loggingOptions).VisitSarifLog(combinedLog);
            File.WriteAllText(outputName, JsonConvert.SerializeObject(reformattedLog, settings));

            return 0;
        }

	    private static IEnumerable<Run> GetAllRuns(IEnumerable<string> sarifFiles)
	    {
		    foreach (string file in sarifFiles)
		    {
			    SarifLog log = ReadFile(file);

			    if (log.Runs == null)
			    {
				    continue;
			    }

			    foreach (Run run in log.Runs)
			    {
				    if (run != null)
				    {
					    yield return new Run(run);
				    }
			    }
		    }
	    }

	    private static IEnumerable<string> GetSarifFiles(MergeOptions mergeOptions)
	    {
		    SearchOption searchOption = mergeOptions.Recurse 
				? SearchOption.AllDirectories 
				: SearchOption.TopDirectoryOnly;

		    foreach (string path in mergeOptions.Files)
		    {
			    string directory, filename;
			    if (Directory.Exists(path))
			    {
				    directory = path;
				    filename = "*";
			    }
			    else
			    {
				    directory = Path.GetDirectoryName(path) ?? path;
				    filename = Path.GetFileName(path) ?? "*";
			    }
			    foreach (string file in Directory.GetFiles(directory, filename, searchOption))
			    {
				    if (file.EndsWith(".sarif", StringComparison.OrdinalIgnoreCase))
				    {
					    yield return file;
				    }
			    }
		    }
	    }

	    private static SarifLog ReadFile(string filePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string logText = File.ReadAllText(filePath);
            
            return JsonConvert.DeserializeObject<SarifLog>(logText, settings);
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return !string.IsNullOrEmpty(mergeOptions.OutputFilePath)
                ? mergeOptions.OutputFilePath 
                : "combined.sarif";
        }
    }
}
