// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Writers;

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

            var sb = new StringBuilder();
            char current;
            while ((current = (char)input.ReadByte()) != '\uffff')
            {
                sb.Append(current);

                if (current == '\n')
                {
                    ProcessLine(sb.ToString(), result);
                    sb.Clear();
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
                bool displayed = true;

                // If we find a numeric value as the first token,
                // this is a general step. We don't actually consume
                // the step, as it is a 0-indexed value and SARIF
                // specifies 1-indexed.

                string uriText = tokens[URI].Trim('"');

                if (uriText.Equals("?", StringComparison.Ordinal))
                {
                    // If we have not literal location, then we are processing
                    // an informational step, which we will not persist
                    displayed = false;
                }

                var uri = new Uri(tokens[URI].Trim('"'), UriKind.RelativeOrAbsolute);

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

                string message = BuildMessageFromState(tokens[STATE]);

                var annotatedCodeLocation = new AnnotatedCodeLocation
                {
                    Kind = kind,
                    Step = step,
                    Importance = displayed ? AnnotatedCodeLocationImportance.Normal : AnnotatedCodeLocationImportance.Nonessential,
                    Message = message,
                    PhysicalLocation = displayed ? new PhysicalLocation
                    {
                        Uri = uri,
                        Region = new Region
                        {
                            StartLine = line
                        }
                    } : null,    
                };

                annotatedCodeLocation.SetProperty("State", tokens[STATE]);

                // Tokens[BOOL] retrieves an SDV property intended to indicate
                // whether a step is 'important' or not. We don't currenlty use this
                // value to set the 'essential' property, due to the prevalence of
                // a 'true' value even in steps that entirely repeat the previous
                // step and which have no location. As above, in the handling for 
                // locations of "?", what we do instead is filter everything with
                // no location. This actually code cause problems, as a some
                // steps with locations are embedded in call/returns that 
                // are discarded. This converter (and the viewer experience)
                // should be compared soon with the actual SDV viewer, with
                // a revisit of the converter's behavior.           


                if (sdvKind.Equals("Call", StringComparison.Ordinal))
                {
                    // Caller is encapsulated in double quotes, which we trim
                    annotatedCodeLocation.SetProperty("Caller", tokens[CALLER].Trim('"'));

                    // Callee is at the end of the line. First we trim the \r\b.
                    // Next remove the encapsulating double-quotes.
                    annotatedCodeLocation.SetProperty("Callee", tokens[CALLEE].Trim().Trim('"'));
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

        private string BuildMessageFromState(string state)
        {
            if (string.IsNullOrEmpty(state) || state == "^")
            {
                return null;
            }

            return state.Replace("^", ",").Trim(',');
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
