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
using Microsoft.CodeAnalysis.Sarif.Writers;

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
        /// <summary>
        /// Convert FxCop log to SARIF format stream
        /// </summary>
        /// <param name="input">FxCop log stream</param>
        /// <param name="output">output stream</param>
        public override void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw (new ArgumentNullException("input"));
            }

            if (output == null)
            {
                throw (new ArgumentNullException("output"));
            }

            LogicalLocationsDictionary.Clear();

            var context = new FxCopLogReader.Context();

            var results = new List<Result>();
            var reader = new FxCopLogReader();
            reader.ResultRead += (FxCopLogReader.Context current) => { results.Add(CreateResult(current)); };
            reader.Read(context, input);

            Tool tool = new Tool
            {
                Name = "FxCop"
            };

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension);
            Dictionary<string, IList<FileData>> fileDictionary = fileInfoFactory.Create(results);

            output.WriteTool(tool);

            if (fileDictionary != null && fileDictionary.Any())
            {
                output.WriteFiles(fileDictionary);
            }

            if (LogicalLocationsDictionary != null && LogicalLocationsDictionary.Any())
            {
                output.WriteLogicalLocations(LogicalLocationsDictionary);
            }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        internal Result CreateResult(FxCopLogReader.Context context)
        {
            Result result = new Result();

            string uniqueId = context.GetUniqueId();
            if (!String.IsNullOrWhiteSpace(uniqueId))
            {
                result.ToolFingerprint = uniqueId;
            }

            string status = context.Status;

            if ("ExcludedInSource".Equals(status))
            {
                result.SuppressionStates = new List<SuppressionStatus>();
                result.SuppressionStates.Add(SuppressionStatus.SuppressedInSource);
            }
            else if ("ExcludedInProject".Equals(status))
            {
                result.SuppressionStatus = SuppressionStatus.SuppressedInBaseline;
            }

            result.RuleId = context.CheckId;
            result.Message = context.Message;
            var location = new Location();

            if (!String.IsNullOrEmpty(context.Target))
            {
                location.AnalysisTarget = new PhysicalLocation
                {
                    Uri = new Uri(context.Target, UriKind.RelativeOrAbsolute)
                };

            }

            string sourceFile = GetFilePath(context);
            if (!String.IsNullOrWhiteSpace(sourceFile))
            {
                location.ResultFile = new PhysicalLocation
                {
                    Uri = new Uri(sourceFile, UriKind.RelativeOrAbsolute),
                    Region = context.Line == null ? null : Extensions.CreateRegion(context.Line.Value)
                };
            }

            location.FullyQualifiedLogicalName = CreateSignature(context);

            IList<LogicalLocationComponent> logicalLocationComponents = CreateLogicalLocationComponents(context);

            if (logicalLocationComponents.Any())
            {
                AddLogicalLocation(location, logicalLocationComponents);
            }

            result.Locations = new HashSet<Location> { location };

            var properties = new Dictionary<string, string>();
            TryAddProperty(properties, context.Level, "Level");
            TryAddProperty(properties, context.Category, "Category");
            TryAddProperty(properties, context.FixCategory, "FixCategory");
            if (properties.Count != 0)
            {
                result.Properties = properties;
            }

            return result;
        }

        private static string CreateSignature(FxCopLogReader.Context context)
        {
            if (!String.IsNullOrEmpty(context.Resource))
            {
                return context.Resource;
            }

            string[] parts = new string[] { context.Namespace, context.Type, context.Member };
            var updated = parts
                    .Where(part => !String.IsNullOrEmpty(part))
                    .Select(part => part.TrimStart('#'));

            string joinedParts = String.Join(".", updated);

            if (String.IsNullOrEmpty(joinedParts))
            {
                return context.Module;
            }
            else
            {
                return joinedParts;
            }
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

        private IList<LogicalLocationComponent> CreateLogicalLocationComponents(FxCopLogReader.Context context)
        {
            var logicalLocationComponents = new List<LogicalLocationComponent>();

            TryAddLogicalLocationComponent(logicalLocationComponents, context.Module, LogicalLocationKind.Module);
            TryAddLogicalLocationComponent(logicalLocationComponents, context.Resource, LogicalLocationKind.Resource);
            TryAddLogicalLocationComponent(logicalLocationComponents, context.Namespace, LogicalLocationKind.Namespace);
            TryAddLogicalLocationComponent(logicalLocationComponents, context.Type, LogicalLocationKind.Type);
            TryAddLogicalLocationComponent(logicalLocationComponents, context.Member, LogicalLocationKind.Member);

            return logicalLocationComponents;
        }

        private static void TryAddLogicalLocationComponent(List<LogicalLocationComponent> location, string value, string kind)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                location.Add(new LogicalLocationComponent
                {
                    Name = value,
                    Kind = kind
                });
            }
        }

        private static void TryAddProperty(Dictionary<string, string> properties, string value, string key)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                properties.Add(key, value);
            }
        }
    }

    /// <summary>
    /// Pluggable FxCop log reader
    /// </summary>
    internal sealed class FxCopLogReader
    {
        public delegate void OnIssueRead(Context context);

        public event OnIssueRead ResultRead;

        private readonly SparseReaderDispatchTable _dispatchTable;

        private const string FxCopReportSchema = "Microsoft.CodeAnalysis.Sarif.Schemata.FxCopReport.xsd";

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
            public string Result { get; private set; }
            public string Certainty { get; private set; }
            public string Level { get; private set; }
            public string Path { get; private set; }
            public string File { get; private set; }
            public int? Line { get; private set; }

            // calculate result's unique id based on the current context
            public string GetUniqueId()
            {
                if (Exception)
                {
                    return CreateId(ExceptionTarget, ExceptionType, MessageId, Result);
                }
                return CreateId(MessageId, Result);
            }

            private static string CreateId(params string[] parts)
            {
                var updated = parts
                    .Where(part => !String.IsNullOrEmpty(part))
                    .Select(part => part.TrimStart('#'));

                return String.Join("#", updated.ToArray());
            }

            public void RefineReport(string report)
            {
                Report = report;
                ClearTarget();
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
                Result = result;
                Certainty = certainty;
                Level = level;
                Path = path;
                File = file;
                Line = line;
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
            }
        }

        /// <summary>
        /// FxCop xml elements and attributes
        /// </summary>
        private static class SchemaStrings
        {
            // elements
            public const string ElementFxCopReport = "FxCopReport";
            public const string ElementExceptions = "Exceptions";
            public const string ElementException = "Exception";
            public const string ElementExceptionMessage = "ExceptionMessage";
            public const string ElementStackTrace = "StackTrace";
            public const string ElementInnerType = "InnerType";
            public const string ElementInnerExceptionMessage = "InnerExceptionMessage";
            public const string ElementInnerStackTrace = "InnerStackTrace";
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

            // attributes (repot)
            public const string AttributeVersion = "Version";

            // attributes (target)
            public const string AttributeName = "Name";

            // attributes (type)
            public const string AttributeKind = "Kind";
            public const string AttributeAccessibility = "Accessibility";
            public const string AttributeExternallyVisible = "ExternallyVisible";

            // attributes (member)
            public const string AttributeStatic = "Static";

            // attributes (message)
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
                {SchemaStrings.ElementIssue, ReadIssue}
            };
        }

        public void Read(Context context, Stream input)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            Assembly assembly = typeof(FxCopLogReader).Assembly;

            using (var stream = assembly.GetManifestResourceStream(FxCopLogReader.FxCopReportSchema))
            using (var reader = XmlReader.Create(stream))
            {
                XmlSchema schema = XmlSchema.Read(reader, new ValidationEventHandler(ReportError));
                schemaSet.Add(schema);
            }

            using (var sparseReader = SparseReader.CreateFromStream(_dispatchTable, input, schemaSet))
            {
                if (sparseReader.LocalName.Equals(SchemaStrings.ElementFxCopReport))
                {
                    ReadFxCopReport(sparseReader, context);
                }
                else
                {
                    throw new XmlException(String.Format(CultureInfo.InvariantCulture, "Invalid root element in FxCop log file: {0}", sparseReader.LocalName));
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

        private static void ReadTargets(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

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

        private static void ReadNamespace(SparseReader reader, object parent)
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

        private static void ReadMessage(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string messageId = reader.ReadAttributeString(SchemaStrings.AttributeId);
            string typename = reader.ReadAttributeString(SchemaStrings.AttributeTypeName);
            string category = reader.ReadAttributeString(SchemaStrings.AttributeCategory);
            string checkId = reader.ReadAttributeString(SchemaStrings.AttributeCheckId);
            string fixCategory = reader.ReadAttributeString(SchemaStrings.AttributeFixCategory);
            string status = reader.ReadAttributeString(SchemaStrings.AttributeStatus);

            context.RefineMessage(checkId, typename, messageId, category, fixCategory, status);
            reader.ReadChildren(SchemaStrings.ElementMessage, parent);
            context.ClearMessage();
        }

        private void ReadIssue(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string name = reader.ReadAttributeString(SchemaStrings.AttributeName);
            string certainty = reader.ReadAttributeString(SchemaStrings.AttributeCertainty);
            string level = reader.ReadAttributeString(SchemaStrings.AttributeLevel);

            string path = reader.ReadAttributeString(SchemaStrings.AttributePath);
            string file = reader.ReadAttributeString(SchemaStrings.AttributeFile);
            int? line = reader.ReadAttributeInt(SchemaStrings.AttributeLine);

            string message = reader.ReadElementContentAsString();

            context.RefineIssue(message, name, certainty, level, path, file, line);

            if (ResultRead != null)
            {
                ResultRead(context);
            }

            context.ClearIssue();
        }

        internal static string MakeExceptionMessage(string kind, string checkId, string type, string message, string stackTrace, string innerType, string innerMessage, string innerStackTrace)
        {
            string innerException = String.Empty;
            if (innerType != null)
            {
                innerException = String.Format(CultureInfo.InvariantCulture, " Inner Exception: {0}: {1} {2}", innerType, innerMessage, innerStackTrace);
            }

            if (kind == SchemaStrings.EnumRule)
            {
                return String.Format(CultureInfo.InvariantCulture, "Rule {0} exception: {1}: {2} {3}.{4}", checkId, type, message, stackTrace, innerException);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "{0} exception: {1}: {2} {3}.{4}", kind, type, message, stackTrace, innerException);
            }
        }
    }
}
