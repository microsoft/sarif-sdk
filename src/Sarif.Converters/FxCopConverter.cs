// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts FxCop report files to sarif format
    /// </summary>
    ///<remarks>
    /// FxCop project files are not supported due to 
    /// loss of source location information
    ///</remarks>
    internal sealed class FxCopConverter : ToolFileConverterBase
    {
        private const string ProjectDirectoryVariable = "$(ProjectDir)";

        public override string ToolName => ToolFormat.FxCop;

        /// <summary>
        /// Convert FxCop log to SARIF format stream
        /// </summary>
        /// <param name="input">FxCop log stream</param>
        /// <param name="output">output stream</param>
        /// <param name="dataToInsert">Optionally emitted properties that should be written to log.</param>
        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            if (input == null)
            {
                throw (new ArgumentNullException(nameof(input)));
            }

            if (output == null)
            {
                throw (new ArgumentNullException(nameof(output)));
            }

            LogicalLocations.Clear();

            var context = new FxCopLogReader.Context();

            var results = new List<Result>();
            var rules = new List<ReportingDescriptor>();
            var reader = new FxCopLogReader();
            reader.RuleRead += (FxCopLogReader.Context current) => { rules.Add(CreateRule(current)); };
            reader.ResultRead += (FxCopLogReader.Context current) => { results.Add(CreateResult(current)); };
            reader.Read(context, input);

            var run = new Run()
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = ToolName,
                        Rules = rules
                    }
                },
            };

            PersistResults(output, results, run);

            output.WriteLogicalLocations(LogicalLocations);
        }

        internal ReportingDescriptor CreateRule(FxCopLogReader.Context context)
        {
            var rule = new ReportingDescriptor
            {
                Id = context.CheckId,
                Name = context.RuleTypeName,
                MessageStrings = context.Resolutions.ConvertToMultiformatMessageStringsDictionary()
            };

            rule.SetProperty("Category", context.RuleCategory);

            return rule;
        }

        internal Result CreateResult(FxCopLogReader.Context context)
        {
            Result result = new Result();

            string uniqueId = context.GetUniqueId();

            if (!string.IsNullOrWhiteSpace(uniqueId))
            {
                if (result.PartialFingerprints == null)
                {
                    result.PartialFingerprints = new Dictionary<string, string>();
                }

                SarifUtilities.AddOrUpdateDictionaryEntry(result.PartialFingerprints, "UniqueId", uniqueId);
            }

            string status = context.Status;

            if ("ExcludedInSource".Equals(status))
            {
                result.Suppressions = new List<Suppression>
                {
                    new Suppression
                    {
                        Kind = SuppressionKind.InSource
                    }
                };

            }
            else if ("ExcludedInProject".Equals(status))
            {
                result.BaselineState = BaselineState.Unchanged;
            }

            result.RuleId = context.CheckId;
            string messageText = context.Message ?? ConverterResources.FxCopNoMessage;
            result.Message = new Message { Arguments = context.Items, Id = context.ResolutionName, Text = messageText };
            var location = new Location();

            string sourceFile = GetFilePath(context);
            string targetFile = context.Target;

            // If both source and target have values and they're different, set analysis target
            if (!string.IsNullOrWhiteSpace(sourceFile) &&
                !string.IsNullOrWhiteSpace(targetFile) &&
                !sourceFile.Equals(targetFile))
            {
                result.AnalysisTarget = BuildFileLocationFromFxCopReference(targetFile);
            }
            else
            {
                // One or the other or both is null, or they're different
                sourceFile = string.IsNullOrWhiteSpace(sourceFile) ? targetFile : sourceFile;
            }

            // Don't emit a location if neither physical location nor logical location information
            // is present. This is the case for CA0001 (unexpected error in analysis tool).
            // https://docs.microsoft.com/en-us/visualstudio/code-quality/ca0001?view=vs-2019
            bool emitLocation = false;

            // If we have a value, set physical location
            if (!string.IsNullOrWhiteSpace(sourceFile))
            {
                location.PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = BuildFileLocationFromFxCopReference(sourceFile),
                    Region = context.Line == null ? null : Extensions.CreateRegion(context.Line.Value)
                };

                emitLocation = true;
            }

            string fullyQualifiedLogicalName = CreateFullyQualifiedLogicalName(context, out int logicalLocationIndex);

            if (!string.IsNullOrWhiteSpace(fullyQualifiedLogicalName) || logicalLocationIndex > -1)
            {
                location.LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = fullyQualifiedLogicalName,
                    Index = logicalLocationIndex
                };

                emitLocation = true;
            }

            if (emitLocation)
            {
                result.Locations = new List<Location> { location };
            }

            bool mapsDirectlyToSarifName;

            result.Level = ConvertFxCopLevelToResultLevel(context.Level ?? "Warning", out mapsDirectlyToSarifName);

            if (!mapsDirectlyToSarifName)
            {
                // We will not recapitulate FxCop MessageLevel names (such as 
                // "Error" and "Warning") as a property. For names that differ
                // (such as "CriticalWarning" and "Information"), we will also 
                // include the FxCop-specific values in the property bag.
                AddProperty(result, context.Level, "Level");
            }

            AddProperty(result, context.Category, "Category");
            AddProperty(result, context.FixCategory, "FixCategory");

            return result;
        }

        private ArtifactLocation BuildFileLocationFromFxCopReference(string fileReference)
        {
            string uriBaseId = null;

            if (fileReference.StartsWith(ProjectDirectoryVariable + "/"))
            {
                uriBaseId = ProjectDirectoryVariable;
                fileReference = fileReference.Substring(ProjectDirectoryVariable.Length + 1);
            }

            return new ArtifactLocation()
            {
                UriBaseId = uriBaseId,
                Uri = new Uri(fileReference, UriKind.RelativeOrAbsolute)
            };
        }

        private static FailureLevel ConvertFxCopLevelToResultLevel(string fxcopLevel, out bool mapsDirectlyToSarifName)
        {
            mapsDirectlyToSarifName = true;

            // Values below derived from definition of FxCop MessageLevel enum
            // Microsoft.VisualStudio.CodeAnalysis.Extensibility.MessageLevel

            switch (fxcopLevel)
            {
                case "Error":
                {
                    return FailureLevel.Error;
                }

                case "CriticalError":
                {
                    mapsDirectlyToSarifName = false;
                    return FailureLevel.Error;
                }

                case "Warning":
                {
                    return FailureLevel.Warning;
                }

                case "CriticalWarning":
                {
                    mapsDirectlyToSarifName = false;
                    return FailureLevel.Warning;
                }

                case "Information":
                {
                    mapsDirectlyToSarifName = false;
                    return FailureLevel.Note;
                }

                default:
                {
                    break;
                }
            }

            // In some circumstances, such as reporting an 'excluded' message,
            // FxCop provides no MessageLevel. For these issues, we shouldn't
            // emit any value at all
            mapsDirectlyToSarifName = false;
            return FailureLevel.None;
        }

        private static string GetFilePath(FxCopLogReader.Context context)
        {
            if (context.Path == null)
            {
                return context.File;
            }
            else if (context.File == null)
            {
                Debug.Fail("FxCop with path set but file unset.");
                return context.Path;
            }
            else
            {
                return Path.Combine(context.Path, context.File);
            }
        }

        private string CreateFullyQualifiedLogicalName(FxCopLogReader.Context context, out int index)
        {
            index = -1;
            string fullyQualifiedName = null;
            string delimiter = string.Empty;

            if (!string.IsNullOrEmpty(context.Module))
            {
                index = AddLogicalLocation(index, ref fullyQualifiedName, context.Module, LogicalLocationKind.Module, delimiter);
                delimiter = "!";
            }

            if (!string.IsNullOrEmpty(context.Resource))
            {
                index = AddLogicalLocation(index, ref fullyQualifiedName, context.Resource, LogicalLocationKind.Resource, delimiter);
                delimiter = ".";
            }

            if (!string.IsNullOrEmpty(context.Namespace))
            {
                index = AddLogicalLocation(index, ref fullyQualifiedName, context.Namespace, LogicalLocationKind.Namespace, delimiter);
                delimiter = ".";
            }

            if (!string.IsNullOrEmpty(context.Type))
            {
                index = AddLogicalLocation(index, ref fullyQualifiedName, context.Type, LogicalLocationKind.Type, delimiter);
                delimiter = ".";
            }

            if (!string.IsNullOrEmpty(context.Member))
            {
                string member = context.Member != null ? context.Member.Trim('#') : null;
                index = AddLogicalLocation(index, ref fullyQualifiedName, member, LogicalLocationKind.Member, delimiter);
            }

            return fullyQualifiedName;
        }

        private int AddLogicalLocation(int parentIndex, ref string fullyQualifiedName, string value, string kind, string delimiter = ".")
        {
            fullyQualifiedName = fullyQualifiedName + delimiter + value;
            var logicalLocation = new LogicalLocation
            {
                FullyQualifiedName = fullyQualifiedName != value ? fullyQualifiedName : null,
                Kind = kind,
                Name = value,
                ParentIndex = parentIndex
            };

            return AddLogicalLocation(logicalLocation);
        }

        private static void AddProperty(Result result, string value, string key)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                result.SetProperty(key, value);
            }
        }
    }

    /// <summary>
    /// Pluggable FxCop log reader
    /// </summary>
    internal sealed class FxCopLogReader
    {
        public delegate void OnRuleRead(Context context);
        public delegate void OnIssueRead(Context context);

        public event OnRuleRead RuleRead;
        public event OnIssueRead ResultRead;

        private bool _readingProjectFile;
        private readonly SparseReaderDispatchTable _dispatchTable;

        private const string FxCopReportSchema = "Microsoft.CodeAnalysis.Sarif.Converters.Schemata.FxCopReport.xsd";

        /// <summary>
        /// Current context of the result 
        /// </summary>
        /// <remarks>
        /// The context accumulates in memory during the streaming,
        /// but the information we gather is very limited,
        /// and there is only one context object per input file
        /// currently constructed
        /// </remarks>
        internal class Context
        {
            public string Report { get; private set; }
            public bool Exception { get; private set; }
            public string ExceptionType { get; private set; }
            public string ExceptionMessage { get; private set; }
            public string StackTrace { get; private set; }
            public string InnerExceptionType { get; private set; }
            public string InnerExceptionMessage { get; private set; }
            public string InnerStackTrace { get; private set; }
            public string ExceptionTarget { get; private set; }
            public string Target { get; private set; }
            public string Module { get; private set; }
            public string Namespace { get; private set; }
            public string Resource { get; private set; }
            public string Member { get; private set; }
            public string Type { get; private set; }
            public string CheckId { get; private set; }
            public string MessageId { get; private set; }
            public string Category { get; private set; }
            public string Typename { get; private set; }
            public string FixCategory { get; private set; }
            public string Status { get; private set; }
            public string Message { get; private set; }
            public IList<string> Items { get; set; }
            public string ResolutionName { get; private set; }
            public string Certainty { get; private set; }
            public string Level { get; private set; }
            public string Path { get; private set; }
            public string File { get; private set; }
            public int? Line { get; private set; }
            public Dictionary<string, string> Resolutions { get; private set; }
            public string RuleTypeName { get; private set; }
            public string RuleCategory { get; private set; }

            // calculate result's unique id based on the current context
            public string GetUniqueId()
            {
                if (Exception)
                {
                    return CreateId(ExceptionTarget, ExceptionType, MessageId, ResolutionName);
                }
                return CreateId(MessageId, ResolutionName);
            }

            private static string CreateId(params string[] parts)
            {
                IEnumerable<string> updated = parts
                    .Where(part => !string.IsNullOrEmpty(part))
                    .Select(part => part.TrimStart('#'));

                return string.Join("#", updated.ToArray());
            }

            public void RefineReport(string report)
            {
                Report = report;
                ClearTarget();
            }

            public void RefineRule(string typeName, string category, string checkId)
            {
                RuleTypeName = typeName;
                RuleCategory = category;
                CheckId = checkId;
                ClearResolutions();
            }

            public void RefineTarget(string target)
            {
                Target = target;
                ClearModule();
                ClearResource();
            }

            public void RefineResource(string resource)
            {
                Resource = resource;
                ClearNamespace();
            }

            public void RefineModule(string module)
            {
                Module = module;
                ClearMessage();
            }

            public void RefineNamespace(string nameSpace)
            {
                Namespace = nameSpace;
                ClearType();
            }

            public void RefineType(string type)
            {
                Type = type;
                ClearMember();
            }

            public void RefineMember(string member)
            {
                Member = member;
                ClearMessage();
            }

            public void RefineMessage(string checkId, string typename, string messageId, string category, string fixcategory, string status)
            {
                CheckId = checkId;
                MessageId = messageId;
                Category = category;
                Typename = typename;
                FixCategory = fixcategory;
                Status = status;

                ClearIssue();
            }

            public void RefineIssue(string message, string result, string certainty, string level, string path, string file, int? line)
            {
                Message = message;
                ResolutionName = result;
                Certainty = certainty;
                Level = level;
                Path = path;
                File = file;
                Line = line;
            }

            public void RefineItem(string item)
            {
                Items = Items ?? new List<string>();
                Items.Add(item);
            }

            public void RefineResolution(string name, string formatString)
            {
                Resolutions = Resolutions ?? new Dictionary<string, string>();
                Resolutions[name] = formatString;
            }

            public void RefineException(bool isException, string checkId, string target)
            {
                ClearTarget();

                Exception = isException;
                CheckId = checkId;
                ExceptionTarget = target;
            }

            public void RefineExceptionType(string type)
            {
                ExceptionType = type;
            }

            public void RefineExceptionMessage(string message)
            {
                ExceptionMessage = message;
            }

            public void RefineStackTrace(string stack)
            {
                StackTrace = stack;
            }

            public void RefineInnerExceptionType(string type)
            {
                InnerExceptionType = type;
            }

            public void RefineInnerExceptionMessage(string message)
            {
                InnerExceptionMessage = message;
            }

            public void RefineInnerStackTrace(string stack)
            {
                InnerStackTrace = stack;
            }

            public void ClearReport()
            {
                RefineReport(null);
            }
            public void ClearException()
            {
                RefineException(false, null, null);
                ExceptionMessage = null;
                ExceptionType = null;
                StackTrace = null;
                InnerExceptionMessage = null;
                InnerExceptionType = null;
                InnerStackTrace = null;

                ClearIssue();
            }

            public void ClearTarget()
            {
                RefineTarget(null);
            }

            public void ClearModule()
            {
                RefineModule(null);
            }

            public void ClearResource()
            {
                RefineResource(null);
            }

            public void ClearNamespace()
            {
                RefineNamespace(null);
            }

            public void ClearType()
            {
                RefineType(null);
            }

            public void ClearMember()
            {
                RefineMember(null);
            }

            public void ClearMessage()
            {
                RefineMessage(null, null, null, null, null, null);
            }

            public void ClearIssue()
            {
                RefineIssue(null, null, null, null, null, null, null);
                Items = null;
            }

            public void ClearRule()
            {
                RefineRule(null, null, null);
            }

            public void ClearResolutions()
            {
                Resolutions = null;
            }
        }

        /// <summary>
        /// FxCop xml elements and attributes
        /// </summary>
        private static class SchemaStrings
        {
            // elements
            public const string ElementFxCopProject = "FxCopProject";
            public const string ElementFxCopReport = "FxCopReport";
            public const string ElementExceptions = "Exceptions";
            public const string ElementException = "Exception";
            public const string ElementExceptionMessage = "ExceptionMessage";
            public const string ElementStackTrace = "StackTrace";
            public const string ElementInnerType = "InnerType";
            public const string ElementInnerExceptionMessage = "InnerExceptionMessage";
            public const string ElementInnerStackTrace = "InnerStackTrace";
            public const string ElementRules = "Rules";
            public const string ElementRule = "Rule";
            public const string ElementTargets = "Targets";
            public const string ElementTarget = "Target";
            public const string ElementModules = "Modules";
            public const string ElementModule = "Module";
            public const string ElementResources = "Resources";
            public const string ElementResource = "Resource";
            public const string ElementNamespaces = "Namespaces";
            public const string ElementNamespace = "Namespace";
            public const string ElementTypes = "Types";
            public const string ElementType = "Type";
            public const string ElementMembers = "Members";
            public const string ElementMember = "Member";
            public const string ElementMessages = "Messages";
            public const string ElementMessage = "Message";
            public const string ElementIssue = "Issue";
            public const string ElementItem = "Item";
            public const string ElementResolution = "Resolution";

            // attributes (report)
            public const string AttributeVersion = "Version";

            // attributes (target + rule)
            public const string AttributeName = "Name";

            // attributes (type)
            public const string AttributeKind = "Kind";
            public const string AttributeAccessibility = "Accessibility";
            public const string AttributeExternallyVisible = "ExternallyVisible";

            // attributes (member)
            public const string AttributeStatic = "Static";

            // attributes (message + rule)
            public const string AttributeId = "Id";
            public const string AttributeTypeName = "TypeName";
            public const string AttributeCategory = "Category";
            public const string AttributeCheckId = "CheckId";
            public const string AttributeStatus = "Status";
            public const string AttributeCreated = "Created";
            public const string AttributeFixCategory = "FixCategory";

            // attributes (result)
            public const string AttributeCertainty = "Certainty";
            public const string AttributeLevel = "Level";
            public const string AttributePath = "Path";
            public const string AttributeFile = "File";
            public const string AttributeLine = "Line";

            // attributes (exception)
            public const string AttributeKeyword = "Keyword";
            public const string AttributeTarget = "Target";

            // enums (exception kind)
            public const string EnumEngine = "Engine";
            public const string EnumRule = "Rule";
        }

        /// <summary>
        /// Constructor to hydrate the private members
        /// </summary>
        public FxCopLogReader()
        {
            _dispatchTable = new SparseReaderDispatchTable
            {
                {SchemaStrings.ElementFxCopReport, ReadFxCopReport},
                {SchemaStrings.ElementExceptions, ReadExceptions},
                {SchemaStrings.ElementException, ReadException},
                {SchemaStrings.ElementExceptionMessage, ReadExceptionMessage},
                {SchemaStrings.ElementStackTrace, ReadStackTrace},
                {SchemaStrings.ElementInnerType, ReadInnerExceptionType},
                {SchemaStrings.ElementInnerExceptionMessage, ReadInnerExceptionMessage},
                {SchemaStrings.ElementInnerStackTrace, ReadInnerStackTrace},
                {SchemaStrings.ElementRules, ReadRules},
                {SchemaStrings.ElementRule, ReadRule},
                {SchemaStrings.ElementResolution, ReadResolution},
                {SchemaStrings.ElementTargets, ReadTargets},
                {SchemaStrings.ElementTarget, ReadTarget},
                {SchemaStrings.ElementResources, ReadResources},
                {SchemaStrings.ElementResource, ReadResource},
                {SchemaStrings.ElementModules, ReadModules},
                {SchemaStrings.ElementModule, ReadModule},
                {SchemaStrings.ElementNamespaces, ReadNamespaces},
                {SchemaStrings.ElementNamespace, ReadNamespace},
                {SchemaStrings.ElementTypes, ReadTypes},
                {SchemaStrings.ElementType, ReadType},
                {SchemaStrings.ElementMembers, ReadMembers},
                {SchemaStrings.ElementMember, ReadMember},
                {SchemaStrings.ElementMessages, ReadMessages},
                {SchemaStrings.ElementMessage, ReadMessage},
                {SchemaStrings.ElementIssue, ReadIssue},
                {SchemaStrings.ElementItem, ReadItem},
            };
        }

        public void Read(Context context, Stream input)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            Assembly assembly = typeof(FxCopLogReader).Assembly;
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            using (Stream stream = assembly.GetManifestResourceStream(FxCopLogReader.FxCopReportSchema))
            using (var reader = XmlReader.Create(stream, settings))
            {
                XmlSchema schema = XmlSchema.Read(reader, new ValidationEventHandler(ReportError));
                schemaSet.Add(schema);
            }

            using (var sparseReader = SparseReader.CreateFromStream(_dispatchTable, input, schemaSet))
            {
                // FxCop distinctions between project and report files.
                // 
                // 1. Project files are designed to be deterministic in output and therefore
                //    do not emit any file locations, only logical locations.
                // 2. Project files do not emit fully-constructed messages, only dynamic
                //    arguments that can be used with rule format strings to construct a message.
                // 3. Project files by default persist excluded message but not absent
                //    messages. Report files by default persist neither excluded or absent
                //    messages.

                if (sparseReader.LocalName.Equals(SchemaStrings.ElementFxCopProject))
                {
                    _readingProjectFile = true;

                    // Skip project information, which should lead us to the report that
                    // holds emitted messages.
                    sparseReader.ReadChildren(SchemaStrings.ElementFxCopProject, context);
                }
                else if (sparseReader.LocalName.Equals(SchemaStrings.ElementFxCopReport))
                {
                    ReadFxCopReport(sparseReader, context);
                }
                else
                {
                    throw new XmlException(string.Format(CultureInfo.InvariantCulture, "Invalid root element in FxCop log file: {0}", sparseReader.LocalName));
                }
            }
        }

        private static void ReportError(object sender, EventArgs e)
        {
            throw new XmlException(e.ToString());
        }

        private static void ReadFxCopReport(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineReport(reader.ReadAttributeString(SchemaStrings.AttributeVersion));
            reader.ReadChildren(SchemaStrings.ElementFxCopReport, parent);
            context.ClearReport();
        }

        private static void ReadExceptions(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementExceptions, parent);
        }

        private void ReadException(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string ruleId = reader.ReadAttributeString(SchemaStrings.AttributeKeyword);
            string kind = reader.ReadAttributeString(SchemaStrings.AttributeKind);
            string checkId = reader.ReadAttributeString(SchemaStrings.AttributeCheckId);
            string target = reader.ReadAttributeString(SchemaStrings.AttributeTarget);

            context.RefineException(true, ruleId, target);

            reader.ReadChildren(SchemaStrings.ElementException, parent);

            string exception = MakeExceptionMessage(kind, checkId, context.ExceptionType, context.ExceptionMessage, context.StackTrace, context.InnerExceptionType, context.InnerExceptionMessage, context.InnerStackTrace);
            context.RefineIssue(exception, null, null, null, null, null, null);

            if (ResultRead != null)
            {
                ResultRead(context);
            }

            context.ClearIssue();
            context.ClearException();
        }

        private static void ReadExceptionType(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            if (context.Exception)
            {
                context.RefineExceptionType(reader.ReadElementContentAsString());
            }
        }

        private static void ReadExceptionMessage(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineExceptionMessage(reader.ReadElementContentAsString());
        }

        private static void ReadStackTrace(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineStackTrace(reader.ReadElementContentAsString());
        }

        private void ReadInnerExceptionType(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineInnerExceptionType(reader.ReadElementContentAsString());
        }

        private static void ReadInnerExceptionMessage(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineInnerExceptionMessage(reader.ReadElementContentAsString());
        }

        private static void ReadInnerStackTrace(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineInnerStackTrace(reader.ReadElementContentAsString());
        }

        private static void ReadResources(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementResources, parent);
        }

        private static void ReadResource(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineResource(reader.ReadAttributeString(SchemaStrings.AttributeName));
            reader.ReadChildren(SchemaStrings.ElementResource, parent);
            context.ClearResource();
        }

        private static void ReadRules(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementRules, parent);
        }

        private void ReadRule(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineRule(
                typeName: reader.ReadAttributeString(SchemaStrings.AttributeTypeName),
                category: reader.ReadAttributeString(SchemaStrings.AttributeCategory),
                checkId: reader.ReadAttributeString(SchemaStrings.AttributeCheckId));

            reader.ReadChildren(SchemaStrings.ElementRule, parent);

            if (RuleRead != null)
            {
                RuleRead(context);
            }

            context.ClearRule();
        }

        private static void ReadResolution(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineResolution(
                name: reader.ReadAttributeString(SchemaStrings.AttributeName),
                formatString: reader.ReadElementContentAsString());
        }

        private static void ReadTargets(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementTargets, parent);
        }

        private static void ReadTarget(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineTarget(reader.ReadAttributeString(SchemaStrings.AttributeName));
            reader.ReadChildren(SchemaStrings.ElementTarget, parent);
            context.ClearTarget();
        }

        private void ReadModules(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementModules, parent);
        }

        private void ReadModule(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineModule(reader.ReadAttributeString(SchemaStrings.AttributeName));
            reader.ReadChildren(SchemaStrings.ElementModule, parent);
            context.ClearModule();
        }

        private static void ReadNamespaces(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementNamespaces, parent);
        }

        private void ReadNamespace(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineNamespace(reader.ReadAttributeString(SchemaStrings.AttributeName));
            reader.ReadChildren(SchemaStrings.ElementNamespace, parent);
            context.ClearNamespace();
        }

        private static void ReadTypes(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementTypes, parent);
        }

        private static void ReadType(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            if (context.Exception)
            {
                ReadExceptionType(reader, parent);
            }
            else
            {
                context.RefineType(reader.ReadAttributeString(SchemaStrings.AttributeName));
                reader.ReadChildren(SchemaStrings.ElementType, parent);
                context.ClearType();
            }
        }

        private static void ReadMembers(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementMembers, parent);
        }

        private static void ReadMember(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            context.RefineMember(reader.ReadAttributeString(SchemaStrings.AttributeName));
            reader.ReadChildren(SchemaStrings.ElementMember, parent);
            context.ClearMember();
        }

        private static void ReadMessages(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementMessages, parent);
        }

        private void ReadMessage(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string messageId = reader.ReadAttributeString(SchemaStrings.AttributeId);
            string typename = reader.ReadAttributeString(SchemaStrings.AttributeTypeName);
            string category = reader.ReadAttributeString(SchemaStrings.AttributeCategory);
            string checkId = reader.ReadAttributeString(SchemaStrings.AttributeCheckId);
            string fixCategory = reader.ReadAttributeString(SchemaStrings.AttributeFixCategory);
            string status = reader.ReadAttributeString(SchemaStrings.AttributeStatus);

            context.RefineMessage(checkId, typename, messageId, category, fixCategory, status);

            if ("Excluded".Equals(status) || "ExcludedInSource".Equals(status))
            {
                // FxCop doesn't actually emit message details for most excluded items
                // and so we must fire here for these items, as the scan for child
                // <Issue> elements may not produce anything. FxCop seems to emit
                // issues for excluded items which are at the namespace level only.
                if (ResultRead != null)
                {
                    ResultRead(context);
                }
            }

            reader.ReadChildren(SchemaStrings.ElementMessage, parent);

            context.ClearMessage();
        }

        private void ReadIssue(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string resolutionName = reader.ReadAttributeString(SchemaStrings.AttributeName);
            string certainty = reader.ReadAttributeString(SchemaStrings.AttributeCertainty);
            string level = reader.ReadAttributeString(SchemaStrings.AttributeLevel);

            string path = reader.ReadAttributeString(SchemaStrings.AttributePath);
            string file = reader.ReadAttributeString(SchemaStrings.AttributeFile);
            int? line = reader.ReadAttributeInt(SchemaStrings.AttributeLine);

            string message = null;

            if (_readingProjectFile)
            {
                // FxCop does not emit a resolution name attribute in cases where it is "Default"
                resolutionName = resolutionName ?? "Default";
                reader.ReadChildren(SchemaStrings.ElementIssue, parent, out message);
                context.RefineIssue(message, message == null ? resolutionName : null, certainty, level, path, file, line);
            }
            else
            {
                // An FxCop project file Issue has a fully-formed output
                // message as its element content.
                message = reader.ReadElementContentAsString();
                context.RefineIssue(message, resolutionName, certainty, level, path, file, line);
            }

            if (ResultRead != null)
            {
                ResultRead(context);
            }

            context.ClearIssue();
        }

        private void ReadItem(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            context.Items = context.Items ?? new List<string>();
            context.Items.Add(reader.ReadElementContentAsString());
        }

        internal static string MakeExceptionMessage(string kind, string checkId, string type, string message, string stackTrace, string innerType, string innerMessage, string innerStackTrace)
        {
            string innerException = string.Empty;
            if (innerType != null)
            {
                innerException = string.Format(CultureInfo.InvariantCulture, " Inner Exception: {0}: {1} {2}", innerType, innerMessage, innerStackTrace);
            }

            if (kind == SchemaStrings.EnumRule)
            {
                return string.Format(CultureInfo.InvariantCulture, "Rule {0} exception: {1}: {2} {3}.{4}", checkId, type, message, stackTrace, innerException);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} exception: {1}: {2} {3}.{4}", kind, type, message, stackTrace, innerException);
            }
        }
    }
}