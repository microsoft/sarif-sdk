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
        private const string Notification_ExceptionLoadingAnalysisTarget = "ExceptionLoadingAnalysisTarget";
        private const string Notification_ExceptionLoadingPdb = "ExceptionLoadingPdb";
        private const string Notification_ExceptionInstantiatingSkimmers = "ExceptionInstantiatingSkimmers";
        private const string Notification_NoRulesLoaded = "NoRulesLoaded";
        private const string Notification_NoValidAnalysisTargets = "NoValidAnalysisTargets";
        private const string Notification_ExceptionCreatingLogFile = "ExceptionCreatingLogFile";
        private const string Notification_MissingFile = "MissingFile";
        private const string Notification_ExceptionAccessingFile = "ExceptionAccessingFile";
        private const string Notification_MissingRuleConfiguration = "MissingRuleConfiguration";
        private const string Notification_ExceptionLoadingPlugIn = "ExceptionLoadingPlugIn";

        // Rule disabling tool errors:
        private const string Notification_ExceptionInCanAnalyze = "ExceptionInCanAnalyze";
        private const string Notification_ExceptionInInitialize = "ExceptionInInitialize";
        private const string Notification_ExceptionInAnalyze = "ExceptionInAnalyze";

        // Analysis halting tool errors:
        private const string Notification_UnhandledEngineException = "UnhandledEngineException";

        // Parse errors:
        private const string Notification_ParseError = "ParseError";

        public static void LogExceptionLoadingTarget(IAnalysisContext context)
        {
            // Could not load analysis target '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionLoadingAnalysisTarget,
                    NotificationLevel.Error,
                    context.TargetLoadException,
                    false,
                    context.TargetUri.LocalPath));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingTargetFile;
        }

        public static void LogExceptionLoadingPdb(IAnalysisContext context, Exception exception)
        {
            string ruleName = context.Rule.Name;

            // '{0}' was not evaluated for check '{1}' because its PDB could not be loaded.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionLoadingPdb,
                    context.Rule.Id,
                    NotificationLevel.Error,
                    exception,
                    false,
                    context.TargetUri.LocalPath,
                    context.Rule.Name));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingPdb;
        }

        public static void LogExceptionInstantiatingSkimmers(
            IAnalysisContext context,
            IEnumerable<Assembly> skimmerAssemblies,
            Exception exception)
        {
            string plugins = string.Join(", ",
                skimmerAssemblies.Select(sa => '"' +  Path.GetFileName(sa.Location) + '"'));

            // Could not instantiate skimmers from the following plugins: {0}
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionInstantiatingSkimmers,
                    NotificationLevel.Error,
                    exception,
                    false,
                    plugins));

            context.RuntimeErrors |= RuntimeConditions.ExceptionInstantiatingSkimmers;
        }

        public static void LogNoRulesLoaded(IAnalysisContext context)
        {
            // No analysis rules could be instantiated.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_NoRulesLoaded,
                    NotificationLevel.Error,
                    null,
                    false));

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;
        }

        public static void LogNoValidAnalysisTargets(IAnalysisContext context)
        {
            // No valid analysis targets were specified.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_NoValidAnalysisTargets,
                    NotificationLevel.Error,
                    null,
                    false));

            context.RuntimeErrors |= RuntimeConditions.NoValidAnalysisTargets;
        }

        public static void LogExceptionCreatingLogFile(IAnalysisContext context, string fileName, Exception exception)
        {
            // Could not create output file: '{0}'
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionCreatingLogFile,
                    NotificationLevel.Error,
                    exception,
                    false,
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.ExceptionCreatingLogfile;
        }

        public static void LogMissingFile(IAnalysisContext context, string fileName)
        {
            // A required file specified on the command line could not be found: '{0}'. 
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_MissingFile,
                    NotificationLevel.Error,
                    null,
                    false,
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.MissingFile;
        }

        public static void LogExceptionAccessingFile(IAnalysisContext context, string fileName, Exception exception)
        {
            // Could not access a file specified on the command-line: '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionAccessingFile,
                    NotificationLevel.Error,
                    exception,
                    false,
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.ExceptionAccessingFile;
        }

        public static void LogMissingRuleConfiguration(IAnalysisContext context, string reasonForNotAnalyzing)
        {            
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
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_MissingRuleConfiguration,
                    NotificationLevel.Error,
                    null,
                    false,
                    context.Rule.Name,
                    string.Empty,           // BUG: There were fewer arguments specified than required by the format string
                    reasonForNotAnalyzing,  // ... and it doesn't look like this fits with the message for {2}
                    exeName));              // ... but this is pretty clearly {3}.

            context.RuntimeErrors |= RuntimeConditions.RuleMissingRequiredConfiguration;
        }

        public static void LogExceptionLoadingPlugIn(string plugInFilePath, IAnalysisContext context, Exception exception)
        {
            // Could not load plug-in '{0}'.
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionLoadingPlugIn,
                    NotificationLevel.Error,
                    exception,
                    false,  
                    plugInFilePath));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingAnalysisPlugIn;
        }

        public static void LogTargetParseError(IAnalysisContext context, Region region, string message)
        {
            // {0}({1}): error {2}: {3}
            context.Logger.LogConfigurationNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ParseError,
                    NotificationLevel.Error,
                    null,
                    false,
                    context.TargetUri.LocalPath,
                    region.FormatForVisualStudio(),
                    Notification_ParseError,
                    message));

            context.RuntimeErrors |= RuntimeConditions.TargetParseError;
        }

        public static void LogUnhandledRuleExceptionAssessingTargetApplicability(
            HashSet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception exception)
        {
            // An unhandled exception was raised attempting to determine whether '{0}'
            // is a valid analysis target for check '{1}' (which has been disabled 
            // for the remainder of the analysis). The exception may have resulted 
            // from a problem related to parsing image metadata and not specific to 
            // the rule, however.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionInCanAnalyze,
                    context.Rule.Id,
                    NotificationLevel.Error,
                    exception,
                    true,
                    context.TargetUri.LocalPath,
                    context.Rule.Name));

            if (disabledSkimmers != null) { disabledSkimmers.Add(context.Rule.Id); }

            context.RuntimeErrors |= RuntimeConditions.ExceptionRaisedInSkimmerCanAnalyze;
        }


        public static void LogUnhandledExceptionInitializingRule(IAnalysisContext context, Exception exception)
        {
            string ruleId = context.Rule.Id;
            string ruleName = context.Rule.Name;

            // An unhandled exception was encountered initializing check '{0}', which 
            // has been disabled for the remainder of the analysis.
            context.Logger.LogToolNotification(
                CreateNotification(
                context.TargetUri,
                Notification_ExceptionInInitialize,
                context.Rule.Id,
                NotificationLevel.Error,
                exception,
                true,
                context.Rule.Name));

            context.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
        }

        public static RuntimeConditions LogUnhandledRuleExceptionAnalyzingTarget(
            HashSet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception exception)
        {
            // An unhandled exception was encountered analyzing '{0}' for check '{1}', 
            // which has been disabled for the remainder of the analysis.The 
            // exception may have resulted from a problem related to parsing 
            // image metadata and not specific to the rule, however.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_ExceptionInAnalyze,
                    context.Rule.Id,
                    NotificationLevel.Error,
                    exception,
                    true,
                    context.TargetUri.LocalPath,
                    context.Rule.Name));

            if (disabledSkimmers != null) { disabledSkimmers.Add(context.Rule.Id); }

            return RuntimeConditions.ExceptionInSkimmerAnalyze;
        }

        public static RuntimeConditions LogUnhandledEngineException(IAnalysisContext context, Exception exception)
        {
            // An unhandled exception was raised during analysis.
            context.Logger.LogToolNotification(
                CreateNotification(
                    context.TargetUri,
                    Notification_UnhandledEngineException,
                    NotificationLevel.Error,
                    exception,
                    true));

            return RuntimeConditions.ExceptionInEngine;
        }

        private static Notification CreateNotification(
            Uri uri,
            string notificationId,
            NotificationLevel level,
            Exception exception,
            bool persistExceptionStack,
            params object[] args)
        {
            return CreateNotification(uri, notificationId, null, level, exception, persistExceptionStack, args);
        }

        private static Notification CreateNotification(
            Uri uri,
            string notificationId,
            string ruleId,
            NotificationLevel level,
            Exception exception,
            bool persistExceptionStack,
            params object[] args)
        {            
            string messageFormat = GetMessageFormatResourceForNotification(notificationId);

            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            string exceptionMessage = exception?.Message;
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                message += " ('" + exceptionMessage + "')";
            }

            var exceptionData = exception != null && persistExceptionStack
                ? ExceptionData.Create(exception)
                : null;

            var physicalLocation = uri != null
                ? new PhysicalLocation { Uri = uri }
                : null;

            var notification = new Notification
            {
                AnalysisTarget = physicalLocation,
                Id = notificationId,
                RuleId = ruleId,
                Level = level,
                Message = message,
                Exception = exceptionData
            };

            return notification;
        }

        private static string GetMessageFormatResourceForNotification(string notificationId)
        {
            string resourceName = "Notification_" + notificationId;

            return (string)typeof(SdkResources)
                            .GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Static)
                            .GetValue(obj: null, index: null);
        }
    }
}
