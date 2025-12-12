// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class PluginDriverCommand<T> : DriverCommand<T>
    {
        // The plugin assemblies that contain IOptionProvider instan
        private IEnumerable<Assembly> _defaultPluginAssemblies;
        public virtual IEnumerable<Assembly> DefaultPluginAssemblies
        {
            get
            {
                return _defaultPluginAssemblies ?? new[] { this.GetType().Assembly };
            }
            set { _defaultPluginAssemblies = value; }
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

        internal static bool ValidateBaselineFile(IAnalysisContext context)
        {
            bool required = !string.IsNullOrEmpty(context.BaselineFilePath);
            bool succeeded = ValidateFile(context,
                                          context.BaselineFilePath,
                                          shouldExist: required ? true : (bool?)null);

            if (required && succeeded && string.IsNullOrEmpty(context.OutputFilePath))
            {
                succeeded = false;
                Errors.LogMissingCommandlineArgument(context,
                                                     "'--output' or '--log Inline'",
                                                     "'--baseline'");
            }

            return succeeded;
        }

        internal static bool ValidateInvocationPropertiesToLog(IAnalysisContext context)
        {
            bool succeeded = true;

            if (context.InvocationPropertiesToLog?.Any() == true)
            {
                var validPropertyNames = new HashSet<string>(
                    typeof(Invocation).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(propInfo => propInfo.Name),
                    StringComparer.OrdinalIgnoreCase);

                foreach (string propertyName in context.InvocationPropertiesToLog)
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

        internal static bool ValidateFile(IAnalysisContext context, string filePath, bool? shouldExist)
        {
            if (filePath == null) { return true; }

            Exception exception = null;

            try
            {
                bool fileExists = context.FileSystem.FileExists(filePath);

                if (fileExists)
                {
                    if (shouldExist == null || shouldExist.Value)
                    {
                        return true;
                    }

                    Errors.LogFileAlreadyExists(context, filePath);
                }
                else
                {
                    if (shouldExist == null || !shouldExist.Value)
                    {
                        return true;
                    }

                    Errors.LogMissingFile(context, filePath);
                }
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

        public static bool ValidateFiles(IAnalysisContext context, IEnumerable<string> filePaths, bool? shouldExist)
        {
            if (filePaths == null) { return true; }

            bool succeeded = true;

            foreach (string filePath in filePaths)
            {
                succeeded &= ValidateFile(context, filePath, shouldExist);
            }

            return succeeded;
        }

        internal static bool ValidateOutputFileCanBeCreated(IAnalysisContext context)
        {
            bool succeeded = true;
            bool force = context.OutputFileOptions.HasFlag(FilePersistenceOptions.ForceOverwrite);

            if (!string.IsNullOrWhiteSpace(context.OutputFilePath) &&
                !DriverUtilities.CanCreateOutputFile(context.OutputFilePath, force, context.FileSystem))
            {
                Errors.LogFileAlreadyExists(context, context.OutputFilePath);
                succeeded = false;
            }

            return succeeded;
        }

        protected virtual void ProcessBaseline(IAnalysisContext context)
        {
            try
            {
                string baselineFilePath = context.BaselineFilePath;
                if (string.IsNullOrEmpty(baselineFilePath))
                {
                    return;
                }

                string outputFilePath = context.OutputFilePath;
                bool inline = context.OutputFileOptions.HasFlag(FilePersistenceOptions.Inline);
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
                    WriteSarifFile(context.FileSystem, baseline, targetFile, minify: !context.OutputFileOptions.HasFlag(FilePersistenceOptions.PrettyPrint));
                }
                catch (Exception ex)
                {
                    throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                    {
                        ExitReason = ExitReason.ExceptionWritingToLogFile
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                context.RuntimeErrors |= RuntimeConditions.ExceptionProcessingBaseline;
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionProcessingBaseline
                };
            }
        }

        protected virtual void PostLogFile(IAnalysisContext globalContext)
        {
            using HttpClientWrapper httpClient = GetHttpClientWrapper();
            SarifPost(globalContext, httpClient);
        }

        protected virtual HttpClientWrapper GetHttpClientWrapper()
        {
            return new HttpClientWrapper();
        }

        internal static void SarifPost(IAnalysisContext globalContext, HttpClientWrapper httpClient)
        {
            if (string.IsNullOrWhiteSpace(globalContext.PostUri) ||
                string.IsNullOrWhiteSpace(globalContext.OutputFilePath) ||
                !globalContext.FileSystem.FileExists(globalContext.OutputFilePath))
            {
                return;
            }

            try
            {
                var postUri = new Uri(globalContext.PostUri);
                string outputFilePath = globalContext.OutputFilePath;
                IFileSystem fileSystem = globalContext.FileSystem;

                (bool, string) result = SarifLog.Post(postUri, outputFilePath, fileSystem, httpClient)
                    .GetAwaiter()
                    .GetResult();

                // This reporting isn't sent through the 'globalContext.Loggers' property
                // because the post operation occurs after the backing SARIF log has 
                // already been finalized.
                if (!globalContext.Quiet || result.Item1 == false)
                {
                    Console.WriteLine(result.Item2);
                }

                if (result.Item1 == false && !result.Item2.Contains("skipped"))
                {
                    globalContext.RuntimeErrors |= RuntimeConditions.ExceptionPostingLogFile;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                globalContext.RuntimeErrors |= RuntimeConditions.ExceptionPostingLogFile;
                globalContext.RuntimeExceptions ??= new List<Exception>();
                globalContext.RuntimeExceptions.Add(ex);
                throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
                {
                    ExitReason = ExitReason.ExceptionPostingLogFile
                };
            }
        }
    }
}
