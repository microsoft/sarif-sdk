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
        private const string ERR997_MissingCommandlineArgument = "ERR997.MissingCommandlineArgument";

        private const string ERR997_NoRulesLoaded = "ERR997.NoRulesLoaded";
        private const string ERR997_FileAlreadyExists = "ERR997.FileAlreadyExists";
        internal const string ERR997_NoPluginsConfigured = "ERR997.NoPluginsConfigured";
        private const string ERR997_ErrorPostingLogFile = "ERR997.ErrorPostingLogFile";
        private const string ERR997_ExceptionLoadingPlugIn = "ERR997.ExceptionLoadingPlugIn";
        internal const string ERR997_NoValidAnalysisTargets = "ERR997.NoValidAnalysisTargets";
        private const string ERR997_ExceptionAccessingFile = "ERR997.ExceptionAccessingFile";
        internal const string ERR997_IncompatibleRulesDetected = "ERR997.IncompatibleRulesDetected";
        internal const string ERR997_AllRulesExplicitlyDisabled = "ERR997.AllRulesExplicitlyDisabled";
        private const string ERR997_ExceptionCreatingOutputFile = "ERR997.ExceptionCreatingOutputFile";
        private const string ERR997_InvalidInvocationPropertyName = "ERR997.InvalidInvocationPropertyName";
        private const string ERR997_MissingReportingConfiguration = "ERR997.MissingReportingConfiguration";
        private const string ERR997_ExceptionLoadingAnalysisTarget = "ERR997.ExceptionLoadingAnalysisTarget";
        private const string ERR997_ExceptionInstantiatingSkimmers = "ERR997.ExceptionInstantiatingSkimmers";

        // Rule disabling tool errors:
        private const string ERR998_ExceptionInAnalyze = "ERR998.ExceptionInAnalyze";
        private const string ERR998_ExceptionInCanAnalyze = "ERR998.ExceptionInCanAnalyze";
        private const string ERR998_ExceptionInInitialize = "ERR998.ExceptionInInitialize";

        // Analysis halting tool errors:
        private const string ERR999_AnalysisCanceled = "ERR999.AnalysisCanceled";
        private const string ERR999_AnalysisTimedOut = "ERR999.AnalysisTimedOut";
        private const string ERR999_UnhandledEngineException = "ERR999.UnhandledEngineException";

        // Parse errors:
        private const string ERR1000_ParseError = "ERR1000.ParseError";

        public static void LogExceptionLoadingTarget(IAnalysisContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingTargetFile;

            string message = exception.Message;
            string exceptionType = exception.GetType().Name;

            // Could not load analysis target '{0}' ({1} : '{2}').
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.CurrentTarget.Uri,
                    ERR997_ExceptionLoadingAnalysisTarget,
                    ruleId: null,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: true,
                    messageFormat: null,
                    context.CurrentTarget.Uri.GetFileName(),
                    exceptionType,
                    message));
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

            context.RuntimeErrors |= RuntimeConditions.ExceptionInstantiatingSkimmers;

            string plugins = string.Join(", ",
                skimmerAssemblies.Select(sa => '"' + Path.GetFileName(sa.Location) + '"'));

            // Could not instantiate skimmers from the following plugins: {0}
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_ExceptionInstantiatingSkimmers,
                    ruleId: null,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: false,
                    messageFormat: null,
                    plugins));
        }

        public static void LogNoRulesLoaded(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;

            // No analysis rules could be instantiated.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_NoRulesLoaded,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null));
        }

        public static void LogAllRulesExplicitlyDisabled(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;

            // All rules were explicitly disabled so there is no work to do.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_AllRulesExplicitlyDisabled,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null));
        }
        public static void LogNoPluginsConfigured(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;

            // No analysis plugins were configured, therefore no rules loaded.
            context.Logger.LogConfigurationNotification(
                    Errors.CreateNotification(
                        null,
                        ERR997_NoPluginsConfigured,
                        ruleId: null,
                        FailureLevel.Error,
                        exception: null,
                        persistExceptionStack: false,
                        messageFormat: null));

        }

        public static void LogNoValidAnalysisTargets(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.NoValidAnalysisTargets;

            // No valid analysis targets were specified.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_NoValidAnalysisTargets,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null));

        }

        public static void LogExceptionCreatingOutputFile(IAnalysisContext context, string fileName, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionCreatingOutputFile;

            // Could not create output file: '{0}'
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_ExceptionCreatingOutputFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: exception,
                    persistExceptionStack: false,
                    messageFormat: null,
                    fileName));
        }

        public static void LogMissingFile(IAnalysisContext context, string fileName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.MissingFile;

            // A required file specified on the command line could not be found: '{0}'. 
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_MissingFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    Path.GetFullPath(fileName)));
        }

        public static void LogMissingCommandlineArgument(IAnalysisContext context, string missing, string required)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;

            // The {0} argument(s) must be specified when using {1}.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_MissingCommandlineArgument,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    missing,
                    required));

        }

        public static void LogExceptionAccessingFile(IAnalysisContext context, string fileName, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionAccessingFile;

            // Could not access a file specified on the command-line: '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_ExceptionAccessingFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: exception,
                    persistExceptionStack: false,
                    messageFormat: null,
                    fileName));
        }

        public static void LogInvalidInvocationPropertyName(IAnalysisContext context, string propertyName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;

            // '{0}' is not a property of the Invocation object.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_InvalidInvocationPropertyName,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    propertyName));
        }

        public static void LogMissingReportingConfiguration(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.RuleMissingRequiredConfiguration;

            var assembly = Assembly.GetEntryAssembly();
            assembly ??= Assembly.GetExecutingAssembly();
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
                context.CurrentTarget.Uri.GetFileName(),
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
                                    Uri = context.CurrentTarget.Uri
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
        }

        public static void LogExceptionLoadingPlugin(string pluginFilePath, IAnalysisContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingAnalysisPlugin;

            // Could not load plug-in '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_ExceptionLoadingPlugIn,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: exception,
                    persistExceptionStack: false,
                    messageFormat: null,
                    pluginFilePath));
        }

        public static void LogFileAlreadyExists(IAnalysisContext context, string filePath)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.FileAlreadyExists;

            // The output file '{0}' already exists. Use --log ForceOverwrite to overwrite.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_FileAlreadyExists,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    filePath));
        }

        public static void LogTargetParseError(IAnalysisContext context, Region region, string message)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.TargetParseError;

            // {0}({1}): error {2}: {3}
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.CurrentTarget.Uri,
                    ERR1000_ParseError,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    context.CurrentTarget.Uri.GetFileName(),
                    region.FormatForVisualStudio(),
                    ERR1000_ParseError,
                    message));
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

            context.RuntimeErrors |= RuntimeConditions.ExceptionRaisedInSkimmerCanAnalyze;

            // An unhandled exception was raised attempting to determine whether '{0}'
            // is a valid analysis target for check '{1}' (which has been disabled 
            // for the remainder of the analysis). The exception may have resulted 
            // from a problem related to parsing image metadata and not specific to 
            // the rule, however.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.CurrentTarget.Uri,
                    ERR998_ExceptionInCanAnalyze,
                    context.Rule.Id,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: true,
                    messageFormat: null,
                    context.CurrentTarget.Uri.GetFileName(),
                    context.Rule.Name));

            if (disabledSkimmers != null)
            {
                lock (disabledSkimmers)
                {
                    disabledSkimmers.Add(context.Rule.Id);
                }
            }
        }

        public static void LogUnhandledExceptionInitializingRule(IAnalysisContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;

            // An unhandled exception was encountered initializing check '{0}', which 
            // has been disabled for the remainder of the analysis.
            context.Logger.LogToolNotification(
                CreateNotification(
                uri: null,
                ERR998_ExceptionInInitialize,
                context.Rule.Id,
                FailureLevel.Error,
                exception,
                persistExceptionStack: true,
                messageFormat: null,
                context.Rule.Name),
                context.Rule);
        }

        public static void LogUnhandledRuleExceptionAnalyzingTarget(
            ISet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerAnalyze;

            // An unhandled exception of type '{0}' was encountered analyzing
            // '{0}' for check '{1}' (which has been disabled for the
            // remainder of the analysis. The exception may have resulted
            // from a problem related to parsing target metadata and not
            // specific to the rule, however.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.CurrentTarget.Uri,
                    ERR998_ExceptionInAnalyze,
                    context.Rule.Id,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: true,
                    messageFormat: null,
                    exception.GetType().Name,
                    context.CurrentTarget.Uri.GetFileName(),
                    context.Rule.Name),
                    context.Rule);

            if (disabledSkimmers != null)
            {
                lock (disabledSkimmers)
                {
                    disabledSkimmers.Add(context.Rule.Id);
                }
            }
        }

        public static void LogUnhandledEngineException(IAnalysisContext context, Exception exception)
        {
            if (context?.Logger == null)
            {
                // TBD construct a notification and emit it directly to console.
                Console.Error.WriteLine(exception.ToString());
                return;
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionInEngine;

            // An unhandled exception was raised during analysis.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.CurrentTarget?.Uri,
                    ERR999_UnhandledEngineException,
                    ruleId: null,
                    FailureLevel.Error,
                    exception,
                    persistExceptionStack: true,
                    messageFormat: "{0}",
                    args: new string[] { exception.ToString() }));
        }

        public static void LogIncompatibleRules(IAnalysisContext context, string ruleId, string incompatibleRuleId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // The current configuration enables rules that are not compatible
            // ('{0}' has declared that it is not compatible with '{1}'). You
            // can selectively disable one of the rules using an updated XML
            // configuration (passed by the --config argument).
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_IncompatibleRulesDetected,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: SdkResources.ERR997_IncompatibleRulesDetected,
                    ruleId,
                    incompatibleRuleId));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreRulesAreIncompatible;
        }

        public static Notification CreateNotification(Uri uri,
                                                      string notificationId,
                                                      string ruleId,
                                                      FailureLevel level,
                                                      Exception exception,
                                                      bool persistExceptionStack,
                                                      string messageFormat,
                                                      params string[] args)
        {
            messageFormat ??= GetMessageFormatResourceForNotification(notificationId);

            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            ExceptionData exceptionData = exception != null && persistExceptionStack
                ? ExceptionData.Create(exception.InnerException ?? exception)
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

            if (exceptionData != null)
            {
                physicalLocation ??= exceptionData.Stack?.Frames?[0].Location?.PhysicalLocation;
            }

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
                //TimeUtc = DateTime.UtcNow,
                Exception = exceptionData,
                Message = new Message { Text = message },
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

        internal static void LogAnalysisCanceled<TContext>(TContext context, OperationCanceledException ex) where TContext : IAnalysisContext, new()
        {
            if (context.RuntimeErrors.HasFlag(RuntimeConditions.AnalysisCanceled))
            {
                return;
            }

            lock (context)
            {
                // If we've already emitted the canceled message, don't repeat it.
                if (!context.RuntimeErrors.HasFlag(RuntimeConditions.AnalysisCanceled))
                {
                    // Analysis was canceled.
                    context.Logger.LogConfigurationNotification(
                    CreateNotification(
                            uri: null,
                            ERR999_AnalysisCanceled,
                            ruleId: null,
                            FailureLevel.Error,
                            exception: ex,
                            persistExceptionStack: false,
                            messageFormat: null));

                    context.RuntimeErrors |= RuntimeConditions.AnalysisCanceled;
                }
            }
        }

        internal static void LogAnalysisTimedOut(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.AnalysisTimedOut;

            string configuredTimeout = TimeSpan.FromMilliseconds(context.TimeoutInMilliseconds).ToString();

            // Analysis timed out. Timeout specified was {0}).
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR999_AnalysisTimedOut,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    configuredTimeout));
        }

        public static void LogErrorPostingLogFile(IAnalysisContext context, string postUri)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RuntimeErrors |= RuntimeConditions.ExceptionPostingLogFile;

            // Could not post to the URI specified on the command line: '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    uri: null,
                    ERR997_ErrorPostingLogFile,
                    ruleId: null,
                    FailureLevel.Error,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    postUri));
        }
    }
}
