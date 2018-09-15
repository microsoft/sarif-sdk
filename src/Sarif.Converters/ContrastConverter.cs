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
    /// Converts exported Contrast Security XML report files to sarif format
    /// </summary>
    ///<remarks>
    ///</remarks>
    internal sealed class ContrastConverter : ToolFileConverterBase
    {
        /// <summary>
        /// Convert Contrast Security log to SARIF format stream
        /// </summary>
        /// <param name="input">Contrast log stream</param>
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

            LogicalLocationsDictionary.Clear();

            var context = new  ContrastLogReader.Context();

            var results = new List<Result>();
            var rules = new List<Rule>();
            var reader = new ContrastLogReader();
            reader.FindingRead += (ContrastLogReader.Context current) => { results.Add(CreateResult(current)); };
            reader.Read(context, input);

            Tool tool = new Tool
            {
                Name = "ContrastSecurity"
            };

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, dataToInsert);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            var run = new Run()
            {
                Tool = tool
            };

            output.Initialize(run);

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

            if (rules.Count > 0)
            {
                var rulesDictionary = new Dictionary<string, IRule>();

                foreach (Rule rule in rules)
                {
                    rulesDictionary[rule.Id] = rule;
                }

                output.WriteRules(rulesDictionary);
            }
        }

        internal Result CreateResult(ContrastLogReader.Context context)
        {
            Result result = new Result();

            result.RuleId = context.CheckId;
            result.Message = new Message { Text = "Unknown" };
            return result;
        }

        private static void AddProperty(Result result, string value, string key)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                result.SetProperty(key, value);
            }
        }
    }

    /// <summary>
    /// Pluggable FxCop log reader
    /// </summary>
    internal sealed class ContrastLogReader
    {
        public delegate void OnFindingRead(Context context);

#pragma warning disable CS0067
        public event OnFindingRead FindingRead;
#pragma warning restore

        private readonly SparseReaderDispatchTable _dispatchTable;

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

            public string CheckId { get; set; }
            public string RequestMethod { get; set; }
            public string RequestUri { get; set; }

            public void RefineFindings()
            {
            }

            public void ClearFindings()
            {

            }

            internal void RefineFinding(string ruleId)
            {
            }

            internal void RefineRequest(string uri, string method)
            {
                RequestUri = uri;
                RequestMethod = method;
            }


            internal void ClearFinding()
            {
            }

            internal void ClearRequest()
            {
                RefineRequest(null, null);
            }
        }

        /// <summary>
        /// FxCop xml elements and attributes
        /// </summary>
        private static class SchemaStrings
        {
            // elements
            public const string ElementFindings = "findings";
            public const string ElementFinding = "finding";
            public const string ElementRequest = "request";
            public const string ElementBody = "body";
            public const string ElementHeaders = "headers";
            public const string ElementH = "h";
            public const string ElementParameters = "parameters";
            public const string ElementEvents = "events";
            public const string ElementPropagationEvent = "propagation-event";
            public const string ElementSignature = "signature";
            public const string ElementObject = "obj";
            public const string ElementArgs = "args";
            public const string ElementProperties = "properties";
            public const string ElementP = "P";
            public const string ElementReturn = "return";
            public const string ElementStack = "stack";
            public const string ElementFrame = "frame";
            public const string ElementMethodEvent = "method-event";
            public const string ElementProps = "props";

            // attributes
            public const string AttributeRuleId = "ruleId";
            public const string AttributeMethod = "method";
            public const string AttributeUri = "uri";
        }

        /// <summary>
        /// Constructor to hydrate the private members
        /// </summary>
        public ContrastLogReader()
        {
            _dispatchTable = new SparseReaderDispatchTable
            {
                { SchemaStrings.ElementFindings, ReadFindings},
                { SchemaStrings.ElementFinding, ReadFinding},
                { SchemaStrings.ElementRequest, ReadRequest},
                { SchemaStrings.ElementBody, ReadBody},
                { SchemaStrings.ElementHeaders, ReadHeaders},
                { SchemaStrings.ElementH, ReadH},
                { SchemaStrings.ElementParameters, ReadParameters},
                { SchemaStrings.ElementEvents, ReadEvents},
                { SchemaStrings.ElementPropagationEvent, ReadPropagationEvent},
                { SchemaStrings.ElementSignature, ReadSignature},
                { SchemaStrings.ElementObject, ReadObject},
                { SchemaStrings.ElementArgs, ReadArgs},
                { SchemaStrings.ElementProperties, ReadProperties},
                { SchemaStrings.ElementP, ReadP},
                { SchemaStrings.ElementReturn, ReadReturn},
                { SchemaStrings.ElementStack, ReadStack},
                { SchemaStrings.ElementFrame, ReadFrame},
                { SchemaStrings.ElementMethodEvent, ReadMethodEvent},
                { SchemaStrings.ElementProps, ReadProps},
            };
        }

        public void Read(Context context, Stream input)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            Assembly assembly = typeof(ContrastLogReader).Assembly;
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            //using (var stream = assembly.GetManifestResourceStream(ContrastLogReader.ContrastSecurityReportSchema))
            //using (var reader = XmlReader.Create(stream, settings))
            //{
            //    XmlSchema schema = XmlSchema.Read(reader, new ValidationEventHandler(ReportError));
            //    schemaSet.Add(schema);
            //}

            using (var sparseReader = SparseReader.CreateFromStream(_dispatchTable, input, schemaSet: null))
            {
                if (sparseReader.LocalName.Equals(SchemaStrings.ElementFindings))
                {
                    ReadFindings(sparseReader, context);
                }
                else
                {
                    throw new XmlException(String.Format(CultureInfo.InvariantCulture, "Invalid root element in Contrast Security log file: {0}", sparseReader.LocalName));
                }
            }
        }

        private static void ReportError(object sender, EventArgs e)
        {
            throw new XmlException(e.ToString());
        }

        private static void ReadFindings(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            reader.ReadChildren(SchemaStrings.ElementFindings, parent);
        }

  
        private void ReadFinding(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string ruleId = reader.ReadAttributeString(SchemaStrings.AttributeRuleId);

            context.RefineFinding(ruleId);            

            reader.ReadChildren(SchemaStrings.ElementFinding, parent);

            context.ClearFinding();
        }

        private static void ReadRequest(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string uri= reader.ReadAttributeString(SchemaStrings.AttributeUri);
            string method = reader.ReadAttributeString(SchemaStrings.AttributeMethod);

            context.RefineRequest(uri, method);

            reader.ReadChildren(SchemaStrings.ElementRequest, parent);

            context.ClearRequest();
        }

        private static void ReadBody(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementBody, parent);
        }

        private static void ReadHeaders(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementHeaders, parent);
        }

        private static void ReadH(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementH, parent);
        }

        private static void ReadParameters(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementParameters, parent);
        }

        private static void ReadEvents(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementEvents, parent);
        }

        private static void ReadPropagationEvent(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementPropagationEvent, parent);
        }

        private static void ReadSignature(SparseReader reader, object parent)
        {
            reader.Skip();
        }

        private static void ReadObject(SparseReader reader, object parent)
        {
            reader.Skip();
        }

        private static void ReadArgs(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementArgs, parent);
        }

        private static void ReadProperties(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementProperties, parent);
        }

        private static void ReadP(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementP, parent);
        }

        private static void ReadReturn(SparseReader reader, object parent)
        {
            reader.Skip();
        }

        private static void ReadStack(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementStack, parent);
        }

        private static void ReadFrame(SparseReader reader, object parent)
        {
            reader.Skip();
        }

        private static void ReadMethodEvent(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementMethodEvent, parent);
        }

        private static void ReadProps(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementProps, parent);
        }
    }
}