﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Sarif.Sdk.Sample
{
    /// <summary>
    /// Example: 
    /// .\SarifSdkSample.exe create C:\SarifLog\sample1.sarif --numResult 5000 --useFileStream
    /// .\SarifSdkSample.exe create C:\SarifLog\sample1.sarif --numResult 500
    /// </summary>
    public class Program
    {
        private const float BytesPerMB = (float)(1024 * 1024);
        private const string RepoRootBaseId = "REPO_ROOT";
        private const string BinRootBaseId = "BIN_ROOT";

        internal static int Main(string[] args)
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
        internal static int LoadSarifLogFile(LoadOptions options)
        {
            string logText = File.ReadAllText(options.InputFilePath);
            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logText);

            Console.WriteLine($"The log file \"{options.InputFilePath}\" contains {log.Runs[0]?.Results.Count} results.");

            return 0;
        }

        /// <summary>
        /// Constructs a sample SARIF log and writes it to the specified location.
        /// </summary>
        /// <param name="options">Create verb options.</param>
        /// <returns>Exit code</returns>
        internal static int CreateSarifLogFile(CreateOptions options)
        {
            if (options.NumOfResultPerRule < 0 || options.NumOfResultPerRule > int.MaxValue)
            {
                return -1;
            }
            // We'll use this source file for several defect results -- the
            // SampleSourceFiles folder should be a child of the project folder,
            // two levels up from the folder that contains the EXE (e.g., bin\Debug).
            string scanRootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\SampleSourceFiles\"));
            var scanRootUri = new Uri(scanRootDirectory, UriKind.Absolute);
            var artifactLocation = new ArtifactLocation
            {
                Uri = new Uri("AnalysisSample.cs", UriKind.Relative),
                UriBaseId = RepoRootBaseId
            };

            // Create a list of rules that will be enforced during your analysis
            #region Rules list
            var rules = new List<ReportingDescriptor>()
            {
                new ReportingDescriptor
                {
                    Id ="CA1819",
                    Name = "Properties should not return arrays",
                    FullDescription = new MultiformatMessageString { Text = "Arrays returned by properties are not write-protected, even if the property is read-only. To keep the array tamper-proof, the property must return a copy of the array. Typically, users will not understand the adverse performance implications of calling such a property." },
                    MessageStrings = new Dictionary<string, MultiformatMessageString>
                    {
                        {
                            "Default",
                            new MultiformatMessageString
                            {
                                Text = "The property {0} returns an array."
                            }
                        }
                    },
                    HelpUri = new Uri("https://www.example.com/rules/CA1819")
                },
                new ReportingDescriptor
                {
                    Id ="CA1820",
                    Name = "Test for empty strings using string length",
                    FullDescription = new MultiformatMessageString { Text = "Comparing strings by using the String.Length property or the String.IsNullOrEmpty method is significantly faster than using Equals." },
                    MessageStrings = new Dictionary<string, MultiformatMessageString>
                    {
                        {
                            "Default",
                            new MultiformatMessageString
                            {
                                Text = "The test for an empty string is performed by a string comparison rather than by testing String.Length."
                            }
                        }
                    },
                    HelpUri = new Uri("https://www.example.com/rules/CA1820")
                },
                new ReportingDescriptor
                {
                    Id ="CA2105",
                    Name = "Array fields should not be read only",
                    FullDescription = new MultiformatMessageString { Text = "When you apply the read-only (ReadOnly in Visual Basic) modifier to a field that contains an array, the field cannot be changed to reference a different array. However, the elements of the array stored in a read-only field can be changed." },
                    MessageStrings = new Dictionary<string, MultiformatMessageString>
                    {
                        {
                            "Default",
                            new MultiformatMessageString
                            {
                                Text = "The array-valued field {0} is marked readonly."
                            }
                        }
                    },
                    HelpUri = new Uri("https://www.example.com/rules/CA2105")
                },
                new ReportingDescriptor
                {
                    Id ="CA2215",
                    Name = "Dispose methods should call base class dispose",
                    FullDescription = new MultiformatMessageString { Text = "If a type inherits from a disposable type, it must call the Dispose method of the base type from its own Dispose method." },
                    MessageStrings = new Dictionary<string, MultiformatMessageString>
                    {
                        {
                            "Default",
                            new MultiformatMessageString
                            {
                                Text = "The Dispose method does not call the base class Dispose method."
                            }
                        }
                    },
                    HelpUri = new Uri("https://www.example.com/rules/CA2215")
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
                        ArtifactChanges = new[]
                        {
                            new ArtifactChange
                            {
                                ArtifactLocation = artifactLocation,
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
                                        InsertedContent = new ArtifactContent
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
                        ArtifactChanges = new[]
                        {
                            new ArtifactChange
                            {
                                ArtifactLocation = artifactLocation,
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
                                        InsertedContent = new ArtifactContent
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

            string binRootDirectory = @"d:\src\module\";
            var binRootUri = new Uri(binRootDirectory, UriKind.Absolute);

            var run = new Run
            {
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                {
                    [RepoRootBaseId] = new ArtifactLocation
                    {
                        Uri = scanRootUri
                    },
                    [BinRootBaseId] = new ArtifactLocation
                    {
                        Uri = binRootUri
                    }
                },
                VersionControlProvenance = new VersionControlDetails[]
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://github.com/microsoft/sarif-sdk"),
                        RevisionId = "ee5a1ca8",
                        Branch = "master",
                        MappedTo = new ArtifactLocation
                        {
                            Uri = new Uri(".", UriKind.Relative),
                            UriBaseId = RepoRootBaseId
                        }
                    }
                }
            };

            var sb = new StringBuilder();
            Measure(() =>
            {
                SerializeSarifResult(options.OutputFilePath, options.NumOfResultPerRule, options.UseFileStream,
                    sb, run, rules, regions, messageArguments, artifactLocation, fixes);

                return $"Serialize Sarif Result using {(options.UseFileStream ? "FileStream" : "StringBuilder")}";
            });

            return 0;
        }

        private static void SerializeSarifResult(string filePath, int numResult, bool useFileStream, StringBuilder stringBuilder, Run run, IList<ReportingDescriptor> rules, IList<Region> regions, string[][] messageArguments, ArtifactLocation artifactLocation, IList<Fix[]> fixes)
        {
            // write to file stream or stringbuilder
            using (TextWriter textWriter = useFileStream ? new StreamWriter(filePath) : (TextWriter)new StringWriter(stringBuilder))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    logFilePersistenceOptions: FilePersistenceOptions.PrettyPrint, // Use PrettyPrint to generate readable (multi-line, indented) JSON
                    dataToInsert:
                        OptionallyEmittedData.TextFiles |       // Embed source file content directly in the log file -- great for portability of the log!
                        OptionallyEmittedData.Hashes |
                        OptionallyEmittedData.RegionSnippets,
                    run: run,
                    kinds: BaseLogger.Fail,
                    levels: BaseLogger.ErrorWarning,
                    analysisTargets: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null,
                    defaultFileEncoding: null))
                {
                    // Create one result for each rule
                    for (int i = 0; i < rules.Count; i++)
                    {
                        ReportingDescriptor rule = rules[i];
                        Region region = regions[i];

                        foreach (Result result in GenerateResults(
                                                    numResult,
                                                    rule,
                                                    messageArguments[i],
                                                    artifactLocation,
                                                    region,
                                                    fixes[i]))
                        {
                            sarifLogger.Log(rule, result, extensionIndex: null);
                        }
                    }
                }
            }

            if (!useFileStream)
            {
                File.WriteAllText(filePath, stringBuilder.ToString());
            }
        }

        private static IEnumerable<Result> GenerateResults(int num, ReportingDescriptor rule, string[] messageArgs, ArtifactLocation artifactLocation, Region region, Fix[] fixes)
        {
            for (int j = 0; j < num; j++)
            {
                yield return new Result()
                {
                    RuleId = rule.Id,
                    AnalysisTarget = new ArtifactLocation
                    {
                        Uri = new Uri("example.dll", UriKind.Relative), // This is the file that was analyzed
                        UriBaseId = BinRootBaseId
                    },
                    Message = new Message
                    {
                        Id = "Default",
                        Arguments = messageArgs,
                    },
                    Locations = new[]
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = artifactLocation,
                                Region = region
                            }
                        },
                    },
                    Fixes = fixes,
                    RelatedLocations = new[]
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    // Because this file doesn't exist, it will be included in the files list but will only have a path and MIME type
                                    // This is the behavior you'll see any time a file can't be located/accessed
                                    Uri = new Uri("SomeOtherSourceFile.cs", UriKind.Relative),
                                    UriBaseId = RepoRootBaseId
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
                                            ArtifactLocation = artifactLocation,
                                            Region = new Region
                                            {
                                                StartLine = 17
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
                                            ArtifactLocation = artifactLocation,
                                            Region = new Region
                                            {
                                                StartLine = 24 // Fake example
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
                                            ArtifactLocation = artifactLocation,
                                            Region = new Region
                                            {
                                                StartLine = 26 // Fake example
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    CodeFlows = new[]
                    {
                        new CodeFlow
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
                                                    ArtifactLocation = artifactLocation,
                                                    Region = region
                                                }
                                            },
                                            Importance = ThreadFlowLocationImportance.Essential
                                        },
                                        new ThreadFlowLocation
                                        {
                                            // This is the declaration of the array
                                            Location = new Location
                                            {
                                                PhysicalLocation = new PhysicalLocation
                                                {
                                                    ArtifactLocation = artifactLocation,
                                                    Region = new Region
                                                    {
                                                        StartLine = 12
                                                    }
                                                }
                                            },
                                            NestingLevel = 1,
                                            Importance = ThreadFlowLocationImportance.Important
                                        }
                                    }
                                }
                            }
                    }
                }
                };
            }
        }

        private static void Measure(Func<string> action)
        {
            long ramBefore = GC.GetTotalMemory(true);
            // long ramBefore = System.Diagnostics.Process.GetCurrentProcess().VirtualMemorySize64;
            var w = System.Diagnostics.Stopwatch.StartNew();

            string message = action();

            w.Stop();
            long ramAfter = GC.GetTotalMemory(true);
            // long ramAfter = System.Diagnostics.Process.GetCurrentProcess().VirtualMemorySize64;

            Console.WriteLine($"{message} in {w.ElapsedMilliseconds:n0} ms and {((ramAfter - ramBefore) / BytesPerMB):n1} MB RAM.");
        }
    }
}
