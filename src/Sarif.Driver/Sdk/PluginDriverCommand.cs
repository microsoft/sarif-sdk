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

        public bool ValidateExport(string outputFilePath, string exportUri)
        {
            if (string.IsNullOrEmpty(exportUri))
            {
                return true;
            }

            if (string.IsNullOrEmpty(outputFilePath))
            {
                return false;
            }

            return true;
        }

        public bool ValidateFile(IAnalysisContext context, string filePath, string policyName, bool? shouldExist)
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

        public bool ValidateFiles(IAnalysisContext context, IEnumerable<string> filePaths, string policyName, bool shouldExist)
        {
            if (filePaths == null) { return true; }

            bool succeeded = true;

            foreach (string filePath in filePaths)
            {
                succeeded &= ValidateFile(context, filePath, policyName, shouldExist);
            }

            return succeeded;
        }

        public bool ValidateInvocationPropertiesToLog(IAnalysisContext context, IEnumerable<string> propertiesToLog)
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

        public bool ValidateOutputFileCanBeCreated(IAnalysisContext context, string outputFilePath, bool force)
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

            if (string.IsNullOrEmpty(options.Export))
            {
                return;
            }

            try
            {
                using var fileStream = fileSystem.FileOpenRead(options.OutputFilePath);

                var httpClient = new HttpClient();
                using var response = httpClient
                    .PostAsync(options.Export, new StreamContent(fileStream))
                    .GetAwaiter()
                    .GetResult();

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionExportingLogFile
                };
            }
        }
    }
}
