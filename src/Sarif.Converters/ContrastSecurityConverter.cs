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
using System.Text.RegularExpressions;
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
        private const string SiteRootUriBaseIdName = "SITE_ROOT";
        private const string SiteRootDescriptionMessageId = "SiteRootDescription";

        private IDictionary<string, ReportingDescriptor> _rules;
        private IDictionary<string, int> _ruleIdToIndexDictionary;

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
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            LogicalLocations.Clear();

            var context = new ContrastLogReader.Context();

            // 1. First we initialize our global rules data from the SARIF we have embedded as a resource.
            Assembly assembly = typeof(ContrastSecurityConverter).Assembly;
            SarifLog sarifLog;

            using (Stream stream = assembly.GetManifestResourceStream(ContrastSecurityRulesData))
            using (var streamReader = new StreamReader(stream))
            {
                string prereleaseRuleDataLogText = streamReader.ReadToEnd();
                PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(prereleaseRuleDataLogText, Newtonsoft.Json.Formatting.Indented, out string currentRuleDataLogText);
                sarifLog = JsonConvert.DeserializeObject<SarifLog>(currentRuleDataLogText);
            }

            // 2. Retain a pointer to the rules dictionary, which we will use to set rule severity.
            Run run = sarifLog.Runs[0];
            _rules = run.Tool.Driver.Rules.ToDictionary(rule => rule.Id);

            // Create another dictionary, from ruleId to ruleIndex, so we can populate easily result.ruleIndex.
            // This is important because we will populate result.message.id, but _not_ result.message.text.
            // That means that viewers will need to _look up_ the message text by way of result.ruleIndex,
            // so we must popuate it.
            _ruleIdToIndexDictionary = CreateRuleToIndexDictionary(run.Tool.Driver.Rules);

            run.OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
            {
                {
                    SiteRootUriBaseIdName,
                    new ArtifactLocation {
                        Description = new Message {
                            Id = SiteRootDescriptionMessageId
                        }
                    }
                }
            };

            run.Tool.Driver.GlobalMessageStrings = new Dictionary<string, MultiformatMessageString>
            {
                {
                    SiteRootDescriptionMessageId,
                    new MultiformatMessageString
                    {
                        Text = ConverterResources.ContrastSecuritySiteRootDescription
                    }
                }
            };

            // 3. Now, parse all the contrast XML to create the complete results set.
            var results = new List<Result>();
            var reader = new ContrastLogReader();
            reader.FindingRead += (ContrastLogReader.Context current) => { results.Add(CreateResult(current)); };
            reader.Read(context, input);

            // 4. Finally, complete the SARIF log file with various tables and then the results
            if (LogicalLocations?.Any() == true)
            {
                run.LogicalLocations = LogicalLocations;
            }

            PersistResults(output, results, run);
        }

        private IDictionary<string, int> CreateRuleToIndexDictionary(IList<ReportingDescriptor> rules)
        {
            var dictionary = new Dictionary<string, int>();
            for (int i = 0; i < rules.Count; ++i)
            {
                dictionary.Add(rules[i].Id, i);
            }

            return dictionary;
        }

        internal Result CreateResult(ContrastLogReader.Context context)
        {
            switch (context.RuleId)
            {
                case ContrastSecurityRuleIds.AntiCachingControlsMissing:
                {
                    return ConstructAntiCachingControlsMissingResult(context);
                }

                case ContrastSecurityRuleIds.AuthorizationRulesMissingDenyRule:
                {
                    return ConstructAuthorizationRulesMissingDenyResult(context);
                }

                case ContrastSecurityRuleIds.CrossSiteScripting:
                {
                    return ConstructCrossSiteScriptingResult(context);
                }

                case ContrastSecurityRuleIds.DetailedErrorMessagesDisplayed:
                {
                    return ConstructDetailedErrorMessagesDisplayedResult(context);
                }

                case ContrastSecurityRuleIds.EventValidationDisabled:
                {
                    return ConstructEventValidationDisabledResult(context);
                }

                case ContrastSecurityRuleIds.FormsAuthenticationSSL:
                {
                    return ConstructFormsAuthenticationSSLResult(context);
                }

                case ContrastSecurityRuleIds.FormsWithoutAutocompletePrevention:
                {
                    return ConstructFormsWithoutAutocompletePreventionResult(context);
                }

                case ContrastSecurityRuleIds.HttpOnlyCookieFlagDisabled:
                {
                    return ConstructHttpOnlyCookieFlagDisabledResult(context);
                }

                case ContrastSecurityRuleIds.InsecureEncryptionAlgorithms:
                {
                    return ConstructInsecureEncryptionAlgorithmsResult(context);
                }

                case ContrastSecurityRuleIds.InsecureHashAlgorithms:
                {
                    return ConstructInsecureHashAlgorithmsResult(context);
                }

                case ContrastSecurityRuleIds.OverlyLongSessionTimeout:
                {
                    return ConstructOverlyLongSessionTimeoutResult(context);
                }

                case ContrastSecurityRuleIds.PagesWithoutAntiClickjackingControls:
                {
                    return ConstructPagesWithoutAntiClickjackingControlsResult(context);
                }

                case ContrastSecurityRuleIds.PathTraversal:
                {
                    return ConstructPathTraversalResult(context);
                }

                case ContrastSecurityRuleIds.RequestValidationDisabled:
                {
                    return ConstructRequestValidationDisabledResult(context);
                }

                case ContrastSecurityRuleIds.RequestValidationModeDisabled:
                {
                    return ConstructRequestValidationModeDisabledResult(context);
                }

                case ContrastSecurityRuleIds.SessionCookieHasNoSecureFlag:
                {
                    return ConstructSessionCookieHasNoSecureFlagResult(context);
                }

                case ContrastSecurityRuleIds.SessionRewriting:
                {
                    return ConstructSessionRewritingResult(context);
                }

                case ContrastSecurityRuleIds.SqlInjection:
                {
                    return ConstructSqlInjectionResult(context);
                }

                case ContrastSecurityRuleIds.VersionHeaderEnabled:
                {
                    return ConstructVersionHeaderEnabledResult(context);
                }

                case ContrastSecurityRuleIds.WebApplicationDeployedinDebugMode:
                {
                    return ConstructWebApplicationDeployedinDebugModeResult(context);
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
                RuleId = ruleId,
                RuleIndex = _ruleIdToIndexDictionary[ruleId],
                Level = GetRuleFailureLevel(ruleId),
                Message = new Message { Text = $"TODO: missing message construction for rule '{ruleId}'." }
            };

            return result;
        }

        private Result ConstructAntiCachingControlsMissingResult(ContrastLogReader.Context context)
        {
            // cache-controls-missing : Anti-Caching Controls Missing

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">{"Header:Cache-Control":"private"}</properties>
            // <properties name="/webgoat/WebGoatCoins/MainPage.aspx">{"Header:Cache-Control":"private"}</properties>

            var locations = new List<Location>();

            string examplePage = null;
            string exampleHeader = null;

            IDictionary<string, string> properties = context.Properties;
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

            Result result = CreateResultCore(context);
            result.Locations = locations;
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {
                    pageCount,     // {0} page Cache-Control header(s) did not contain 'no-store' or 'no-cache';
                    examplePage,   // e.g., the value in page '{1}' 
                    exampleHeader  // was observed to be '{2}'.
                }
            };

            return result;
        }

        private Result ConstructAuthorizationRulesMissingDenyResult(ContrastLogReader.Context context)
        {
            // authorization-missing-deny : Authorization Rules Missing Deny Rule

            Result result = CreateResultCore(context);

            // <properties name="path">\web.config</properties>
            // <properties name="locationPath">CustomerLogin.aspx</properties>
            // <properties name="snippet">10:     &lt;system.web&gt;&#xD;</properties>

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string locationPath = properties.ContainsKey(nameof(locationPath)) ? properties[nameof(locationPath)] : null;
            string snippet = properties[nameof(snippet)];

            IList<Location> locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                }
            };

            result.Locations = locations;

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

        private Result ConstructInsecureHashAlgorithmsResult(ContrastLogReader.Context context)
        {
            // crypto-bad-mac : Insecure Hash Algorithms

            Result result = CreateResultCore(context);

            Stack stack = context.MethodEvent.Stack;
            result.Stacks = new List<Stack>
            {
                stack
            };

            Location resultLocation = stack.Frames[1].Location;
            result.Locations = new List<Location>
            {
                resultLocation
            };

            string insecureClassName = GetClassNameFromCtorCall(
                stack.Frames[0].Location.LogicalLocation.FullyQualifiedName);
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {
                    resultLocation.LogicalLocation.FullyQualifiedName,
                    insecureClassName
                }
            };

            return result;
        }

        private static readonly Regex ClassNameFromCtorRegex =
            new Regex(@"\.(?<className>[^.]+)\.\.ctor\(\)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static string GetClassNameFromCtorCall(string fullyQualifiedName)
        {
            Match match = ClassNameFromCtorRegex.Match(fullyQualifiedName);

            // If we can't parse this as a ctor invocation, do the best we can: just return
            // the whole string.
            return match.Success
                ? match.Groups["className"].Value
                : fullyQualifiedName;
        }

        private Result ConstructPagesWithoutAntiClickjackingControlsResult(ContrastLogReader.Context context)
        {
            // clickjacking-control-missing : Pages Without Anti-Clickjacking Controls

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">1536704253063</properties>
            // <properties name="/webgoat/WebGoatCoins/MainPage.aspx">1536704186306</properties>

            var locations = new List<Location>();

            IDictionary<string, string> properties = context.Properties;
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

            Result result = CreateResultCore(context);
            result.Locations = locations;
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {
                    pageCount,  // {0} page(s) have insufficient anti-clickjacking controls, 
                    examplePage // e.g., '{1}' 
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
            string page = context.RequestTarget;
            string controlId = context.Properties.ContainsKey(nameof(controlId)) ? context.Properties[nameof(controlId)] : null;

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(page),
                }
            };

            if (controlId == null)
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
                        controlId      // '{2}' control and observed going into the HTTP response without validation or encoding.
                    }
                };
            }

            return result;
        }

        private Result ConstructDetailedErrorMessagesDisplayedResult(ContrastLogReader.Context context)
        {
            // custom-errors-off : Detailed Error Messages Displayed

            // <properties name="path">\web.config</properties>
            // <properties name="snippet">30:   &lt;system.web&gt;&#xD;

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                }
            };
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                  // The configuration in 
                    path,          // '{0}' has 'mode' set to 'Off' in the <customErrors> section.
                }
            };

            return result;
        }

        private Result ConstructEventValidationDisabledResult(ContrastLogReader.Context context)
        {
            // event-validation-disabled : Event Validation Disabled

            // <properties name="aspx">\Content\HeaderInjection.aspx</properties>\
            // <properties name="snippet">1: &lt;%@ Page Title="" Language="C#" ...

            IDictionary<string, string> properties = context.Properties;
            string aspx = properties[nameof(aspx)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(aspx, CreateRegion(snippet)),
                }
            };
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                  // The configuration in 
                    aspx,          // '{0}' has 'enableEventValidation' set to 'false' in the page directive.
                }
            };

            return result;
        }

        private Result ConstructFormsAuthenticationSSLResult(ContrastLogReader.Context context)
        {
            // forms-auth-ssl : Forms Authentication SSL

            // <properties name="path">\web.config</properties>
            // <properties name="snippet">39:     &lt;!-- set up users --&gt;&#xD;

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                }
            };
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                  // The configuration in 
                    path,          // '{0}' was configured to use forms authentication and 'resquireSSL' was not set to 'true' in an <authentication> section.
                }
            };

            return result;
        }

        private Result ConstructFormsWithoutAutocompletePreventionResult(ContrastLogReader.Context context)
        {
            // The maximum length of a snippet that Contrast Security embeds in their HTML file.
            // If the actual <form> element is longer than this, they tack on an ellipsis, which
            // we don't want to include in the SARIF file.
            const int MaxSnippetLength = 1000;

            // autocomplete-missing : Forms Without Autocomplete Prevention

            // <properties name="/webgoat/Content/EncryptVSEncode.aspx">{"html":"\u003cform name\u003d\"aspnetForm\"

            var locations = new List<Location>();

            IDictionary<string, string> properties = context.Properties;
            foreach (string key in properties.Keys)
            {
                if (KeyIsReservedPropertyName(key)) { continue; }

                string jsonValue = properties[key];
                JObject root = JObject.Parse(jsonValue);
                string snippet = root["html"].Value<string>();

                int snippetLength = snippet.Length;
                if (snippetLength > MaxSnippetLength)
                {
                    snippet = snippet.Substring(0, MaxSnippetLength);
                    snippetLength = MaxSnippetLength;
                }

                locations.Add(new Location
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        ArtifactLocation = new ArtifactLocation
                        {
                            UriBaseId = SiteRootUriBaseIdName,
                            Uri = new Uri(key, UriKind.RelativeOrAbsolute)
                        },
                        Region = new Region
                        {
                            CharOffset = 0,             // Unfortunately the XML doesn't actually tell us this, but SARIF requires it.
                            CharLength = snippetLength,
                            Snippet = new ArtifactContent
                            {
                                Text = snippet
                            }
                        }
                    }
                });
            }

            string pageCount = locations.Count.ToString();
            string examplePage = locations[0].PhysicalLocation.ArtifactLocation.Uri.OriginalString;

            Result result = CreateResultCore(context);
            result.Locations = locations;
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {
                    pageCount,  //'{0}' pages contain a <form> element that do
                    examplePage, //not have 'autocomplete' set to 'off'; e.g. '{1}'.
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

        private Result ConstructHttpOnlyCookieFlagDisabledResult(ContrastLogReader.Context context)
        {
            // http-only-disabled : HttpOnly Cookie Flag Disabled

            // default : The configuration in '{0}' had 'httpOnlyCookies' set to 'false' in an <httpCookies> section.

            // <properties name="path">\web.config</properties>
            // <properties name = "snippet" >35:    &lt;/compilation&gt;&#xD;
            // 36:     &lt;httpCookies httpOnlyCookies="false"/&gt;&#xD;
            // 37:     &lt;!--show detailed error messages --&gt;&#xD;

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);

            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet))
                }
            };

            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                  // The configuration in
                    path           // '{0}' had 'httpOnlyCookies' set to 'false' in an <httpCookies> section.
                }
            };

            return result;
        }

        private Result ConstructInsecureEncryptionAlgorithmsResult(ContrastLogReader.Context context)
        {
            // crypto-bad-cyphers : Insecure Encryption Algorithms

            // default : '{0}' obtained a handle to the cryptographically insecure '{1}' algorithm.

            return ConstructNotImplementedRuleResult(context.RuleId);
        }

        private Result ConstructOverlyLongSessionTimeoutResult(ContrastLogReader.Context context)
        {
            // session-timeout : Overly Long Session Timeout

            // <properties name="path">\web.config</properties>
            // <properties name="section">sessionState</properties>
            // <properties name="snippet">52:     &lt;trace enabled="false" ...

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string section = properties[nameof(section)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                }
            };
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {              // The configuration in the
                    section,   // <{0}> section of 
                    path       // '{1}' specified a session timeout value greater than 30 minutes.
                }
            };

            return result;
        }

        private Result ConstructPathTraversalResult(ContrastLogReader.Context context)
        {
            // path-traversal : Path Traversal

            // default : An attacker-controlled path traversal was observed from '{0}' on page '{1}'.

            Result result = CreateResultCore(context);

            result.Locations = new List<Location>
            {
                new Location
                {
                    LogicalLocation = new LogicalLocation
                    {
                        FullyQualifiedName = GetUserCodeLocation(context.MethodEvent.Stack)
                    }
                }
            };

            string page = context.RequestTarget;
            string sources = BuildSourcesString(context.Sources);

            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {              // An attacker-controlled path traversal was observed from
                    sources,   // '{0}' on
                    page       // page '{1}'.
                }
            };

            return result;
        }

        private Result ConstructRequestValidationDisabledResult(ContrastLogReader.Context context)
        {
            // request-validation-disabled : Request Validation Disabled

            // default : The web page '{0}' had 'ValidateRequest' set to 'false' in the page directive. Request Validation helps prevent several types of attacks including XSS by detecting potentially dangerous character sequences. An exception is thrown by the framework when a potentially dangerous character sequence is encountered. This exception returns an error page to the user and prevents the application from processing the request. An attacker can submit malicious data to the application that may be processed without further input validation. This malicious data could contain XSS or other injection attacks that may have been prevented by ASP.NET request validation. Note that request validation does not provide 100% protection against XSS or other attacks and should be thought of as a defense-in-depth measure.

            // <properties name="aspx">\WebGoatCoins\ProductDetails.aspx</properties>
            // <properties name="snippet">1: &lt;%@ Page Title="" Language="C#" ValidateRequest="false" ... %&gt;</properties>

            IDictionary<string, string> properties = context.Properties;
            string aspx = properties[nameof(aspx)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(aspx, CreateRegion(snippet)),
                }
            };

            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {              // The web page 
                    aspx       // '{0}' had 'ValidateRequest' set to 'false' in the page directive. ...
                }
            };

            return result;
        }

        private Result ConstructRequestValidationModeDisabledResult(ContrastLogReader.Context context)
        {
            // request-validation-control-disabled : Request Validation Mode Disabled

            // default : A control on the web page '{0}' had 'ValidateRequestMode' set to 'Disabled'. Request Validation helps prevent several types of attacks including XSS by detecting potentially dangerous character sequences. An exception is thrown by the framework when a potentially dangerous character sequence is encountered. This exception returns an error page to the user and prevents the application from processing the request. An attacker can submit malicious data to the application that may be processed without further input validation. This malicious data could contain XSS or other injection attacks that may have been prevented by ASP.NET request validation. Note that request validation does not provide 100% protection against XSS or other attacks and should be thought of as a defense-in-depth measure.

            return ConstructNotImplementedRuleResult(context.RuleId);
        }

        private Result ConstructSessionCookieHasNoSecureFlagResult(ContrastLogReader.Context context)
        {
            // secure-flag-missing : Session Cookie Has No 'secure' Flag

            // default : The value of the HttpCookie for the cookie '{0}' did not contain the 'secure' flag; the value observed was '{1}'.

            // <properties name="cookieName">ASP.NET_SessionId</properties>

            // They also use the "evidence" element to hold the text of the cookie.

            IDictionary<string, string> properties = context.Properties;
            string cookieName = properties[nameof(cookieName)];

            Result result = CreateResultCore(context);

            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(context.RequestTarget)
                }
            };

            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                       // The value of the HttpCookie for the cookie 
                    cookieName,         // ''{0}' did not contain the 'secure' flag;
                    context.Evidence    // the value observed was '{1}'.
                }
            };

            return result;
        }

        private Result ConstructSessionRewritingResult(ContrastLogReader.Context context)
        {
            // session-rewriting : Session Rewriting

            // default : The configuration in the {0} section of '{1}' has 'UseCookies' set to a value other than 'cookieless'. As a result, the session ID (which is as good as a username and password) is logged to browser history, server logs and proxy logs. More serious, session rewriting can enable session fixcation attacks, in which an attacker causes a victim to use a well-known session id. If the victim authenticates under the attacker's chosen session ID, the attacker can present that session ID to the server and be recognized as the victim.

            // <properties name="path">\web.config</properties>
            // <properties name="section">forms</properties>
            // <properties name="snippet">39:     &lt;!--set up users--&gt;&#xD;
            //   40:    &lt;authentication mode="Forms"&gt;&#xD; ...

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string section = properties[nameof(section)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);

            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet))
                }
            };

            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                       // The configuration in the
                    section,            // {0} section of
                    path                // '{1}' has 'UseCookies' set to a value other than 'cookieless'.
                }
            };

            return result;
        }

        private Result ConstructSqlInjectionResult(ContrastLogReader.Context context)
        {
            // sql-injection : SQL Injection

            // <properties name="platform">ASP.NET Web Forms</properties>
            // <properties name="webforms-page">OWASP.WebGoat.NET.ForgotPassword</properties>
            // <properties name="route-signature">OWASP.WebGoat.NET.ForgotPassword</properties>

            string untrustedData = BuildSourcesString(context.Sources);
            string page = context.RequestTarget;
            string source = context.PropagationEvents[0].Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;
            string caller = context.PropagationEvents[context.PropagationEvents.Count - 1].Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;
            string sink = context.MethodEvent.Stack.Frames[0].Location.LogicalLocation?.FullyQualifiedName;

            // default : SQL injection from untrusted source(s) '{0}' observed on '{1}' page. Untrusted data flowed from '{2}' to dangerous sink '{3}' in '{4}'.

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(page),
                }
            };
            result.Message = new Message
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
            };

            result.CodeFlows[0].ThreadFlows[0].Locations.Add(context.MethodEvent);

            return result;
        }

        private string BuildSourcesString(HashSet<Tuple<string, string>> sources)
        {
            const string UnknownSourceTypeName = "<unknown source type>";

            var sb = new StringBuilder();
            foreach (Tuple<string, string> tuple in sources)
            {
                if (sb.Length > 0) { sb.Append(", "); }

                // Item1 is the name, Item2 is the source type, e.g., parameter
                if (!string.IsNullOrWhiteSpace(tuple.Item1)) { sb.Append(tuple.Item1 + ": "); }

                string sourceType = !string.IsNullOrWhiteSpace(tuple.Item2)
                    ? tuple.Item2
                    : UnknownSourceTypeName;

                sb.Append(sourceType);
            }

            return sb.ToString();
        }

        private Result ConstructVersionHeaderEnabledResult(ContrastLogReader.Context context)
        {
            // version-header-enabled : Version Header Enabled

            // <properties name="path">\web.config</properties>

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path),
                }
            };
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                  // The configuration in 
                    path,          // '{0}' did not explicitly disable 'enableVersionHeader' in the <httpRuntime> section.
                }
            };

            return result;
        }

        private Result ConstructWebApplicationDeployedinDebugModeResult(ContrastLogReader.Context context)
        {
            // compilation-debug : Web Application Deployed in Debug Mode

            // <properties name="path">\web.config</properties>
            // <properties name="snippet">30:   &lt;system.web&gt;&#xD;

            IDictionary<string, string> properties = context.Properties;
            string path = properties[nameof(path)];
            string snippet = properties[nameof(snippet)];

            Result result = CreateResultCore(context);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(path, CreateRegion(snippet)),
                }
            };
            result.Message = new Message
            {
                Id = "default",
                Arguments = new List<string>
                {                  // The configuration in 
                    path,          // '{0}' has 'debug' set to 'true' in the <compilation> section.
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

            return new Region
            {
                StartLine = startLine.Value,
                EndLine = endLine != startLine.Value ? endLine : 0,
                Snippet = new ArtifactContent { Text = sb.ToString() }
            };
        }

        private string NaiveXmlDecode(string snippet)
        {
            return snippet.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
        }

        private Result CreateResultCore(ContrastLogReader.Context context)
        {
            return new Result
            {
                RuleId = context.RuleId,
                RuleIndex = _ruleIdToIndexDictionary[context.RuleId],
                Level = GetRuleFailureLevel(context.RuleId),
                WebRequest = CreateWebRequest(context),
                CodeFlows = CreateCodeFlows(context)
            };
        }

        private WebRequest CreateWebRequest(ContrastLogReader.Context context)
        {
            return context.HasRequest() ?
                new WebRequest
                {
                    Protocol = context.RequestProtocol,
                    Version = context.RequestVersion,
                    Method = context.RequestMethod,
                    Target = context.RequestTarget,
                    Headers = context.Headers,
                    Parameters = context.Parameters,
                    Body = string.IsNullOrEmpty(context.RequestBody)
                        ? null
                        : new ArtifactContent
                        {
                            Text = context.RequestBody
                        }
                } : null;
        }

        private IList<CodeFlow> CreateCodeFlows(ContrastLogReader.Context context)
        {
            List<CodeFlow> codeFlows = null;

            if (context.PropagationEvents != null)
            {
                codeFlows = new List<CodeFlow>
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

                if (context.MethodEvent != null)
                {
                    codeFlows[0].ThreadFlows[0].Locations.Add(context.MethodEvent);
                }
            }

            return codeFlows;
        }

        private PhysicalLocation CreatePhysicalLocation(string uri, Region region = null)
        {
            return new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    UriBaseId = SiteRootUriBaseIdName,
                    Uri = new Uri(uri, UriKind.RelativeOrAbsolute)
                },
                Region = region
            };
        }

        // Find the user code method call closest to the top of the stack. This is
        // the location we should report as being responsible for the result.
        private static string GetUserCodeLocation(Stack stack)
        {
            const string SystemPrefix = "System.";

            foreach (StackFrame frame in stack.Frames)
            {
                string fullyQualifiedLogicalName = frame.Location.LogicalLocation.FullyQualifiedName;
                if (!fullyQualifiedLogicalName.StartsWith(SystemPrefix))
                {
                    return fullyQualifiedLogicalName;
                }
            }

            return string.Empty;
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

            public string RequestProtocol { get; set; }

            public string RequestVersion { get; set; }

            public string RequestMethod { get; set; }

            public string RequestTarget { get; set; }

            public string RequestBody { get; set; }

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

            public string Evidence { get; set; }

            public HashSet<Tuple<string, string>> Sources { get; set; }

            public IDictionary<string, string> Headers { get; set; }

            public IDictionary<string, string> Parameters { get; set; }

            public bool HasRequest()
            {
                // Whatever other web request-related properties the Contrast Security XML has,
                // it will always have a "protocol" attribute. (The same is true of "method",
                // but there's no need to check both.)
                return RequestProtocol != null;
            }

            internal void RefineFinding(string ruleId)
            {
                RuleId = ruleId;
                ClearProperties();
                ClearRequest();
                ClearEvidence();
            }

            internal void RefineRequest(string protocol, string version, string target, string method)
            {
                RequestProtocol = protocol;
                RequestVersion = version;
                RequestTarget = target;
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

            internal void ClearHeaders()
            {
                Headers = null;
            }

            internal void AddHeader(string name, string value)
            {
                Headers = Headers ?? new Dictionary<string, string>();

                if (!Headers.ContainsKey(name)) { Headers.Add(name, value); }
            }

            internal void ClearParameters()
            {
                Parameters = null;
            }

            internal void AddParameter(string name, string value)
            {
                Parameters = Parameters ?? new Dictionary<string, string>();

                if (!Parameters.ContainsKey(name)) { Parameters.Add(name, value); }
            }

            internal void ClearFinding()
            {
                RefineFinding(null);
            }

            internal void ClearRequest()
            {
                RefineRequest(protocol: null, version: null, target: null, method: null);
                ClearHeaders();
                ClearParameters();
                ClearBody();
            }

            internal void ClearProperties()
            {
                RefineProperties(null, null);
            }

            internal void ClearBody()
            {
                RequestBody = null;
            }

            internal void ClearEvidence()
            {
                RefineEvidence(null);
            }

            internal void RefineEvidence(string evidence)
            {
                Evidence = evidence;
            }
        }

        /// <summary>
        /// Contrast Security exported XML elements and attributes.
        /// </summary>
        private static class SchemaStrings
        {
            // elements
            public const string ElementH = "h";
            public const string ElementP = "p";
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
            public const string ElementEvidence = "evidence";
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
            public const string AttributeProtocol = "protocol";
            public const string AttributeVersion = "version";
            public const string AttributeMethod = "method";
            public const string AttributeValue = "value";
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
                { SchemaStrings.ElementEvidence, ReadEvidence },
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
                    throw new XmlException(string.Format(CultureInfo.InvariantCulture, "Invalid root element in Contrast Security log file: {0}", sparseReader.LocalName));
                }
            }
        }

        private static void ReportError(object sender, EventArgs e)
        {
            throw new XmlException(e.ToString());
        }

        private static void ReadFindings(SparseReader reader, object parent)
        {
            reader.ReadChildren(SchemaStrings.ElementFindings, parent);
        }


        private void ReadFinding(SparseReader reader, object parent)
        {
            Context context = (Context)parent;

            string ruleId = reader.ReadAttributeString(SchemaStrings.AttributeRuleId);

            context.RefineFinding(ruleId);
            reader.ReadChildren(SchemaStrings.ElementFinding, parent);

            FindingRead?.Invoke(context);

            context.ClearFinding();
        }

        private static void ReadRequest(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            context.ClearRequest();

            string protocol = reader.ReadAttributeString(SchemaStrings.AttributeProtocol);
            string version = reader.ReadAttributeString(SchemaStrings.AttributeVersion);
            string target = reader.ReadAttributeString(SchemaStrings.AttributeUri);
            string method = reader.ReadAttributeString(SchemaStrings.AttributeMethod);

            context.RefineRequest(protocol, version, target, method);

            reader.ReadChildren(SchemaStrings.ElementRequest, parent);
        }

        private static void ReadBody(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            context.RequestBody = reader.ReadElementContentAsString();
        }

        private static void ReadHeaders(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            context.ClearHeaders();

            reader.ReadChildren(SchemaStrings.ElementHeaders, parent);
        }

        private static void ReadH(SparseReader reader, object parent)
        {
            string name = reader.ReadAttributeString(SchemaStrings.AttributeName);
            string value = reader.ReadAttributeString(SchemaStrings.AttributeValue);

            Context context = (Context)parent;
            context.AddHeader(name, value);

            reader.ReadChildren(SchemaStrings.ElementH, parent);
        }

        private static void ReadParameters(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            context.ClearParameters();

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
            string signatureMinusReturnType = RemoveReturnTypeFrom(signature);

            return new StackFrame
            {
                Location = new Location
                {
                    LogicalLocation = new LogicalLocation
                    {
                        FullyQualifiedName = signatureMinusReturnType
                    }
                }
            };
        }

        private static readonly Regex LogicalLocationRegex =
            new Regex(
                @"^
                  ([^\s]*\s+)?         # Skip over an optional leading blank-terminated return type name such as 'void '.
                  (?<fqln>.*)          # Take everything else.
                $",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private static string RemoveReturnTypeFrom(string signature)
        {
            Match match = LogicalLocationRegex.Match(signature);
            if (match.Success)
            {
                signature = match.Groups["fqln"].Value;
            }

            return signature;
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
            // p elements occur in two places:
            //
            // 1. As children of finding/request/parameters. In this context, they have
            //    attributes "name" and "value", and should be added to the context's
            //    Parameters dictionary.
            //
            // 2. As children of findings/events/propagation-event/properties, in which case
            //    they have attributes "k" and "v" and I don't know what to do with them yet.
            //
            // Make sure we don't confuse the one with the other.
            string name = reader.ReadAttributeString(SchemaStrings.AttributeName);
            if (name != null)
            {
                string value = reader.ReadAttributeString(SchemaStrings.AttributeValue);

                Context context = (Context)parent;
                context.AddParameter(name, value);
            }

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
                Location = context.Signature.Location,

                Stack = new Stack
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

        private static void ReadEvidence(SparseReader reader, object parent)
        {
            Context context = (Context)parent;
            string evidence = reader.ReadElementContentAsString();
            context.RefineEvidence(evidence);
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