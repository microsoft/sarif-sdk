using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	        var sarifFiles = GetSarifFiles(mergeOptions.Files);

	        var allRuns = GetAllRuns(sarifFiles);

	        // Build one SarifLog with all the Runs.
			SarifLog combinedLog = new SarifLog(SarifVersion.OneZeroZero.ConvertToSchemaUri(),
												SarifVersion.OneZeroZero, allRuns);

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
		    foreach (var file in sarifFiles)
		    {
			    SarifLog log = ReadFile(file);

			    if (log.Runs == null)
			    {
				    continue;
			    }

			    foreach (var run in log.Runs)
			    {
				    if (run != null)
				    {
					    yield return new Run(run);
				    }
			    }
		    }
	    }

	    private static IEnumerable<string> GetSarifFiles(IEnumerable<string> files)
	    {
		    foreach (var path in files)
		    {
			    var lastBlackslashPos = path.LastIndexOf('\\') + 1;
			    var directory = path.Substring(0, lastBlackslashPos);
			    var filename = path.Substring(lastBlackslashPos, path.Length - lastBlackslashPos);
			    foreach (var file in Directory.GetFiles(directory, filename, SearchOption.AllDirectories))
			    {
				    if (file.EndsWith(".sarif", StringComparison.InvariantCultureIgnoreCase))
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
            
            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logText, settings);

            return log;
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return !string.IsNullOrEmpty(mergeOptions.OutputFilePath)
                ? mergeOptions.OutputFilePath 
                : "combined.sarif";
        }
    }
}
