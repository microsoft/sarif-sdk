// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class ConsoleLogger : IResultLogger
    {
        public ConsoleLogger(bool verbose)
        {
            Verbose = verbose;
        }

        public bool Verbose { get; set; }

        public void Log(ResultKind messageKind, string formatSpecifier, params string[] arguments)
        {
            string message = String.Format(formatSpecifier, arguments);
            WriteToConsole(messageKind, null, null, message);
        }

        public void Log(ResultKind messageKind, IAnalysisContext context, string formatSpecifierId, params string[] arguments)
        {
            string formatSpecifier = context.Rule.FormatSpecifiers[formatSpecifierId];
            string message = String.Format(formatSpecifier, arguments);
            WriteToConsole(messageKind, context.TargetUri, context.Rule.Id, message);
        }

        private void WriteToConsole(ResultKind messageKind, Uri uri, string ruleId, string message)
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
                        Console.WriteLine(GetMessageText(uri, ruleId, message, messageKind));
                    }
                    break;
                }

                // These result types are alwayss emitted
                case ResultKind.Error:
                case ResultKind.Warning:
                case ResultKind.InternalError:
                case ResultKind.ConfigurationError:
                {
                    Console.WriteLine(GetMessageText(uri, ruleId, message, messageKind));
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
            string ruleId,
            string message, 
            ResultKind messageKind,
            Region region = null)
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
                    {
                        issueType = "CONFIGURATION ERROR";
                        break;
                    }

                case ResultKind.InternalError:
                    {
                        issueType = "INTERNAL ERROR";
                        break;
                }

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
                if (region.Offset > 0 || region.StartColumn == 0) { throw new NotImplementedException(); }

                if (region.StartLine == 0) { throw new InvalidOperationException(); }

                // VS supports the following formatting options:
                //      (startLine)
                //      (startLine-endLine)
                //      (startLine,startColumn)
                //      (startLine,startColumn-endColumn)
                //      (startLine,startColumn,endLine,endColumn
                //
                //  For expedience, we'll convert everything to the most fully qualified format

                string start = region.StartLine.ToString() + "," +
                              (region.StartColumn > 0 ? region.StartColumn.ToString() : "1");

                string end = (region.EndLine > region.StartLine ? region.EndLine.ToString() : region.StartLine.ToString()) + "," +
                             (region.EndColumn > 0 ? region.EndColumn.ToString() : region.StartColumn.ToString());

                location =
                    "(" +
                        start + (end != start ? "," + end : "") +
                    ")";
            }

            return (path != null ? (path + location + ": ") : "") +
                   issueType + (!string.IsNullOrEmpty(ruleId) ? " " : "")  +
                   ruleId + ": " +
                   detailedDiagnosis;
        }

        public static string NormalizeMessage(string message, bool enquote)
        {
            return (enquote ? "\"" : "") +
                message.Replace('"', '\'') +
                (enquote ? "\"" : "");
        }
    }
}