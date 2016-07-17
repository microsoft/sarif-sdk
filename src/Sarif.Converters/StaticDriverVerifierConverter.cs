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

        /// <summary>Initializes a new instance of the <see cref="StaticDriverVerifierConverter"/> class.</summary>
        public StaticDriverVerifierConverter()
        {
            _sb = new StringBuilder();
        }

        /// <summary>
        /// Interface implementation that takes a CppChecker log stream and converts its data to a SARIF json stream.
        /// Read in CppChecker data from an input stream and write Result objects.
        /// </summary>
        /// <param name="input">Stream of a CppChecker log</param>
        /// <param name="output">SARIF json stream of the converted CppChecker log</param>
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

            result.CodeFlows = new List<CodeFlow>();        
            result.CodeFlows.Add(new CodeFlow
            {
                Locations = new List<AnnotatedCodeLocation>()
            });

            var codeLocationStack = new Stack<AnnotatedCodeLocation>();
            var sb = new StringBuilder();
            char current;
            while ((current = (char)input.ReadByte()) != '\uffff')
            {
                sb.Append(current);

                if (current == '\n')
                {
                    ProcessLine(sb.ToString(), result, codeLocationStack);
                    sb.Clear();
                }
            }

            return result;
        }

        private void ProcessLine(string logFileLine, Result result, Stack<AnnotatedCodeLocation> codeLocationStack)
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

            int step;

            string[] tokens = logFileLine.Split(' ');

            if (int.TryParse(tokens[STEP], out step))
            {
                bool displayed = true;
                var importance = AnnotatedCodeLocationImportance.Important;

                // If we find a numeric value as the first token,
                // this is a general step. We don't actually consume
                // the step, as it is a 0-indexed value and SARIF
                // specifies 1-indexed.

                string uriText = tokens[URI].Trim('"');

                if (IsNonessentialFile(uriText))
                {
                    importance = AnnotatedCodeLocationImportance.Unimportant;
                }


                string uriValue = tokens[URI].Trim('"');

                if (uriText.Equals("?"))
                {
                    displayed = false;
                }
                else
                {
                    if (File.Exists(uriValue))
                    {
                        uriValue = Path.GetFullPath(uriValue);
                    }
                }

                var uri = new Uri(uriValue, UriKind.RelativeOrAbsolute);

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
                    Importance = importance,
                    PhysicalLocation = displayed ? new PhysicalLocation
                    {
                        Uri = uri,
                        Region = new Region
                        {
                            StartLine = line
                        }
                    } : null,    
                };

                var state = tokens[STATE];
                string[] stateTokens = state.Split(new string[] { "^====Auto=====" }, StringSplitOptions.RemoveEmptyEntries);

                if (stateTokens.Length == 1)
                {
                    // We have a single token only because no data values were specified
                    if (state.StartsWith("^====Auto====="))
                    {
                        annotatedCodeLocation.SetProperty("permVars", stateTokens[0]);
                    }
                    else
                    {
                        // Only data values were specified, no permanent vars
                        annotatedCodeLocation.SetProperty("dataValuesCurrent", stateTokens[0]);
                    }
                }
                else if (stateTokens.Length > 1)
                {
                    annotatedCodeLocation.SetProperty("dataValuesCurrent", stateTokens[0]);
                    annotatedCodeLocation.SetProperty("permVars", stateTokens[1]);
                }

                if (kind == AnnotatedCodeLocationKind.Call)
                {
                    codeLocationStack.Push(annotatedCodeLocation);

                    string[] callTokens = logFileLine.Split(new string[] { " Call " }, StringSplitOptions.RemoveEmptyEntries);
                    Debug.Assert(callTokens.Length == 2);

                    string extraMsg = "Call " + callTokens[1].Replace('"', '\'').Trim();
                    annotatedCodeLocation.SetProperty("extraMsg", extraMsg);

                    string caller, callee; 

                    if (IsImportantCall(uriValue, extraMsg, out caller, out callee))
                    {
                        foreach(AnnotatedCodeLocation acl in codeLocationStack)
                        {
                            if (acl.Importance == AnnotatedCodeLocationImportance.Unimportant)
                            {
                                acl.Importance = AnnotatedCodeLocationImportance.Essential;
                            }
                        }
                    }

                    if (caller != null)
                    {
                        annotatedCodeLocation.FullyQualifiedLogicalName = caller;
                    }

                    if (callee != null)
                    {
                        annotatedCodeLocation.FullyQualifiedLogicalName = callee;
                    }
                }
                else if (kind == AnnotatedCodeLocationKind.CallReturn)
                {
                    AnnotatedCodeLocation caller = codeLocationStack.Pop();
                    annotatedCodeLocation.Importance = caller.Importance;
                    annotatedCodeLocation.FullyQualifiedLogicalName = caller.FullyQualifiedLogicalName;
                }
                else if (uriValue == "?")
                {
                    annotatedCodeLocation.Importance = AnnotatedCodeLocationImportance.Unimportant;
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

        private static Regex s_callRegEx = new Regex(@"Call '(.*)' '(.*)'", RegexOptions.Compiled);
        private bool IsImportantCall(string fileName, string extraMsg, out string caller, out string callee)
        {
            caller = callee = null;

            Match match = s_callRegEx.Match(extraMsg);
            if (match.Success && match.Groups.Count == 3)
            {
                caller = match.Groups[0].Value;
                callee = match.Groups[1].Value;

                return (!IsHarnessOrRuleFile(fileName) && !callee.StartsWith("SLIC_"));
            }
            Debug.Assert(false);
            return false;
        }

        private bool IsNonessentialFile(string fileName)
        {
            return IsHarnessOrRuleFile(fileName) || fileName == "?";
        }

        private bool IsHarnessOrRuleFile(string fileName)
        {
            return fileName.EndsWith(".slic") || fileName.EndsWith("sdv-harness.c");
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
            return ResultLevel.Unknown;
        }
    }
}
