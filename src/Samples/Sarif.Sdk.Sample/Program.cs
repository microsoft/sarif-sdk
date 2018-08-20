// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Sarif.Sdk.Sample
{
    class Program
    {
        static int Main(string[] args)
        {
            int result = Parser.Default.ParseArguments<LoadOptions, CreateOptions>(args)
                .MapResult(
                    (LoadOptions options) => LoadSarifLogFile(options),
                    (CreateOptions options) => CreateSarifLogFile(options),
                    errors => 1);

            return result;
        }

        /// <summary>
        /// Loads a SARIF log file from disk and deserializes it into a code object.
        /// </summary>
        /// <param name="options">Load verb options.</param>
        /// <returns>Exit code</returns>
        static int LoadSarifLogFile(LoadOptions options)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
            };

            string logText = File.ReadAllText(options.InputFilePath);
            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logText, settings);

            Console.WriteLine($"The log file \"{options.InputFilePath}\" contains {log.Runs[0]?.Results.Count} results.");

            return 0;
        }

        /// <summary>
        /// Constructs a sample SARIF log and writes it to the specified location.
        /// </summary>
        /// <param name="options">Create verb options.</param>
        /// <returns>Exit code</returns>
        static int CreateSarifLogFile(CreateOptions options)
        {
            // We'll use this source file for several defect results -- the
            // SampleSourceFiles folder should be at the same level as the project folder
            // Because this file can actually be accessed by this app, its
            // content will be embedded in the log file.
            var fileLocation = new FileLocation { Uri = new Uri($"file://{AppDomain.CurrentDomain.BaseDirectory}/../../../SampleSourceFiles/AnalysisSample.cs") };

            // Create a list of rules that will be enforced during your analysis
            #region Rules list
            var rules = new List<Rule>()
            {
                new Rule
                {
                    Id ="CA1819",
                    Name = new Message { Text = "Properties should not return arrays" },
                    FullDescription = new Message { Text = "Arrays returned by properties are not write-protected, even if the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property." },
                    MessageStrings = new Dictionary<string, string>
                    {
                        { "Default", "The property {0} returns an array." }
                    }
                },
                new Rule
                {
                    Id ="CA1820",
                    Name = new Message { Text = "Test for empty strings using string length" },
                    FullDescription = new Message { Text = "Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals." },
                    MessageStrings = new Dictionary<string, string>
                    {
                        { "Default", "The test for an empty string is performed by a string comparison rather than by testing String.Length." }
                    }
                },
                new Rule
                {
                    Id ="CA2105",
                    Name = new Message { Text = "Array fields should not be read only" },
                    FullDescription = new Message { Text = "When you apply the read-only (ReadOnly in Visual Basic) modifier to a field that contains an array, the field cannot be changed to reference a different array. However, the elements of the array stored in a read-only field can be changed." },
                    MessageStrings = new Dictionary<string, string>
                    {
                        { "Default", "The array-valued field {0} is marked readonly." }
                    }
                },
                new Rule
                {
                    Id ="CA2215",
                    Name = new Message { Text = "Dispose methods should call base class dispose" },
                    FullDescription = new Message { Text = "If a type inherits from a disposable type, it must call the Dispose method of the base type from its own Dispose method." },
                    MessageStrings = new Dictionary<string, string>
                    {
                        { "Default", "The Dispose method does not call the base class Dispose method." }
                    }
                }
            };
            #endregion

            // Regions will be calculated by your analysis process
            #region Regions
            var regions = new List<Region>()
            {
                new Region // CA1819
                {
                    StartLine = 17,
                    StartColumn = 16,
                    EndColumn = 32
                },
                new Region // CA1820
                {
                    StartLine = 26,
                    StartColumn = 21,
                    EndColumn = 44
                },
                new Region // CA2105
                {
                    StartLine = 14,
                    StartColumn = 17,
                    EndColumn = 25
                },
                new Region // CA2215
                {
                    StartLine = 37,
                    StartColumn = 9,
                    EndColumn = 9
                }
            };
            #endregion

            #region Message arguments
            string[][] messageArguments = new string[][]
            {
                new string[]
                {
                    "MyIntArray"
                },

                null,

                new string[]
                {
                    "_myStringArray"
                },

                null
            };
            #endregion

            // Sets of fixes corresponding to each rule
            // Multiple fixes can be provided for the user to choose from
            #region Fixes
            IList<Fix[]> fixes = new List<Fix[]>
            {
                null, // no suggested fixes for CA1819

                new[]
                {
                    new Fix // CA1820
                    {
                        Description = new Message { Text = "Replace empty string test with test for zero length." },
                        FileChanges = new[]
                        {
                            new FileChange
                            {
                                FileLocation = fileLocation,
                                Replacements = new[]
                                {
                                    new Replacement
                                    {
                                        DeletedRegion = new Region
                                        {
                                            StartLine = 26,
                                            StartColumn = 38,
                                            EndColumn = 44
                                        },
                                        InsertedContent = new FileContent
                                        {
                                            Text = ".Length == 0"
                                        }
                                    }
                                }
                            }
                        },
                    }
                },

                null, // no suggested fix for CA2105

                new[]
                {
                    new Fix // CA2215
                    {
                        Description = new Message { Text = "Call base.Dispose in the derived's class's Dispose method." },
                        FileChanges = new[]
                        {
                            new FileChange
                            {
                                FileLocation = fileLocation,
                                Replacements = new[]
                                {
                                    new Replacement
                                    {
                                        DeletedRegion = new Region
                                        {
                                            StartLine = 37,
                                            StartColumn = 1,
                                            EndColumn = 1
                                        },
                                        InsertedContent = new FileContent
                                        {
                                            Text = @"            base.Dispose();\n"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            #endregion

            // The SarifLogger will write the JSON-formatted log to this StringBuilder
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    loggingOptions: LoggingOptions.PrettyPrint, // Use PrettyPrint to generate readable (multi-line, indented) JSON
                    dataToInsert:
                        OptionallyEmittedData.TextFiles |       // Embed source file content directly in the log file -- great for portability of the log!
                        OptionallyEmittedData.Hashes,
                    tool: null,
                    run: null,
                    analysisTargets: null,
                    targetsAreTextFiles: true,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null,
                    defaultFileEncoding: null))
                {
                    // Create one result for each rule
                    for (int i = 0; i < rules.Count; i++)
                    {
                        Rule rule = rules[i];
                        Region region = regions[i];

                        var result = new Result()
                        {
                            RuleId = rule.Id,
                            AnalysisTarget = new FileLocation { Uri = new Uri(@"file://d:/src/module/example.dll") }, // This is the file that was analyzed
                            Message = new Message
                            {
                                Arguments = messageArguments[i]
                            },
                            RuleMessageId = "Default",
                            Locations = new[]
                            {
                                new Location
                                {
                                    PhysicalLocation = new PhysicalLocation
                                    {
                                        FileLocation = fileLocation,
                                        Region = region
                                    }
                                },
                            },
                            Fixes = fixes[i],
                            RelatedLocations = new[]
                            {
                                new Location
                                {
                                    PhysicalLocation = new PhysicalLocation
                                    {
                                        FileLocation = new FileLocation
                                        {
                                            // Because this file doesn't exist, it will be included in the files list but will only have a path and MIME type
                                            // This is the behavior you'll see any time a file can't be located/accessed
                                            Uri = new Uri($"file://{AppDomain.CurrentDomain.BaseDirectory}/../../../SampleSourceFiles/SomeOtherSourceFile.cs"),
                                        },
                                        Region = new Region
                                        {
                                            StartLine = 147,
                                            StartColumn = 19,
                                            EndLine = 147,
                                            EndColumn = 40
                                        }
                                    }
                                }
                            },
                            Stacks = new[]
                            {
                                new Stack
                                {
                                    Frames = new[]
                                    {
                                        new StackFrame
                                        {
                                            // The method that contains the defect
                                            Location = new Location
                                            {
                                                PhysicalLocation = new PhysicalLocation
                                                {
                                                    FileLocation = fileLocation,
                                                    Region = new Region
                                                    {
                                                        StartLine = 212
                                                    }
                                                }
                                            }
                                        },
                                        new StackFrame
                                        {
                                            // The method that calls the one above, e.g. ComputeSomeValue()
                                            Location = new Location
                                            {
                                                PhysicalLocation = new PhysicalLocation
                                                {
                                                    FileLocation = fileLocation,
                                                    Region = new Region
                                                    {
                                                        StartLine = 452 // Fake example
                                                    }
                                                }
                                            }
                                        },
                                        new StackFrame
                                        {
                                            // The method that calls the one above, e.g. Main()
                                            Location = new Location
                                            {
                                                PhysicalLocation = new PhysicalLocation
                                                {
                                                    FileLocation = fileLocation,
                                                    Region = new Region
                                                    {
                                                        StartLine = 145
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        // Let's add a CodeFlow for the first defect (properties shouldn't return arrays)
                        // This flow shows where the array is declared, and where it is returned by a property
                        if (i == 0)
                        {
                            var codeFlow = new CodeFlow
                            {
                                // This is what a single-threaded result looks like
                                // TIP: use SarifUtilities.CreateSingleThreadedCodeFlow to reduce repetition
                                // Multi-threaded example coming soon!
                                ThreadFlows = new[]
                                {
                                    new ThreadFlow
                                    {
                                        Locations = new[]
                                        {
                                            new ThreadFlowLocation
                                            {
                                                // This is the defect statement's location
                                                Location = new Location
                                                {
                                                    PhysicalLocation = new PhysicalLocation
                                                    {
                                                        FileLocation = fileLocation,
                                                        Region = region
                                                    }
                                                },
                                                Step = 1,
                                                Importance = ThreadFlowLocationImportance.Essential
                                            },
                                            new ThreadFlowLocation
                                            {
                                                // This is the declaration of the array
                                                Location = new Location
                                                {
                                                    PhysicalLocation = new PhysicalLocation
                                                    {
                                                        FileLocation = fileLocation,
                                                        Region = new Region
                                                        {
                                                            StartLine = 12
                                                        }
                                                    }
                                                },
                                                NestingLevel = 1,
                                                Step = 2,
                                                Importance = ThreadFlowLocationImportance.Important
                                            }
                                        }
                                    }
                                }
                            };
                            result.CodeFlows = new[] { codeFlow };
                        }

                        sarifLogger.Log(rule, result);
                    }
                }
            }

            File.WriteAllText(options.OutputFilePath, sb.ToString());
            return 0;
        }
    }
}
