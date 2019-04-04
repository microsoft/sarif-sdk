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
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts exported Contrast Security XML report files to SARIF format.
    /// </summary>
    internal sealed class ContrastSecurityConverter : ToolFileConverterBase
    {
        private const string ContrastSecurityRulesData = "Microsoft.CodeAnalysis.Sarif.Converters.RulesData.ContrastSecurity.sarif";

        private IDictionary<string, ReportingDescriptor> _rules;
        private HashSet<Artifact> _files;

        public override string ToolName => "Contrast Security";

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

            LogicalLocations.Clear();

            _files = new HashSet<Artifact>(Artifact.ValueComparer);

            var context = new ContrastLogReader.Context();

            // 1. First we initialize our global rules data from the SARIF we have embedded as a resource
            Assembly assembly = typeof(ContrastSecurityConverter).Assembly;
            SarifLog sarifLog;

            using (var stream = assembly.GetManifestResourceStream(ContrastSecurityRulesData))
            using (var streamReader = new StreamReader(stream))
            {
                string prereleaseRuleDataLogText = streamReader.ReadToEnd();
                PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(prereleaseRuleDataLogText, Newtonsoft.Json.Formatting.Indented, out string currentRuleDataLogText);
                sarifLog = JsonConvert.DeserializeObject<SarifLog>(currentRuleDataLogText);
            }

            // 2. Retain a pointer to the rules dictionary, which we will use to set rule severity
            Run run = sarifLog.Runs[0];
            _rules = run.Tool.Driver.Rules.ToDictionary(rule => rule.Id);

            run.OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                {  "SITE_ROOT", new ArtifactLocation { Uri = new Uri(@"E:\src\WebGoat.NET") } } 
            };

            // 3. Now, parse all the contrast XML to create the complete results set
            var results = new List<Result>();
            var reader = new ContrastLogReader();
            reader.FindingRead += (ContrastLogReader.Context current) => { results.Add(CreateResult(current)); };
            reader.Read(context, input);

            // 4. Construct the files array, based on all results returned
            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, dataToInsert);
            HashSet<Artifact> files = fileInfoFactory.Create(results);

            // 5. Finally, complete the SARIF log file with various tables and then the results
            output.Initialize(run);

            foreach (Artifact fileData in _files)
            {
                files.Add(fileData);
            }

            if (files?.Any() == true)
            {
                output.WriteArtifacts(files.ToList());
            }

            if (LogicalLocations?.Any() == true)
            {
                output.WriteLogicalLocations(LogicalLocations);
            }

            //if (_rules?.Any() == true)
            //{
            //    output.WriteRules(_rules.Values.ToList());
            //}

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        internal Result CreateResult(ContrastLogReader.Context context)
        {
            switch (context.RuleId)
            {
                case ContrastSecurityRuleIds.AntiCachingControlsMissing:
                {
                    return ConstructAntiCachingControlsMissingResult(context.Properties);
                }

                case ContrastSecurityRuleIds.AuthorizationRulesMissingDenyRule:
                {
                    return ConstructAuthorizationRulesMissingDenyResult(context.Properties);
                }

                case ContrastSecurityRuleIds.BadMessageAuthenticationCode:
                {
                    return ConstructBadMessageAuthenticationCodeResult(context.Properties);
                }

                case ContrastSecurityRuleIds.CrossSiteScripting:
                {
                    return ConstructCrossSiteScriptingResult(context);
                }

                case ContrastSecurityRuleIds.DetailedErrorMessagesDisplayed:
                {
                    return ConstructDetailedErrorMessagesDisplayedResult(context.Properties);
                }

                case ContrastSecurityRuleIds.EventValidationDisabled:
                {
                    return ConstructEventValidationDisabledResult(context.Properties);
                }

                case ContrastSecurityRuleIds.FormsAuthenticationSSL:
                {
                    return ConstructFormsAuthenticationSSLResult(context.Properties);
                }

                case ContrastSecurityRuleIds.FormsWithoutAutocompletePrevention:
                {
                    return ConstructFormsWithoutAutocompletePreventionResult(context.Properties);
                }

                case ContrastSecurityRuleIds.HttpOnlyCookieFlagDisabled:
                {
                    return ConstructHttpOnlyCookieFlagDisabledResult(context.Properties);
                }

                case ContrastSecurityRuleIds.InsecureEncryptionAlgorithms:
                {
                    return ConstructInsecureEncryptionAlgorithmsResult(context.Properties);
                }

                case ContrastSecurityRuleIds.OverlyLongSessionTimeout:
                {
                    return ConstructOverlyLongSessionTimeoutResult(context.Properties);
                }

                case ContrastSecurityRuleIds.PagesWithoutAntiClickjackingControls:
                {
                    return ConstructPagesWithoutAntiClickjackingControlsResult(context.Properties);
                }

                case ContrastSecurityRuleIds.PathTraversal:
                {
                    return ConstructPathTraversalResult(context.Properties);
                }

                case ContrastSecurityRuleIds.RequestValidationModeDisabled:
                {
                    return ConstructRequestValidationModeDisabledResult(context.Properties);
                }

                case ContrastSecurityRuleIds.SessionCookieHasNoSecureFlag:
                {
                    return ConstructSessionCookieHasNoSecureFlagResult(context.Properties);
                }

                case ContrastSecurityRuleIds.SessionRewriting:
                {
                    return ConstructSessionRewritingResult(context.Properties);
                }

                case ContrastSecurityRuleIds.SqlInjection:
                {
                    return ConstructSqlInjectionResult(context);
                }

                case ContrastSecurityRuleIds.VersionHeaderEnabled:
                {
                    return ConstructVersionHeaderEnabledResult(context.Properties);
                }

                case ContrastSecurityRuleIds.WebApplicationDeployedinDebugMode:
                {
                    return ConstructWebApplicationDeployedinDebugModeResult(context.Properties);
                }

                default:
                {
                    return ConstructNotImplementedRuleResult(context.RuleId);
                }
            }
        }

        private Result ConstructNotImplementedRuleResult(string ruleId)
        {
            var result = new Result
            {
                Level = GetRuleFailureLevel(ruleId),
                RuleId = ruleId,
                Message = new Message { Text = $"TODO: missing message construction for rule '{ruleId}'." }
            };

            return result;
        }

        private Result ConstructAntiCachingControlsMissingResult(IDictionary<string, string> properties)
        {
            // cache-controls-missing : Anti-Caching Controls Missing

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">{"Header:Cache-Control":"private"}</properties>
            // <properties name="/webgoat/WebGoatCoins/MainPage.aspx">{"Header:Cache-Control":"private"}</properties>

            var locations = new List<Location>();

            string examplePage = null;
            string exampleHeader = null;

            foreach (string key in properties.Keys)
            {
                if (KeyIsReservedPropertyName(key)) { continue; }
                string value = properties[key];
                locations.Add(new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(key)
                });

                examplePage = examplePage ?? key;
                exampleHeader = exampleHeader ?? properties[key];
            }

            string pageCount = locations.Count.ToString();

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.AntiCachingControlsMissing),
                RuleId = ContrastSecurityRuleIds.AntiCachingControlsMissing,
                Locations = locations,
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {
                        pageCount,     // {0} page Cache-Control header(s) did not contain 'no-store' or 'no-cache';
                        examplePage,   // e.g., the value in page '{1}' 
                        exampleHeader  // was observed to be '{2}'.
                    }
                }
            };

            return result;
        }

        private Result ConstructAuthorizationRulesMissingDenyResult(IDictionary<string, string> properties)
        {
            // authorization-missing-deny : Authorization Rules Missing Deny Rule

            var result = new Result
            {
                RuleId = ContrastSecurityRuleIds.AuthorizationRulesMissingDenyRule
            };

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
            result.Level = GetRuleFailureLevel(ContrastSecurityRuleIds.AuthorizationRulesMissingDenyRule);

            if (locationPath == null)
            {
                result.Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // The configuration in 
                        path,          // '{0}' is missing a <deny> rule in the <authorization> section.
                    }
                };
            }
            else
            {
                result.Message = new Message
                {
                    Id = "underLocation",
                    Arguments = new List<string>
                    {                  // The configuration under location 
                        locationPath,  // '{0}' in 
                        path,          // '{1}' is missing a <deny> rule in the <authorization> section.
                    }
                };
            }

            return result;
        }
        private Result ConstructBadMessageAuthenticationCodeResult(IDictionary<string, string> properties)
        {
            var result = new Result
            {
                RuleId = ContrastSecurityRuleIds.BadMessageAuthenticationCode
            };

            return result;
        }

        private Result ConstructPagesWithoutAntiClickjackingControlsResult(IDictionary<string, string> properties)
        {
            // cache-controls-missing : Anti-Caching Controls Missing

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">1536704253063</properties>
            // <properties name="/webgoat/WebGoatCoins/MainPage.aspx">1536704186306</properties>

            var locations = new List<Location>();

            foreach (string key in properties.Keys)
            {
                if (KeyIsReservedPropertyName(key)) { continue; }
                string value = properties[key];
                locations.Add(new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(key)
                });
            }

            string pageCount = locations.Count.ToString();
            string examplePage = locations[0].PhysicalLocation.ArtifactLocation.Uri.ToString();

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.PagesWithoutAntiClickjackingControls),
                RuleId = ContrastSecurityRuleIds.PagesWithoutAntiClickjackingControls,
                Locations = locations,
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {
                        pageCount,  // {0} page(s) have insufficient anti-clickjacking controls, 
                        examplePage // e.g., '{1}' 
                    },
                }
            };

            return result;
        }

        private Result ConstructCrossSiteScriptingResult(ContrastLogReader.Context context)
        {
            // reflected-xss : Cross-Site Scripting

            // default : A cross-site scripting vulnerability was observed from '{0}' on '{1}' page.

            // <properties name="controlID">lblOutput</properties>
            // <properties name="platform">ASP.NET Web Forms</properties>
            // <properties name="webforms-page">OWASP.WebGoat.NET.ReflectedXSS</properties>
            // <properties name="route-signature">OWASP.WebGoat.NET.ReflectedXSS</properties>

            string untrustedData = BuildSourcesString(context.Sources);
            string page = context.RequestUri;
            string caller = context.PropagationEvents[context.PropagationEvents.Count - 1].Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;
            string controlID = context.Properties.ContainsKey(nameof(controlID)) ? context.Properties[nameof(controlID)] : null;

            Result result;

            result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.CrossSiteScripting),
                RuleId = ContrastSecurityRuleIds.CrossSiteScripting,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(page),
                    }
                }
            };

            if (controlID == null)
            {
                result.Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // A cross-site scripting vulnerability was seen as untrusted data
                        untrustedData, // '{0}' on 
                        page,          // '{1}' was observed going into the HTTP response without validation or encoding.
                    }
                };
            }
            else
            {
                result.Message = new Message
                {
                    Id = "hasControlId",
                    Arguments = new List<string>
                    {                  // A cross-site scripting vulnerability was seen as untrusted data
                        untrustedData, // '{0}' on 
                        page,          // '{1}' was accessed within 
                        controlID      // '{2}' control and observed going into the HTTP response without validation or encoding.
                    }
                };
            }

            return result;
        }


                    // A cross-site scripting vulnerability was observed as untrusted data '{0}' on  '{1}' was accessed within  '{2}' control and observed going into the HTTP response without validation or encoding.
    



    private Result ConstructDetailedErrorMessagesDisplayedResult(IDictionary<string, string> properties)
        {
            // custom-errors-off : Detailed Error Messages Displayed

            // <properties name="path">\web.config</properties>
            // <properties name="snippet">30:   &lt;system.web&gt;&#xD;

            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.DetailedErrorMessagesDisplayed),
                RuleId = ContrastSecurityRuleIds.DetailedErrorMessagesDisplayed,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // The configuration in 
                        path,          // '{0}' has 'mode' set to 'Off' in the <customErrors> section.
                    }
                }
            };

            return result;
        }

        private Result ConstructEventValidationDisabledResult(IDictionary<string, string> properties)
        {
            // event-validation-disabled : Event Validation Disabled

            // <properties name="aspx">\Content\HeaderInjection.aspx</properties>\
            // <properties name="snippet">1: &lt;%@ Page Title="" Language="C#" ...

            string aspx = properties[nameof(aspx)];
            string snippet = properties[nameof(snippet)];

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.EventValidationDisabled),
                RuleId = ContrastSecurityRuleIds.EventValidationDisabled,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(aspx, CreateRegion(snippet)),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // The configuration in 
                        aspx,          // '{0}' has 'enableEventValidation' set to 'false' in the page directive.
                    }
                }
            };

            return result;
        }

        private Result ConstructFormsAuthenticationSSLResult(IDictionary<string, string> properties)
        {
            // forms-auth-ssl : Forms Authentication SSL

            // <properties name="path">\web.config</properties>
            // <properties name="snippet">39:     &lt;!-- set up users --&gt;&#xD;

            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.FormsAuthenticationSSL),
                RuleId = ContrastSecurityRuleIds.FormsAuthenticationSSL,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // The configuration in 
                        path,          // '{0}' was configured to use forms authentication and 'resquireSSL' was not set to 'true' in an <authentication> section.
                    }
                }
            };

            return result;
        }

        private Result ConstructFormsWithoutAutocompletePreventionResult(IDictionary<string, string> properties)
        {
            // autocomplete-missing : Forms Without Autocomplete Prevention

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">{"html":"\u003cform name\u003d\"aspnetForm\"

            var locations = new List<Location>();

            foreach (string key in properties.Keys)
            {
                if (KeyIsReservedPropertyName(key)) { continue; }

                string jsonValue = properties[key];
                JObject root = JObject.Parse(jsonValue);
                string html = root["html"].Value<string>();
                string fileDataKey = "#RuntimeGenerated#=" + key;

                fileDataKey = key;

                byte[] htmlBytes = Encoding.UTF8.GetBytes(html);
                string encoded = System.Convert.ToBase64String(htmlBytes);


                _files.Add(
                    new Artifact
                    {
                        Contents = new ArtifactContent
                        {
                            Text = html,
                            Binary = encoded
                        }
                    });

                locations.Add(new Location()
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation
                        {
                            //UriBaseId = "RuntimeGenerated",
                            Uri = new Uri(key, UriKind.RelativeOrAbsolute)
                        },
                        Region = new Region() { StartLine = 1 }
                    }
                });
            }

            string pageCount = locations.Count.ToString();
            string examplePage = locations[0].PhysicalLocation.ArtifactLocation.Uri.OriginalString;

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.FormsWithoutAutocompletePrevention),
                RuleId = ContrastSecurityRuleIds.FormsWithoutAutocompletePrevention,
                Locations = locations,
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {
                        pageCount,  //'{0}' pages contain a <form> element that do
                        examplePage, //not have 'autocomplete' set to 'off'; e.g. '{1}'.
                    }
                }
            };

            return result;
        }

        private bool KeyIsReservedPropertyName(string key)
        {
            return
                key == "platform" ||
                key == "webforms-page" ||
                key == "route-signature";
        }

        private Result ConstructHttpOnlyCookieFlagDisabledResult(IDictionary<string, string> properties)
        {
            // http-only-disabled : HttpOnly Cookie Flag Disabled

            // default : The configuration in '{0}' had 'httpOnlyCookies' set to 'false' in an <httpCookies> section.

            return ConstructNotImplementedRuleResult(ContrastSecurityRuleIds.HttpOnlyCookieFlagDisabled);
        }

        private Result ConstructInsecureEncryptionAlgorithmsResult(IDictionary<string, string> properties)
        {
            // crypto-bad-cyphers : Insecure Encryption Algorithms

            // default : '{0}' obtained a handle to the cryptographically insecure '{1}' algorithm.

            return ConstructNotImplementedRuleResult(ContrastSecurityRuleIds.InsecureEncryptionAlgorithms);
        }

        private Result ConstructOverlyLongSessionTimeoutResult(IDictionary<string, string> properties)
        {
            // session-timeout : Overly Long Session Timeout

            // <properties name="path">\web.config</properties>
            // <properties name="section">sessionState</properties>
            // <properties name="snippet">52:     &lt;trace enabled="false" ...

            string path = properties[nameof(path)];
            string section = properties[nameof(section)];
            string snippet = properties[nameof(snippet)];

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.OverlyLongSessionTimeout),
                RuleId = ContrastSecurityRuleIds.OverlyLongSessionTimeout,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {              // The configuration in the
                        section,   // <{0}> section of 
                        path       // '{1}' specified a session timeout value greater than 30 minutes.
                    }
                }
            };

            return result;
        }

        private Result ConstructPathTraversalResult(IDictionary<string, string> properties)
        {
            // path-traversal : Path Traversal

            // default : Attacker-controlled path traversal was observed from '{0}' on '{1}' page.

            return ConstructNotImplementedRuleResult(ContrastSecurityRuleIds.PathTraversal);
        }

        private Result ConstructRequestValidationModeDisabledResult(IDictionary<string, string> properties)
        {
            // request-validation-control-disabled : Request Validation Mode Disabled

            // default : The configuration in '{0}' had 'ValidateRequest' set to 'false' in the page directive. Request Validation helps prevent several types of attacks including XSS by detecting potentially dangerous character sequences. An exception is thrown by the framework when a potentially dangerous character sequence is encountered. This exception returns an error page to the user and prevents the application from processing the request. An attacker can submit malicious data to the application that may be processed without further input validation. This malicious data could contain XSS or other injection attacks that may have been prevented by ASP.NET request validation. Note that request validation does not provide 100% protection against XSS or other attacks and should be thought of as a defense-in-depth measure.

            return ConstructNotImplementedRuleResult(ContrastSecurityRuleIds.RequestValidationModeDisabled);
        }

        private Result ConstructSessionCookieHasNoSecureFlagResult(IDictionary<string, string> properties)
        {
            // secure-flag-missing : Session Cookie Has No 'secure' Flag

            // default : The value of the HttpCookie for the cookie '{0}' did not contain the 'secure' flag; the value observed was '{1}'.

            return ConstructNotImplementedRuleResult(ContrastSecurityRuleIds.SessionCookieHasNoSecureFlag);
        }

        private Result ConstructSessionRewritingResult(IDictionary<string, string> properties)
        {
            // session-rewriting : Session Rewriting

            // default : The configuration the the <forms> section of '{0}' has 'UseCookies' set to a value other than 'cookieless'. As a result, the session ID (which is as good as a username and password) is logged to browser history, server logs and proxy logs. More serious, session rewriting can enable session fixcation attacks, in which an attacker causes a victim to use a well-known session id. If the victim authenticates under the attacker's chosen session ID, the attacker can present that session ID to the server and be recognized as the victim.

            return ConstructNotImplementedRuleResult(ContrastSecurityRuleIds.SessionRewriting);
        }

        private Result ConstructSqlInjectionResult(ContrastLogReader.Context context)
        {
            // sql-injection : SQL Injection

            // <properties name="platform">ASP.NET Web Forms</properties>
            // <properties name="webforms-page">OWASP.WebGoat.NET.ForgotPassword</properties>
            // <properties name="route-signature">OWASP.WebGoat.NET.ForgotPassword</properties>

            string untrustedData = BuildSourcesString(context.Sources);
            string page = context.RequestUri;
            string source = context.PropagationEvents[0].Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;
            string caller = context.PropagationEvents[context.PropagationEvents.Count - 1].Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;
            string sink = context.MethodEvent.Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;

            // default : SQL injection from untrusted source(s) '{0}' observed on '{1}' page. Untrusted data flowed from '{2}' to dangerous sink '{3}' in '{4}'.

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.SqlInjection),
                RuleId = ContrastSecurityRuleIds.SqlInjection,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(page),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {
                        untrustedData, // SQL injection from untrusted source(s) '{0}'
                        page,          // observed on '{1}' page.
                        source,        // Untrusted data flowed from '{2}'
                        sink,          // to dangerous sink '{3}'
                        caller         // from a call site in '{4}'.
                    }
                }
            };

            result.CodeFlows = new List<CodeFlow>
            {
                new CodeFlow
                {
                     ThreadFlows = new List<ThreadFlow>
                     {
                         new ThreadFlow
                         {
                              Locations = context.PropagationEvents
                         }
                     }
                }
            };

            result.CodeFlows[0].ThreadFlows[0].Locations.Add(context.MethodEvent);

            // ** TODO remove this ** /
            result.Stacks = new List<Stack>();

            foreach (ThreadFlowLocation threadFlowLocation in result.CodeFlows[0].ThreadFlows[0].Locations)
            {
                result.Stacks.Add(threadFlowLocation.Stack);
            }

            return result;
        }

        private string BuildSourcesString(HashSet<Tuple<string, string>> sources)
        {
            var sb = new StringBuilder();
            foreach (Tuple<string, string> tuple in sources)
            {
                if (tuple.Item1 == null || tuple.Item2 == null) { continue; }
                // Item1 is the name, Item2 is the source type, e.g., parameter
                sb.Append(tuple.Item1 + "(" + tuple.Item2 + ")");
            }
            return sb.ToString();
        }

        private Result ConstructVersionHeaderEnabledResult(IDictionary<string, string> properties)
        {
            // version-header-enabled : Version Header Enabled

            // <properties name="path">\web.config</properties>

            string path = properties[nameof(path)];

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.VersionHeaderEnabled),
                RuleId = ContrastSecurityRuleIds.VersionHeaderEnabled,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(path),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // The configuration in 
                        path,          // '{0}' did not explicitly disable 'enableVersionHeader' in the <httpRuntime> section.
                    }
                }
            };

            return result;
        }

        private Result ConstructWebApplicationDeployedinDebugModeResult(IDictionary<string, string> properties)
        {
            // compilation-debug : Web Application Deployed in Debug Mode

            // <properties name="path">\web.config</properties>
            // <properties name="snippet">30:   &lt;system.web&gt;&#xD;

            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            var result = new Result
            {
                Level = GetRuleFailureLevel(ContrastSecurityRuleIds.WebApplicationDeployedinDebugMode),
                RuleId = ContrastSecurityRuleIds.WebApplicationDeployedinDebugMode,
                Locations = new List<Location>()
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                    }
                },
                Message = new Message
                {
                    Id = "default",
                    Arguments = new List<string>
                    {                  // The configuration in 
                        path,          // '{0}' has 'debug' set to 'true' in the <compilation> section.
                    }
                }
            };

            return result;
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
                Snippet = new ArtifactContent { Text = sb.ToString() }
            };
        }

        private string NaiveXmlDecode(string snippet)
        {
            return snippet.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
        }

        private PhysicalLocation CreatePhysicalLocation(string uri, Region region = null)
        {
            region = region ?? new Region { StartLine = 1, StartColumn = 1, EndColumn = 1 };
            //return new PhysicalLocation
            //{
            //    FileLocation = new FileLocation
            //    {
            //        Uri = new Uri(uri, UriKind.Relative),
            //        UriBaseId = "SITE_ROOT"
            //    },
            //    Region = region,
            //    ContextRegion = region
            //};

            uri = @"E:\src\WebGoat.NET" + uri.Replace(@"/", @"\");

            return new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = new Uri(uri, UriKind.Absolute)
                },
                Region = region,
                ContextRegion = region
            };

        }

        private static void AddProperty(Result result, string value, string key)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                result.SetProperty(key, value);
            }
        }

        // Get the failure level for the rule with the specified id, defaulting to
        // "warning" if the rule does not specify a configuration.
        private FailureLevel GetRuleFailureLevel(string ruleId)
        {
            return _rules[ruleId].DefaultConfiguration?.Level ?? FailureLevel.Warning;
        }
    }

    /// <summary>
    /// Pluggable Contrast Security log reader.
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

            public List<ThreadFlowLocation> PropagationEvents { get; set; }

            public ThreadFlowLocation MethodEvent { get; set; }

            public ThreadFlowLocation CurrentThreadFlowLocation { get; set; }

            public StackFrame Signature { get; set; }

            public HashSet<Tuple<string, string>> Sources { get; set; }

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
        /// Contrast Security exported XML elements and attributes.
        /// </summary>
        private static class SchemaStrings
        {
            // elements
            public const string ElementH = "h";
            public const string ElementP = "P";
            public const string ElementBody = "body";
            public const string ElementArgs = "args";
            public const string ElementObject = "obj";
            public const string ElementProps = "props";
            public const string ElementStack = "stack";
            public const string ElementFrame = "frame";
            public const string ElementReturn = "return";
            public const string ElementEvents = "events";
            public const string ElementSource = "source";
            public const string ElementSources = "sources";
            public const string ElementFinding = "finding";
            public const string ElementFindings = "findings";
            public const string ElementRequest = "request";
            public const string ElementHeaders = "headers";
            public const string ElementSignature = "signature";
            public const string ElementParameters = "parameters";
            public const string ElementProperties = "properties";
            public const string ElementMethodEvent = "method-event";
            public const string ElementPropagationEvent = "propagation-event";

            // attributes
            public const string AttributeUri = "uri";
            public const string AttributeName = "name";
            public const string AttributeType = "type";
            public const string AttributeRuleId = "ruleId";
            public const string AttributeMethod = "method";
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
                { SchemaStrings.ElementSource, ReadSource},
                { SchemaStrings.ElementSources, ReadSources},
                { SchemaStrings.ElementFrame, ReadFrame},
                { SchemaStrings.ElementMethodEvent, ReadMethodEvent},
                { SchemaStrings.ElementProps, ReadProps},
            };
        }

        public void Read(Context context, Stream input)
        {
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
            context.ClearRequest();

            string uri = reader.ReadAttributeString(SchemaStrings.AttributeUri);
            string method = reader.ReadAttributeString(SchemaStrings.AttributeMethod);

            context.RefineRequest(uri, method);

            reader.ReadChildren(SchemaStrings.ElementRequest, parent);
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
            Context context = (Context)parent;
            context.Signature = null;
            context.MethodEvent = null;
            context.PropagationEvents = null;
            context.CurrentThreadFlowLocation = null;
            context.Sources = null;

            reader.ReadChildren(SchemaStrings.ElementEvents, parent);
        }

        private static void ReadPropagationEvent(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementPropagationEvent, parent);

            Context context = (Context)parent;
            context.PropagationEvents = context.PropagationEvents ?? new List<ThreadFlowLocation>();
            context.PropagationEvents.Add(context.CurrentThreadFlowLocation);
            context.CurrentThreadFlowLocation = null;
            context.Signature = null;
        }

        private static void ReadSignature(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            string signature = reader.ReadElementContentAsString();
            context.Signature = CreateStackFrameFromSignature(signature);
        }

        private static StackFrame CreateStackFrameFromSignature(string signature)
        {
            return new StackFrame
            {
                Location = new Location
                {
                    LogicalLocation = new LogicalLocation
                    {
                        FullyQualifiedName = signature
                    }
                }
            };
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
            Context context = (Context)parent;
            Debug.Assert(context.CurrentThreadFlowLocation == null);
            context.CurrentThreadFlowLocation = new ThreadFlowLocation
            {
                Stack = new Stack()
                {
                    Frames = new List<StackFrame> { context.Signature }
                }
            };

            reader.ReadChildren(SchemaStrings.ElementStack, parent);
        }

        private static void ReadSources(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementSources, parent);
        }

        private static void ReadSource(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            string type = reader.ReadAttributeString(SchemaStrings.AttributeType);
            string name = reader.ReadAttributeString(SchemaStrings.AttributeName);
            context.Sources = context.Sources ?? new HashSet<Tuple<string, string>>();
            context.Sources.Add(new Tuple<string, string>(name, type));
            reader.Skip();
        }

        private static void ReadFrame(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            string frame = reader.ReadElementContentAsString();
            context.CurrentThreadFlowLocation.Stack.Frames.Add(CreateStackFrameFromSignature(frame));
        }

        private static void ReadMethodEvent(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementMethodEvent, parent);

            Context context = (Context)parent;
            context.MethodEvent = context.CurrentThreadFlowLocation;
            context.CurrentThreadFlowLocation = null;
            context.Signature = null;
        }

        private void ReadProps(SparseReader reader, object parent)
        {
            _readingProps = true;
            reader.ReadChildren(SchemaStrings.ElementProps, parent);
            _readingProps = false;
        }
    }
}