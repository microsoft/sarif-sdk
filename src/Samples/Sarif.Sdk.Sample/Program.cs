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
			// content will be embedded in the log file
            var fileLocation = new FileLocation { Uri = new Uri($"file://{AppDomain.CurrentDomain.BaseDirectory}/../../../../SampleSourceFiles/AnalysisSample.cs") };

            // Create a list of rules that will be enforced during your analysis
            #region Rules list
            var rules = new List<Rule>()
            {
                new Rule
                {
                    Id ="CA1819",
                    Name = new Message { Text = "Properties should not return arrays" },
                    FullDescription = new Message { Text = "Arrays returned by properties are not write-protected, even if the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property." }
                },
                new Rule
                {
                    Id ="CA1820",
                    Name = new Message { Text = "Test for empty strings using string length" },
                    FullDescription = new Message { Text = "Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals." }
                },
                new Rule
                {
                    Id ="CA2105",
                    Name = new Message { Text = "Array fields should not be read only" },
                    FullDescription = new Message { Text = "When you apply the read-only (ReadOnly in Visual Basic) modifier to a field that contains an array, the field cannot be changed to reference a different array. However, the elements of the array stored in a read-only field can be changed." }
                },
                new Rule
                {
                    Id ="CA2215",
                    Name = new Message { Text = "Dispose methods should call base class dispose" },
                    FullDescription = new Message { Text = "If a type inherits from a disposable type, it must call the Dispose method of the base type from its own Dispose method." }
                },
                //new Rule
                //{
                //    Id ="CA1816",
                //    Name = new Message { Text = "Call GC.SuppressFinalize correctly" },
                //    FullDescription = new Message { Text = "A method that is an implementation of Dispose does not call GC.SuppressFinalize, or a method that is not an implementation of Dispose calls GC.SuppressFinalize, or a method calls GC.SuppressFinalize and passes something other than this (Me in Visual Basic)." }
                //},
                //new Rule
                //{
                //    Id ="CA2006",
                //    Name = new Message { Text = "Use SafeHandle to encapsulate native resources" },
                //    FullDescription = new Message { Text = "Use of IntPtr in managed code might indicate a potential security and reliability problem. All uses of IntPtr must be reviewed to determine whether use of a SafeHandle, or similar technology, is required in its place." }
                //}
            };
            #endregion

            // Regions will be calculated by your analysis process
            #region Regions
            var regions = new List<Region>()
            {
                new Region // CA1819
                {
                    StartLine = 16,
                    StartColumn = 19,
                    EndLine = 16,
                    EndColumn = 38,
                    Offset = 331, // Offset should account for the BOM, if present in source file
                    Length = 19
                },
                new Region // CA1820
                {
                    StartLine = 23,
                    StartColumn = 21,
                    EndLine = 23,
                    EndColumn = 44,
                    Offset = 507, // Offset should account for the BOM, if present in source file
                    Length = 23
                },
                new Region // CA2105
                {
                    StartLine = 11,
                    StartColumn = 9,
                    EndLine = 11,
                    EndColumn = 50,
                    Offset = 198, // Offset should account for the BOM, if present in source file
                    Length = 41
                },
                new Region // CA2215
                {
                    StartLine = 32,
                    StartColumn = 9,
                    EndLine = 32,
                    EndColumn = 30,
                    Offset = 646, // Offset should account for the BOM, if present in source file
                    Length = 21
                }
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
                                        DeletedLength = 6,
                                        InsertedBytes = ".Length == 0",
                                        Offset = 524
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
                                        DeletedLength = 0,
                                        InsertedBytes = @"\nbase.Dispose();",
                                        Offset = 656
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
                    analysisTargets: null,
                    loggingOptions: LoggingOptions.PersistFileContents | // <-- embed source file content directly in the log file -- great for portability of the log!
                                    LoggingOptions.ComputeFileHashes |
                                    LoggingOptions.PrettyPrint, // <-- use PrettyPrint to generate readable (multi-line, indented) JSON
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null))
                {
                    // Create one result for each rule
                    for (int i = 0; i < rules.Count; i++)
                    {
                        Rule rule = rules[i];
                        Region region = regions[i];

                        var result = new Result()
                        {
                            RuleId = rule.Id,
                            AnalysisTarget = new FileLocation { Uri = new Uri(@"file://d:/src/module/foo.dll") }, // This is the file that was analyzed
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
                                            EndColumn = 40,
                                            Offset = 1245,
                                            Length = 21
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
                                            new CodeFlowLocation
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
                                                Importance = CodeFlowLocationImportance.Essential
                                            },
                                            new CodeFlowLocation
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
                                                Importance = CodeFlowLocationImportance.Important
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
