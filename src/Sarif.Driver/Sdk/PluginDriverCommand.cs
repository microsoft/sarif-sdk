// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class PluginDriverCommand<T> : DriverCommand<T>
    {
        // The plugin assemblies that contain IOptionProvider instances.
        public virtual IEnumerable<Assembly> DefaultPluginAssemblies
        {
            get => null;
            set => throw new InvalidOperationException();
        }

        // An additional IOptionsProvider instance, typically, the one
        // that exposes a client tool command-line interface.
        public virtual IOptionsProvider AdditionalOptionsProvider
        {
            get => null;
            set => throw new InvalidOperationException();
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
                bool fileExists = context.FileSystem.FileExists(filePath);

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

            if (!string.IsNullOrWhiteSpace(outputFilePath) &&
                !DriverUtilities.CanCreateOutputFile(outputFilePath, force, context.FileSystem))
            {
                Errors.LogOutputFileAlreadyExists(context, outputFilePath);
                succeeded = false;
            }

            return succeeded;
        }

        protected virtual void ProcessBaseline(IAnalysisContext context)
        {
            string baselineFilePath = context.BaselineFilePath;
            if (string.IsNullOrEmpty(baselineFilePath))
            {
                return;
            }

            bool inline = context.Inline;
            string outputFilePath = context.OutputFilePath;
            if (!inline && string.IsNullOrEmpty(outputFilePath))
            {
                return;
            }

            SarifLog baselineFile = JsonConvert.DeserializeObject<SarifLog>(context.FileSystem.FileReadAllText(baselineFilePath));
            SarifLog currentSarifLog = JsonConvert.DeserializeObject<SarifLog>(context.FileSystem.FileReadAllText(outputFilePath));

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
                string targetFile = inline ? baselineFilePath : outputFilePath;
                WriteSarifFile(context.FileSystem, baseline, targetFile, minify: !context.PrettyPrint);
            }
            catch (Exception ex)
            {
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionWritingToLogFile
                };
            }
        }

        protected virtual void PostLogFile(string postUri, string outputFilePath, IFileSystem fileSystem)
        {
            using (var httpClient = new HttpClient())
            {
                PostLogFile(postUri, outputFilePath, fileSystem, httpClient)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        internal static async Task PostLogFile(string postUri, string outputFilePath, IFileSystem fileSystem, HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(postUri))
            {
                return;
            }

            try
            {
                await SarifLog.Post(new Uri(postUri), outputFilePath, fileSystem, httpClient);
            }
            catch (Exception ex)
            {
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionPostingLogFile
                };
            }
        }
    }
}
