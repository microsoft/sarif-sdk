// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class StaticDriverVerifierConverter : ToolFileConverterBase
    {
        private StringBuilder _sb;
        private Stack<string> _callers;

        /// <summary>Initializes a new instance of the <see cref="StaticDriverVerifierConverter"/> class.</summary>
        public StaticDriverVerifierConverter()
        {
            _sb = new StringBuilder();
            _callers = new Stack<string>();
        }

        /// <summary>
        /// Interface implementation that takes a Static Driver Verifier log stream and converts
        ///  its data to a SARIF json stream. Read in Static Driver Verifier data from an input
        ///  stream and write Result objects.
        /// </summary>
        /// <param name="input">Stream of a Static Driver Verifier log</param>
        /// <param name="output">SARIF json stream of the converted Static Driver Verifier log</param>
        public override void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            Result result = ProcessSdvDefectStream(input);
            var results = new Result[] { result };

            var tool = new Tool
            {
                Name = "StaticDriverVerifier",
            };

            var fileInfoFactory = new FileInfoFactory(null);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            output.Initialize(id: null, correlationId: null);

            output.WriteTool(tool);
            if (fileDictionary != null && fileDictionary.Count > 0) { output.WriteFiles(fileDictionary); }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        private Result ProcessSdvDefectStream(Stream input)
        {
            var result = new Result();

            result.Locations = new List<Location>();
       
            result.CodeFlows = new[]
            {
                new CodeFlow
                {
                    Locations = new List<AnnotatedCodeLocation>()
                }
            };

            using (var reader = new StreamReader(input))
            {
                string line;

                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    ProcessLine(line, result);
                }
            }

            return result;
        }

        private void ProcessLine(string logFileLine, Result result)
        {
            var codeFlow = result.CodeFlows[0];

            const int STEP  = 0;
            const int URI   = 1;
            const int LINE  = 2;
            // const int IMPORTANCE  = 3; This value not persisted to SARIF
            const int STATE = 4;
            const int KIND1 = 5;

            // When KIND1 == "Atomic" the 6th slot is the
            // the remainder of the kind id, e.g., Atomic Assigment
            const int KIND2 = 6;

            // When KIND1 == "Call" the 6th and 7th slots are:
            const int CALLER = 6;
            const int CALLEE = 7;

            int step;

            string[] tokens = logFileLine.Split(' ');

            if (int.TryParse(tokens[STEP], out step))
            {
                // If we find a numeric value as the first token,
                // this is a general step.

                Uri uri = null;
                string uriText = tokens[URI].Trim('"');

                if (!uriText.Equals("?", StringComparison.Ordinal))
                {
                    if (File.Exists(uriText))
                    {
                        uriText = Path.GetFullPath(uriText);
                    }
                    uri = new Uri(uriText, UriKind.RelativeOrAbsolute);
                }

                // We assume a valid line here. This code will throw if not.
                int line = int.Parse(tokens[LINE]);

                string sdvKind = tokens[KIND1];

                if (sdvKind.Equals("Atomic", StringComparison.Ordinal))
                {
                    // For multipart SDV kinds 'Atomic XXX', we 
                    // map using the second value only, e.g, 
                    // 'Assignment' or 'Conditional'
                    sdvKind = tokens[KIND2];
                }

                AnnotatedCodeLocationKind kind = ConvertToAnnotatedCodeLocationKind(sdvKind.Trim());

                var annotatedCodeLocation = new AnnotatedCodeLocation
                {
                    Kind = kind,
                    Step = step,
                    Importance = AnnotatedCodeLocationImportance.Unimportant,
                    PhysicalLocation = (uri != null) ? new PhysicalLocation
                    {
                        Uri = uri,
                        Region = new Region
                        {
                            StartLine = line
                        }
                    } : null,    
                };

                if (kind == AnnotatedCodeLocationKind.Call)
                {
                    string extraMsg = tokens[KIND1] + " " + tokens[CALLER] + " " + tokens[CALLEE];

                    string caller, callee;

                    if (ExtractCallerAndCallee(extraMsg.Trim(), out caller, out callee))
                    {
                        annotatedCodeLocation.FullyQualifiedLogicalName = caller;
                        annotatedCodeLocation.Target = callee;
                        _callers.Push(caller);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    if (uri == null)
                    {
                        annotatedCodeLocation.Importance = AnnotatedCodeLocationImportance.Unimportant;
                    }
                    else if (IsHarnessOrRulesFiles(uriText))
                    {
                        annotatedCodeLocation.Importance = AnnotatedCodeLocationImportance.Important;
                    }
                    else
                    {
                        annotatedCodeLocation.Importance = AnnotatedCodeLocationImportance.Essential;
                    }

                }

                if (kind == AnnotatedCodeLocationKind.CallReturn)
                {
                    Debug.Assert(_callers.Count > 0);
                    annotatedCodeLocation.FullyQualifiedLogicalName = _callers.Pop();
                }

                string separatorText = "^====Auto=====";
                string state = tokens[STATE];
                string[] stateTokens = state.Split(new string[] { separatorText }, StringSplitOptions.RemoveEmptyEntries);

                if (stateTokens.Length > 0)
                {
                    if (stateTokens.Length == 2)
                    {
                        annotatedCodeLocation.SetProperty("currentDataValues", stateTokens[0]);
                        annotatedCodeLocation.SetProperty("permanentDataValues", stateTokens[1]);
                    }
                    else
                    {
                        Debug.Assert(stateTokens.Length == 1);
                        if (stateTokens[0].StartsWith(separatorText))
                        {
                            annotatedCodeLocation.SetProperty("permanentDataValues", stateTokens[0]);
                        }
                        else
                        {
                            annotatedCodeLocation.SetProperty("currentDataValues", stateTokens[0]);
                        }
                    }
                }

                codeFlow.Locations.Add(annotatedCodeLocation);
            }
            else
            {
                // This is the defect message.
                const int LEVEL  = 0;

                string levelText = tokens[LEVEL];

                result.Level = ConvertToResultLevel(levelText);

                // Everything on the line following defect level comprises the message
                result.Message = logFileLine.Substring(levelText.Length).Trim();

                // SDV currently produces 'pass' notifications when 
                // the final line is prefixed with 'Error'. We'll examine
                // the message text to detect this condition
                if (result.Message.Contains("is satisfied"))
                {
                    result.Level = ResultLevel.Pass;
                }

                // Finally, populate this result location with the
                // last observed location in the code flow.

                IList<AnnotatedCodeLocation> locations = result.CodeFlows[0].Locations;

                for (int i = locations.Count - 1; i >= 0; --i)
                {
                    if (locations[i].PhysicalLocation != null)
                    {
                        result.Locations.Add(new Location
                        {
                            ResultFile = locations[i].PhysicalLocation
                        });
                        break;
                    }
                }
            }
        }

        private bool IsHarnessOrRulesFiles(string fileName)
        {
            return fileName.EndsWith(".slic", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith("sdv-harness.c", StringComparison.OrdinalIgnoreCase);
        }

        private static Regex s_callRegex = new Regex(@"Call ""(.*)"" ""(.*)""", RegexOptions.Compiled);

        private static bool ExtractCallerAndCallee(string text, out string caller, out string callee)
        {
            caller = callee = null;

            var match = s_callRegex.Match(text);
            if (match.Success && match.Groups.Count == 3)
            {
                caller = match.Groups[1].Value;
                callee = match.Groups[2].Value;
                return true;
            }
            return false;
        }

        private static AnnotatedCodeLocationKind ConvertToAnnotatedCodeLocationKind(string sdvKind)
        {
            switch (sdvKind)
            {
                case "Assignment": return AnnotatedCodeLocationKind.Assignment;
                case "Call": return AnnotatedCodeLocationKind.Call;
                case "Conditional": return AnnotatedCodeLocationKind.Branch;
                case "Continuation": return AnnotatedCodeLocationKind.Continuation;
                case "Return": return AnnotatedCodeLocationKind.CallReturn;
            }

            Debug.Assert(false);
            return AnnotatedCodeLocationKind.Unknown;
        }

        private static ResultLevel ConvertToResultLevel(string sdvLevel)
        {
            switch (sdvLevel)
            {
                case "Error": return ResultLevel.Error;
            }

            Debug.Assert(false);
            return ResultLevel.Default;
        }
    }
}
