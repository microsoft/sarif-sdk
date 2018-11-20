// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts exported Contrast Security XML report files to sarif format
    /// </summary>
    ///<remarks>
    ///</remarks>
    internal sealed class ContrastSecurityConverter : ToolFileConverterBase
    {
        private const string ContrastSecurityRulesData = "Microsoft.CodeAnalysis.Sarif.Converters.RulesData.ContrastSecurity.sarif";

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
            reader.FindingRead += (ContrastLogReader.Context current) => { results.AddRange(CreateResult(current)); };
            reader.Read(context, input);

            Assembly assembly = typeof(ContrastSecurityConverter).Assembly;

            SarifLog sarifLog;

            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Newtonsoft.Json.Formatting.Indented                
            };

            using (var stream = assembly.GetManifestResourceStream(ContrastSecurityConverter.ContrastSecurityRulesData))
            using (var streamReader = new StreamReader(stream))
            {
                sarifLog = JsonConvert.DeserializeObject<SarifLog>(streamReader.ReadToEnd(), settings);
            }

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, dataToInsert);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            Run run = sarifLog.Runs[0];

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

        internal IEnumerable<Result> CreateResult(ContrastLogReader.Context context)
        {
            switch (context.RuleId)
            {
                case ContrastSecurityRuleIds.AntiCachingControlsMissing:
                {
                    return ConstructAntiCachingControlsMissingResults(context.Properties);
                }

                case "clickjacking-control-missing":
                {
                    return ConstructClickJackingControlMissingResults(context.Properties);
                }

                case "authorization-missing-deny":
                {
                    return ConstructAuthorizationRulesMissingDenyResults(context.Properties);
                }

                default:
                {
                    return ConstructNotImplementedRuleResult(context.RuleId);
                }
            }

            throw new InvalidOperationException();
        }

        private IEnumerable<Result> ConstructNotImplementedRuleResult(string ruleId)
        {
            Result result = new Result();
            result.RuleId = ruleId;
            result.Message = new Message { Text = "TODO: missing message construction for '" + result.RuleId + "' rule." };
            return new List<Result> { result };
        }
        
        private IEnumerable<Result> ConstructAntiCachingControlsMissingResults(IDictionary<string, string> properties)
        {
            // cache-controls-missing : Anti-Caching Controls Missing

            // default : 'The {0}' page Cache-Control header did not contain 'no-store' or 'no-cache'; The value observed was '{1}'.


            var results = new List<Result>();

            foreach (string key in properties.Keys)
            {
                string value = properties[key];
                var result = new Result() { RuleId = ContrastSecurityRuleIds.AntiCachingControlsMissing };
                result.Locations = new List<Location> {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(key)
                    }};

                result.Message = new Message
                {
                    MessageId = "default",
                    Arguments = new List<string>
                    {
                        key,   // 'The {0}' page Cache-Control header did not contain 'no-store' or 'no-cache';
                        value, //The value observed was '{1}'.
                    }
                };
                results.Add(result);
            }
            return results;
        }

        private IEnumerable<Result> ConstructAuthorizationRulesMissingDenyResults(IDictionary<string, string> properties)
        {
            // authorization-missing-deny : Authorization Rules Missing Deny Rule

            Result result = new Result();
            result.RuleId = ContrastSecurityRuleIds.AuthorizationRulesMissingDenyRule;

            // authorization-missing-deny instances track the following properties:
            // 
            // <properties name="path">\web.config</properties>
            // <properties name="locationPath">CustomerLogin.aspx</properties>
            // <properties name="snippet">10:     &lt;system.web&gt;&#xD;</properties>

            string path = properties[nameof(path)];
            string locationPath = properties.ContainsKey(nameof(locationPath)) ? properties[nameof(locationPath)] : null;
            string snippet = properties[nameof(snippet)];

            IList<Location> locations = new List<Location>();
            locations.Add(new Location
            {
                PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
            });

            result.Locations = locations;

            if (locationPath == null)
            {
                result.Message = new Message
                {
                    MessageId = "default",
                    Arguments = new List<string>
                    {                  // The configuraiton in 
                        path,          // '{0}' is missing a <deny> rule in the <authorization> section.
                    }
                };
            }
            else
            {
                result.Message = new Message
                {
                    MessageId = "underLocation",
                    Arguments = new List<string>
                    {                  // The configuration under location 
                        locationPath,  // '{0}' in 
                        path,          // '{1}' is missing a <deny> rule in the <authorization> section.
                    }
                };
            }

            return new List<Result> { result };
        }

        private IEnumerable<Result> ConstructClickJackingControlMissingResults(IDictionary<string, string> properties)
        {
            // cache-controls-missing : Anti-Caching Controls Missing

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">1536704253063</properties>
            // <properties name="/webgoat/WebGoatCoins/MainPage.aspx">1536704186306</properties>

            var results = new List<Result>();

            foreach (string key in properties.Keys)
            {
                var result = new Result() { RuleId = ContrastSecurityRuleIds.PagesWithoutAntiClickjackingControls };
                result.Locations = new List<Location> {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(key)
                    }};

                // TODO identify exact nature of the value, which looks like an id for some kind of cache
                result.SetProperty("id", properties[key]);

                result.Message = new Message
                {
                    MessageId = "default",
                    Arguments = new List<string>
                    {
                        key // '{0}' has insufficient anti-clickjacking controls.
                    }
                };
                results.Add(result);
            }
            return results;
        }

        private Region CreateRegion(string snippet)
        {
            int? startLine = null;
            int endLine = 0;

            snippet = NaiveXmlDecode(snippet);

            string[] lines = snippet.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();

            foreach (string line in lines)
            {
                string[] lineTokens = line.Split(':');
                
                if (startLine == null)
                {
                    startLine = int.Parse(lineTokens[0]);
                    endLine = startLine.Value;
                }
                else
                {
                    endLine++;
                }
                sb.AppendLine(lineTokens[1]);                
            }

            return new Region()
            {
                StartLine = startLine.Value,
                EndLine = endLine,
                Snippet = new FileContent { Text = sb.ToString() }
            };
        }

        private string NaiveXmlDecode(string snippet)
        {
            return snippet.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
        }

        private PhysicalLocation CreatePhysicalLocation(string uri, Region contextRegion = null)
        {
            return new PhysicalLocation
            {
                FileLocation = new FileLocation
                {
                    Uri = new Uri(uri, UriKind.Relative),
                    UriBaseId = "SITE_ROOT"                    
                },
                ContextRegion = contextRegion
            };
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
            public string RuleId { get; set; }

            public string RequestMethod { get; set; }

            public string RequestUri { get; set; }

            // Holds properties produced by both the <props> and
            // <properties> elements within Contrast XML
            public IDictionary<string, string> Properties { get; set; }

            // One variety of Contrast XML property
            // persists keys and values as distinct
            // elements. The following properties
            // track this mechanism.
            public string PropertyKey { get; set; }

            public string PropertyValue { get; set; }
            

            internal void RefineFinding(string ruleId)
            {
                RuleId = ruleId;
                ClearProperties();
            }

            internal void RefineRequest(string uri, string method)
            {
                RequestUri = uri;
                RequestMethod = method;
            }

            internal void RefineProperties(string key, string value)
            {
                if (key == null && value == null)
                {
                    Properties = null;
                    PropertyKey = null;
                    PropertyValue = null;
                    return;
                }

                Properties = Properties ?? new Dictionary<string, string>();
                Properties.Add(key, value);
            }

            internal void RefineKey(string key)
            {
            }


            internal void ClearFinding()
            {
                RefineFinding(null);
            }

            internal void ClearRequest()
            {
                RefineRequest(null, null);
            }

            internal void ClearProperties()
            {
                RefineProperties(null, null);
            }
        }

        /// <summary>
        /// COntrast Security exported xml elements and attributes
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
            public const string AttributeName = "name";
            public const string AttributeUri = "uri";
        }

        // Flag used to distinguish between reading both types of XML-persisted
        // property bags. We need to distinguish these cases because they share
        // use of an element named <properties>. There isn't a way to use the 
        // reader to infer the situation (by examining whether it has children,
        // for example) due to the parsing mechanism we're using. 
        //
        // These appear as direct child elements of a finding and store per-rule data:
        //   <props>
        //     <properties name = "someKey" >someValue</properties>
        //   </props>
        // 
        // These are persisted to propagation-event and method-event elements:
        //   <properties>
        //     <p>
        //       <k>someKey</k>
        //       <v>someValue</v>
        //     </p>
        //   </properties>
        //
        private bool _readingProps;

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

            if (FindingRead != null)
            {
                FindingRead(context);
            }

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

        private void ReadProperties(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            if (!_readingProps)
            {
                // We are reading properties within a propagation or method event, e.g.,
                //   <properties>
                //     <p>
                //       <k>someKey</k>
                //       <v>someValue</v>
                //     </p>
                //   </properties>
                reader.ReadChildren(SchemaStrings.ElementProperties, parent);
            }
            else
            {
                // We are reading a properties child element of a finding
                //   <props>
                //     <properties name = "someKey" >someValue</properties>
                //   </props>
                string key = reader.ReadAttributeString(SchemaStrings.AttributeName);
                string value = reader.ReadElementContentAsString();
                context.RefineProperties(key, value);
            }
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

        private void ReadProps(SparseReader reader, object parent)
        {
            _readingProps = true;
            reader.ReadChildren(SchemaStrings.ElementProps, parent);
            _readingProps = false;
        }
    }
}