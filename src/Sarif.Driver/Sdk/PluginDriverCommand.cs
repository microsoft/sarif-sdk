// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class PluginDriverCommand<T> : DriverCommand<T>
    {
        private HttpClient _httpClient;

        public virtual IEnumerable<Assembly> DefaultPluginAssemblies
        {
            get { return null; }
            set { throw new InvalidOperationException(); }
        }

        public IEnumerable<Assembly> RetrievePluginAssemblies(IEnumerable<Assembly> defaultPluginAssemblies, IEnumerable<string> pluginFilePaths)
        {
            if (pluginFilePaths == null)
            {
                return DefaultPluginAssemblies;
            }

            var assemblies = new List<Assembly>(defaultPluginAssemblies);
            foreach (string pluginFilePath in pluginFilePaths)
            {
                assemblies.Add(Assembly.LoadFrom(pluginFilePath));
            }

            return assemblies;
        }

        internal static bool ValidatePostUri(string outputFilePath, string postUri)
        {
            if (string.IsNullOrEmpty(postUri))
            {
                return true;
            }

            // If postUri is not empty, then outputFilePath must exist.
            if (string.IsNullOrEmpty(outputFilePath))
            {
                return false;
            }

            return Uri.IsWellFormedUriString(postUri, UriKind.Absolute);
        }

        internal static bool ValidateInvocationPropertiesToLog(IAnalysisContext context, IEnumerable<string> propertiesToLog)
        {
            bool succeeded = true;

            if (propertiesToLog != null)
            {
                var validPropertyNames = new HashSet<string>(
                    typeof(Invocation).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(propInfo => propInfo.Name),
                    StringComparer.OrdinalIgnoreCase);

                foreach (string propertyName in propertiesToLog)
                {
                    if (!validPropertyNames.Contains(propertyName))
                    {
                        Errors.LogInvalidInvocationPropertyName(context, propertyName);
                        succeeded = false;
                    }
                }
            }

            return succeeded;
        }

        internal bool ValidateFile(IAnalysisContext context, string filePath, string policyName, bool? shouldExist)
        {
            if (filePath == null || filePath == policyName) { return true; }

            Exception exception = null;

            try
            {
                bool fileExists = FileSystem.FileExists(filePath);

                if (fileExists || shouldExist == null || !shouldExist.Value)
                {
                    return true;
                }

                Errors.LogMissingFile(context, filePath);
            }
            catch (IOException ex) { exception = ex; }
            catch (SecurityException ex) { exception = ex; }
            catch (UnauthorizedAccessException ex) { exception = ex; }

            if (exception != null)
            {
                Errors.LogExceptionAccessingFile(context, filePath, exception);
            }

            return false;
        }

        internal bool ValidateFiles(IAnalysisContext context, IEnumerable<string> filePaths, string policyName, bool shouldExist)
        {
            if (filePaths == null) { return true; }

            bool succeeded = true;

            foreach (string filePath in filePaths)
            {
                succeeded &= ValidateFile(context, filePath, policyName, shouldExist);
            }

            return succeeded;
        }

        internal bool ValidateOutputFileCanBeCreated(IAnalysisContext context, string outputFilePath, bool force)
        {
            bool succeeded = true;

            if (!DriverUtilities.CanCreateOutputFile(outputFilePath, force, FileSystem))
            {
                Errors.LogOutputFileAlreadyExists(context, outputFilePath);
                succeeded = false;
            }

            return succeeded;
        }

        protected virtual void ProcessBaseline(IAnalysisContext context, T driverOptions, IFileSystem fileSystem)
        {
            if (!(driverOptions is AnalyzeOptionsBase options))
            {
                return;
            }

            if (string.IsNullOrEmpty(options.BaselineSarifFile) || string.IsNullOrEmpty(options.OutputFilePath))
            {
                return;
            }

            SarifLog baselineFile = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(fileSystem.FileReadAllText(options.BaselineSarifFile),
                                                                                              options.Formatting,
                                                                                              out string _);

            SarifLog currentSarifLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(fileSystem.FileReadAllText(options.OutputFilePath),
                                                                                                 options.Formatting,
                                                                                                 out string _);

            SarifLog baseline;
            try
            {
                ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
                baseline = matcher.Match(new SarifLog[] { baselineFile }, new SarifLog[] { currentSarifLog }).First();
            }
            catch (Exception ex)
            {
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionProcessingBaseline
                };
            }

            try
            {
                string targetFile = options.Inline ? options.BaselineSarifFile : options.OutputFilePath;

                if (options.SarifOutputVersion == SarifVersion.OneZeroZero)
                {
                    var visitor = new SarifCurrentToVersionOneVisitor();
                    visitor.VisitSarifLog(baseline);

                    WriteSarifFile(fileSystem, visitor.SarifLogVersionOne, targetFile, options.Formatting, SarifContractResolverVersionOne.Instance);
                }
                else
                {
                    WriteSarifFile(fileSystem, baseline, targetFile, options.Formatting);
                }
            }
            catch (Exception ex)
            {
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionWritingToLogFile
                };
            }
        }

        protected virtual void Export(IAnalysisContext context, T driverOptions, IFileSystem fileSystem)
        {
            if (!(driverOptions is AnalyzeOptionsBase options))
            {
                return;
            }

            if (string.IsNullOrEmpty(options.PostUri))
            {
                return;
            }

            try
            {
                using Stream fileStream = fileSystem.FileOpenRead(options.OutputFilePath);
                using var streamContent = new StreamContent(fileStream);
                using HttpClient httpClient = _httpClient ?? new HttpClient();
                using HttpResponseMessage response = httpClient
                    .PostAsync(options.PostUri, streamContent)
                    .GetAwaiter()
                    .GetResult();

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionPostingLog
                };
            }
        }

        /// <summary>
        /// This method will be used to mock the HttpClient so we can add tests.
        /// </summary>
        /// <param name="httpClient"></param>
        internal void SetHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
