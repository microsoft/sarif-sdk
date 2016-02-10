// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class ConsoleLogger : IAnalysisLogger
    {
        public ConsoleLogger(bool verbose)
        {
            Verbose = verbose;
        }

        public bool Verbose { get; set; }


        public void AnalysisStarted()
        {
            Console.WriteLine(SdkResources.MSG_Analyzing);
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            Console.WriteLine();

            if (runtimeConditions == RuntimeConditions.NoErrors)
            {
                Console.WriteLine(SdkResources.MSG_AnalysisCompletedSuccessfully);
                return;
            }

            if ((runtimeConditions & RuntimeConditions.Fatal) != 0)
            {
                // One or more fatal conditions observed at runtime, so
                // we'll report a catastrophic exit (withuot paying
                // particular attention to anything non-fatal
                Console.WriteLine(SdkResources.MSG_UnexpectedApplicationExit);
            }
            else
            {
                // Analysis finished but was not complete due
                // to non-fatal runtime errors.
                Console.WriteLine(SdkResources.MSG_AnalysisIncomplete);
            }

            Console.WriteLine("Unexpected runtime condition(s) observed: " + runtimeConditions.ToString());
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            if (this.Verbose)
            {
                Console.WriteLine(string.Format(
                    SdkResources.MSG1001_AnalyzingTarget,
                        Path.GetFileName(context.TargetUri.LocalPath)));
            }
        }

        public void LogMessage(bool verbose, string message)
        {
            if (this.Verbose)
            {
                Console.WriteLine(message);
            }
        }

        public void Log(IRuleDescriptor rule, Result result)
        {
            string message = result.GetMessageText(rule, concise: false);

            // TODO we need better retrieval for locations than these defaults
            // Note that we can potentially emit many messages from a single result
            WriteToConsole(
                result.Kind,
                result.Locations?[0].AnalysisTarget?[0].Uri,
                result.Locations?[0].AnalysisTarget?[0].Region,
                result.RuleId,
                message);
        }

        private void WriteToConsole(ResultKind messageKind, Uri uri, Region region, string ruleId, string message)
        {
            switch (messageKind)
            {

                // These result types are optionally emitted
                case ResultKind.Pass:
                case ResultKind.Note:
                case ResultKind.NotApplicable:
                {
                    if (Verbose)
                    {
                        Console.WriteLine(GetMessageText(uri, region, ruleId, message, messageKind));
                    }
                    break;
                }

                // These result types are alwayss emitted
                case ResultKind.Error:
                case ResultKind.Warning:
                case ResultKind.InternalError:
                case ResultKind.ConfigurationError:
                {
                    Console.WriteLine(GetMessageText(uri, region, ruleId, message, messageKind));
                    break;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public static string GetMessageText(
            Uri uri, 
            Region region,
            string ruleId,
            string message, 
            ResultKind messageKind)
        {
            string path = null;

            if (uri != null)
            {
                // If a path refers to a URI of form file://blah, we will convert to the local path           
                if (uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeFile)
                {
                    path = uri.LocalPath;
                }
                else
                {
                    path = uri.ToString();
                }
            }

            string issueType = null;

            switch (messageKind)
            {
                case ResultKind.ConfigurationError:
                case ResultKind.InternalError:
                case ResultKind.Error:
                {
                    issueType = "error";
                    break;
                }

                case ResultKind.Warning:
                {
                    issueType = "warning";
                    break;
                }

                case ResultKind.NotApplicable:
                case ResultKind.Note:
                case ResultKind.Pass:
                {
                        issueType = "note";
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException("Unknown message kind:" + messageKind.ToString());
                    }
            }

            string detailedDiagnosis = NormalizeMessage(message, enquote: false);

            string location = "";

            if (region != null)
            {
                // TODO 
                if (region.CharOffset > 0 ||
                    region.ByteOffset > 0 ||
                    region.StartColumn == 0)
                {
                    throw new NotImplementedException();
                }

                if (region.StartLine == 0)
                {
                    throw new InvalidOperationException();
                }

                location = region.FormatForVisualStudio();
            }

            string result = (path != null ? (path + location + ": ") : "") +
                   issueType + (!string.IsNullOrEmpty(ruleId) ? " " : "")  +
                   (messageKind != ResultKind.Note ? ruleId : "" ) + ": " +
                   detailedDiagnosis;

            return result;
        }

        public static string NormalizeMessage(string message, bool enquote)
        {
            return (enquote ? "\"" : "") +
                message.Replace('"', '\'') +
                (enquote ? "\"" : "");
        }
    }
}