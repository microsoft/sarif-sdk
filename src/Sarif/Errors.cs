// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Errors
    {
        // Configuration errors:
        private const string ERR997_MissingFile = "ERR997.MissingFile";
        private const string ERR997_NoRulesLoaded = "ERR997.NoRulesLoaded";
        private const string ERR997_ExceptionLoadingPlugIn = "ERR997.ExceptionLoadingPlugIn";
        private const string ERR997_NoValidAnalysisTargets = "ERR997.NoValidAnalysisTargets";
        private const string ERR997_ExceptionAccessingFile = "ERR997.ExceptionAccessingFile";
        private const string ERR997_MissingReportingConfiguration = "ERR997.MissingReportingConfiguration";
        private const string ERR997_ExceptionCreatingLogFile = "ERR997.ExceptionCreatingLogFile";
        internal const string ERR997_AllRulesExplicitlyDisabled = "ERR997.AllRulesExplicitlyDisabled";
        private const string ERR997_InvalidInvocationPropertyName = "ERR997.InvalidInvocationPropertyName";
        private const string ERR997_ExceptionLoadingAnalysisTarget = "ERR997.ExceptionLoadingAnalysisTarget";
        private const string ERR997_ExceptionInstantiatingSkimmers = "ERR997.ExceptionInstantiatingSkimmers";
        private const string ERR997_OutputFileAlreadyExists = "ERR997.OutputFileAlreadyExists";

        // Rule disabling tool errors:
        private const string ERR998_ExceptionInCanAnalyze = "ERR998.ExceptionInCanAnalyze";
        private const string ERR998_ExceptionInInitialize = "ERR998.ExceptionInInitialize";
        private const string ERR998_ExceptionInAnalyze = "ERR998.ExceptionInAnalyze";

        // Analysis halting tool errors:
        private const string ERR999UnhandledEngineException = "ERR999.UnhandledEngineException";

        // Parse errors:
        private const string ERR1000_ParseError = "ERR1000.ParseError";

        public static void LogExceptionLoadingTarget(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Could not load analysis target '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_ExceptionLoadingAnalysisTarget,
                    ruleId: null,
                    FailureLevel.Error,
                    context.TargetLoadException,
                    persistExceptionStack: false,
                    messageFormat: null,
                    context.TargetUri.GetFileName()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingTargetFile;
        }

        public static void LogExceptionInstantiatingSkimmers(
            IAnalysisContext context,
            IEnumerable<Assembly> skimmerAssemblies,
            Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string plugins = string.Join(", ",
                skimmerAssemblies.Select(sa => '"' + Path.GetFileName(sa.Location) + '"'));

            // Could not instantiate skimmers from the following plugins: {0}
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_ExceptionInstantiatingSkimmers,
                    ruleId: null,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: false,
                    messageFormat: null,
                    plugins));

            context.RuntimeErrors |= RuntimeConditions.ExceptionInstantiatingSkimmers;
        }

        public static void LogNoRulesLoaded(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // No analysis rules could be instantiated.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_NoRulesLoaded,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null));

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;
        }


        public static void LogAllRulesExplicitlyDisabled(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // All rules were explicitly disabled so there is no work to do.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_AllRulesExplicitlyDisabled,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null));

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;
        }

        public static void LogNoValidAnalysisTargets(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // No valid analysis targets were specified.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_NoValidAnalysisTargets,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null));

            context.RuntimeErrors |= RuntimeConditions.NoValidAnalysisTargets;
        }

        public static void LogExceptionCreatingLogFile(IAnalysisContext context, string fileName, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Could not create output file: '{0}'
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_ExceptionCreatingLogFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.ExceptionCreatingLogFile;
        }

        public static void LogMissingFile(IAnalysisContext context, string fileName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // A required file specified on the command line could not be found: '{0}'. 
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_MissingFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.MissingFile;
        }

        public static void LogExceptionAccessingFile(IAnalysisContext context, string fileName, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Could not access a file specified on the command-line: '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_ExceptionAccessingFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.ExceptionAccessingFile;
        }

        public static void LogInvalidInvocationPropertyName(IAnalysisContext context, string propertyName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // '{0}' is not a property of the Invocation object.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_InvalidInvocationPropertyName,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    propertyName));

            context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
        }

        public static void LogMissingreportingConfiguration(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Assembly assembly = Assembly.GetEntryAssembly();
            assembly = assembly ?? Assembly.GetExecutingAssembly();
            string exeName = Path.GetFileName(assembly.Location);

            // Check '{0}' was disabled while analyzing '{1}' because the analysis
            // was not configured with required policy ({2}). To resolve this,
            // configure and provide a policy file on the {3} command-line using
            // the --policy argument (recommended), or pass '--config default'
            // to invoke built-in settings. Invoke the {3} 'exportConfig' command
            // to produce an initial configuration file that can be edited, if
            // necessary, and passed back into the tool.
            string message = string.Format(CultureInfo.InvariantCulture, SdkResources.ERR997_MissingReportingConfiguration,
                context.Rule.Name,
                context.TargetUri.GetFileName(),
                reasonForNotAnalyzing,
                exeName);

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = context.TargetUri
                                },
                            }
                        }
                    },
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = ERR997_MissingReportingConfiguration,
                    },
                    AssociatedRule = new ReportingDescriptorReference
                    {
                        Id = context.Rule.Id,
                    },
                    Level = FailureLevel.Error,
                    Message = new Message { Text = message }
                });

            context.RuntimeErrors |= RuntimeConditions.RuleMissingRequiredConfiguration;
        }

        public static void LogExceptionLoadingPlugin(string pluginFilePath, IAnalysisContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Could not load plug-in '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_ExceptionLoadingPlugIn,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    pluginFilePath));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingAnalysisPlugin;
        }

        public static void LogOutputFileAlreadyExists(IAnalysisContext context, string outputFilePath)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR997_OutputFileAlreadyExists,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    outputFilePath));

            context.RuntimeErrors |= RuntimeConditions.OutputFileAlreadyExists;
        }

        public static void LogTargetParseError(IAnalysisContext context, Region region, string message)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // {0}({1}): error {2}: {3}
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR1000_ParseError,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    context.TargetUri.LocalPath,
                    region.FormatForVisualStudio(),
                    ERR1000_ParseError,
                    message));

            context.RuntimeErrors |= RuntimeConditions.TargetParseError;
        }

        public static void LogUnhandledRuleExceptionAssessingTargetApplicability(
            ISet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // An unhandled exception was raised attempting to determine whether '{0}'
            // is a valid analysis target for check '{1}' (which has been disabled 
            // for the remainder of the analysis). The exception may have resulted 
            // from a problem related to parsing image metadata and not specific to 
            // the rule, however.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR998_ExceptionInCanAnalyze,
                    context.Rule.Id,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: true,
                    messageFormat: null,
                    context.TargetUri.GetFileName(),
                    context.Rule.Name));

            if (disabledSkimmers != null) { disabledSkimmers.Add(context.Rule.Id); }

            context.RuntimeErrors |= RuntimeConditions.ExceptionRaisedInSkimmerCanAnalyze;
        }


        public static void LogUnhandledExceptionInitializingRule(IAnalysisContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // An unhandled exception was encountered initializing check '{0}', which 
            // has been disabled for the remainder of the analysis.
            context.Logger.LogToolNotification(
                CreateNotification(
                context.TargetUri,
                ERR998_ExceptionInInitialize,
                context.Rule.Id,
                FailureLevel.Error,
                exception,
                persistExceptionStack: true,
                messageFormat: null,
                context.Rule.Name));

            context.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
        }

        public static RuntimeConditions LogUnhandledRuleExceptionAnalyzingTarget(
            ISet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // An unhandled exception was encountered analyzing '{0}' for check '{1}', 
            // which has been disabled for the remainder of the analysis.The 
            // exception may have resulted from a problem related to parsing 
            // image metadata and not specific to the rule, however.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR998_ExceptionInAnalyze,
                    context.Rule.Id,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: false,
                    messageFormat: null,
                    context.TargetUri.GetFileName(),
                    context.Rule.Name));

            if (disabledSkimmers != null) { disabledSkimmers.Add(context.Rule.Id); }

            return RuntimeConditions.ExceptionInSkimmerAnalyze;
        }

        public static RuntimeConditions LogUnhandledEngineException(IAnalysisContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // An unhandled exception was raised during analysis.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.TargetUri,
                    ERR999UnhandledEngineException,
                    ruleId: null,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: true,
                    messageFormat: null));

            return RuntimeConditions.ExceptionInEngine;
        }

        public static Notification CreateNotification(
            Uri uri,
            string notificationId,
            string ruleId,
            FailureLevel level,
            Exception exception,
            bool persistExceptionStack,
            string messageFormat,
            params string[] args)
        {
            messageFormat = messageFormat ?? GetMessageFormatResourceForNotification(notificationId);

            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            string exceptionMessage = exception?.Message;
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                // {0} ('{1}')
                message = string.Format(CultureInfo.InvariantCulture, SdkResources.NotificationWithExceptionMessage, message, exceptionMessage);
            }

            ExceptionData exceptionData = exception != null && persistExceptionStack
                ? ExceptionData.Create(exception)
                : null;

            PhysicalLocation physicalLocation = uri != null
                ? new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = uri
                    },
                }
                : null;

            var notification = new Notification
            {
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = physicalLocation
                    }
                },
                Level = level,
                Message = new Message { Text = message },
                Exception = exceptionData
            };

            if (!string.IsNullOrWhiteSpace(notificationId))
            {
                notification.Descriptor = new ReportingDescriptorReference
                {
                    Id = notificationId,
                };
            }

            if (!string.IsNullOrWhiteSpace(ruleId))
            {
                notification.AssociatedRule = new ReportingDescriptorReference
                {
                    Id = ruleId,
                };
            }

            return notification;
        }

        private static string GetMessageFormatResourceForNotification(string notificationId)
        {
            string resourceName = notificationId.Replace('.', '_');

            return (string)typeof(SdkResources)
                            .GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static)
                            .GetValue(obj: null, index: null);
        }
    }
}
