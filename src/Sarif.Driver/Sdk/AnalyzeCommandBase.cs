﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class AnalyzeCommandBase<TContext, TOptions> : PlugInDriverCommand<TOptions>
        where TContext : IAnalysisContext, new()
        where TOptions : AnalyzeOptionsBase
    {
        public const string DefaultPolicyName = "default";

        private TContext rootContext;

        public Exception ExecutionException { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

        public virtual FileFormat ConfigurationFormat { get { return FileFormat.Json; } }

        public override int Run(TOptions analyzeOptions)
        {
            // 0. Initialize an common logger that drives all outputs. This
            //    object drives logging for console, statistics, etc.
            using (AggregatingLogger logger = InitializeLogger(analyzeOptions))
            {
                try
                {
                    Analyze(analyzeOptions, logger);
                }
                catch (ExitApplicationException<ExitReason> ex)
                {
                    // These exceptions have already been logged
                    ExecutionException = ex;
                    return FAILURE;
                }
                catch (Exception ex)
                {
                    // These exceptions escaped our net and must be logged here                    
                    RuntimeErrors |= Errors.LogUnhandledEngineException(this.rootContext, ex);
                    ExecutionException = ex;
                    return FAILURE;
                }
                finally
                {
                    logger.AnalysisStopped(RuntimeErrors);
                }
            }

            return ((RuntimeErrors & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None) ? SUCCESS : FAILURE;
        }

        private void Analyze(TOptions analyzeOptions, AggregatingLogger logger)
        {
            // 0. Log analysis initiation
            logger.AnalysisStarted();

            // 1. Create context object to pass to skimmers. The logger
            //    and configuration objects are common to all context
            //    instances and will be passed on again for analysis.
            this.rootContext = CreateContext(analyzeOptions, logger, RuntimeErrors);

            // 2. Perform any command line argument validation beyond what
            //    the command line parser library is capable of.
            ValidateOptions(this.rootContext, analyzeOptions);

            // 3. Create our configuration property bag, which will be 
            //    shared with all rules during analysis
            //ConfigureFromOptions(this.rootContext, analyzeOptions);

            // 4. Produce a comprehensive set of analysis targets 
            HashSet<string> targets = CreateTargetsSet(analyzeOptions);

            // 5. Proactively validate that we can locate and 
            //    access all analysis targets. Helper will return
            //    a list that potentially filters out files which
            //    did not exist, could not be accessed, etc.
            targets = ValidateTargetsExist(this.rootContext, targets);

            // 6. Initialize report file, if configured.
            InitializeOutputFile(analyzeOptions, this.rootContext, targets);

            // 7. Instantiate skimmers.
            HashSet<ISkimmer<TContext>> skimmers = CreateSkimmers(this.rootContext);

            // 8. Initialize configuration. This step must be done after initializing
            //    the skimmers, as rules define their specific context objects and
            //    so those assemblies must be loaded.
            InitializeConfiguration(analyzeOptions, this.rootContext);

            // 9. Initialize skimmers. Initialize occurs a single time only. This
            //    step needs to occurs after initializing configuration in order
            //    to allow command-line override of rule settings
            skimmers = InitializeSkimmers(skimmers, this.rootContext);

            // 10. Run all analysis
            AnalyzeTargets(analyzeOptions, skimmers, this.rootContext, targets);

            // 11. For test purposes, raise an unhandled exception if indicated
            if (RaiseUnhandledExceptionInDriverCode)
            {
                throw new InvalidOperationException(this.GetType().Name);
            }
        }

        protected virtual void ValidateOptions(TContext context, TOptions analyzeOptions)
        {
            bool succeeded = true;

            succeeded &= ValidateFile(context, analyzeOptions.OutputFilePath, shouldExist: null);
            succeeded &= ValidateFile(context, analyzeOptions.ConfigurationFilePath, shouldExist: true);
            succeeded &= ValidateFiles(context, analyzeOptions.PluginFilePaths, shouldExist: true);
            succeeded &= ValidateInvocationPropertiesToLog(context, analyzeOptions.InvocationPropertiesToLog);

            if (!succeeded)
            {
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }
        }

        private bool ValidateFiles(TContext context, IEnumerable<string> filePaths, bool shouldExist)
        {
            if (filePaths == null) { return true; }

            bool succeeded = true;

            foreach (string filePath in filePaths)
            {
                succeeded &= ValidateFile(context, filePath, shouldExist);
            }

            return succeeded;
        }

        private bool ValidateFile(TContext context, string filePath, bool? shouldExist)
        {
            if (filePath == null || filePath == DefaultPolicyName) { return true; }

            Exception exception = null;

            try
            {
                bool fileExists = File.Exists(filePath);

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

        private static bool ValidateInvocationPropertiesToLog(TContext context, IEnumerable<string> propertiesToLog)
        {
            bool succeeded = true;

            if (propertiesToLog != null)
            {
                List<string> validPropertyNames = typeof(Invocation).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(propInfo => propInfo.Name.ToUpperInvariant())
                    .ToList();

                foreach (string propertyName in propertiesToLog)
                {
                    if (!validPropertyNames.Contains(propertyName.ToUpperInvariant()))
                    {
                        Errors.LogInvalidInvocationPropertyName(context, propertyName);
                        succeeded = false;
                    }
                }
            }

            return succeeded;
        }

        internal AggregatingLogger InitializeLogger(AnalyzeOptionsBase analyzeOptions)
        {
            var logger = new AggregatingLogger();

            if (!analyzeOptions.Quiet)
            {
                logger.Loggers.Add(new ConsoleLogger(analyzeOptions.Verbose));
            }

            if (analyzeOptions.Statistics)
            {
                logger.Loggers.Add(new StatisticsLogger());
            }

            return logger;
        }

        private static HashSet<string> CreateTargetsSet(TOptions analyzeOptions)
        {
            HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string specifier in analyzeOptions.TargetFileSpecifiers)
            {
                string normalizedSpecifier = specifier;

                Uri uri;
                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (uri.IsAbsoluteUri && (uri.IsFile || uri.IsUnc))
                    {
                        normalizedSpecifier = uri.LocalPath;
                    }
                }

                // Currently, we do not filter on any extensions.
                var fileSpecifier = new FileSpecifier(normalizedSpecifier, recurse: analyzeOptions.Recurse);
                foreach (string file in fileSpecifier.Files) { targets.Add(file); }
            }

            return targets;
        }

        private HashSet<string> ValidateTargetsExist(TContext context, HashSet<string> targets)
        {
            if (targets.Count == 0)
            {
                Errors.LogNoValidAnalysisTargets(context);
                ThrowExitApplicationException(context, ExitReason.NoValidAnalysisTargets);
            }

            return targets;
        }

        protected virtual TContext CreateContext(
            TOptions options,
            IAnalysisLogger logger,
            RuntimeConditions runtimeErrors,
            string filePath = null)
        {
            var context = new TContext();
            context.Logger = logger;
            context.RuntimeErrors = runtimeErrors;

            if (filePath != null)
            {
                context.TargetUri = new Uri(filePath);
            }



            return context;
        }

        protected virtual void InitializeConfiguration(TOptions options, TContext context)
        {
            context.Policy = new PropertiesDictionary();

            if (String.IsNullOrEmpty(options.ConfigurationFilePath) ||
                options.ConfigurationFilePath == DefaultPolicyName)
            {
                return;
            }

            string extension = Path.GetExtension(options.ConfigurationFilePath);

            if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                context.Policy.LoadFromXml(options.ConfigurationFilePath);
            }
            else if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                context.Policy.LoadFromJson(options.ConfigurationFilePath);
            }
            else if (ConfigurationFormat == FileFormat.Xml)
            {
                context.Policy.LoadFromXml(options.ConfigurationFilePath);
            }
            else
            {
                context.Policy.LoadFromJson(options.ConfigurationFilePath);
            }
        }

        private void InitializeOutputFile(TOptions analyzeOptions, TContext context, HashSet<string> targets)
        {
            string filePath = analyzeOptions.OutputFilePath;
            AggregatingLogger aggregatingLogger = (AggregatingLogger)context.Logger;

            if (!string.IsNullOrEmpty(filePath))
            {
                InvokeCatchingRelevantIOExceptions
                (
                    () => aggregatingLogger.Loggers.Add(
                            new SarifLogger(
                                analyzeOptions.OutputFilePath,
                                targets,
                                analyzeOptions.Verbose,
                                analyzeOptions.LogEnvironment,
                                analyzeOptions.ComputeTargetsHash,
                                Prerelease,
                                invocationTokensToRedact: GenerateSensitiveTokensList(),
                                invocationPropertiesToLog: analyzeOptions.InvocationPropertiesToLog)),
                    (ex) =>
                    {
                        Errors.LogExceptionCreatingLogFile(context, filePath, ex);
                        ThrowExitApplicationException(context, ExitReason.ExceptionCreatingLogFile, ex);
                    }
                );
            }
        }

        private IEnumerable<string> GenerateSensitiveTokensList()
        {
            var result = new List<String>();

            result.Add(Environment.MachineName);
            result.Add(Environment.UserName);
            result.Add(Environment.UserDomainName);

            string userDnsDomain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
            string logonServer = Environment.GetEnvironmentVariable("LOGONSERVER");

            if (!string.IsNullOrEmpty(userDnsDomain)) { result.Add(userDnsDomain); }
            if (!string.IsNullOrEmpty(logonServer)) { result.Add(logonServer); }

            return result;
        }

        public void InvokeCatchingRelevantIOExceptions(Action action, Action<Exception> exceptionHandler)
        {
            try
            {
                action();
            }
            catch (UnauthorizedAccessException ex)
            {
                exceptionHandler(ex);
            }
            catch (IOException ex)
            {
                exceptionHandler(ex);
            }
        }

        private HashSet<ISkimmer<TContext>> CreateSkimmers(TContext context)
        {
            IEnumerable<ISkimmer<TContext>> skimmers;
            HashSet<ISkimmer<TContext>> result = new HashSet<ISkimmer<TContext>>();

            try
            {
                skimmers = DriverUtilities.GetExports<ISkimmer<TContext>>(DefaultPlugInAssemblies);

                foreach (ISkimmer<TContext> skimmer in skimmers)
                {
                    result.Add(skimmer);
                }
            }
            catch (Exception ex)
            {
                Errors.LogExceptionInstantiatingSkimmers(context, DefaultPlugInAssemblies, ex);
                ThrowExitApplicationException(context, ExitReason.UnhandledExceptionInstantiatingSkimmers, ex);
            }

            if (result.Count == 0)
            {
                Errors.LogNoRulesLoaded(context);
                ThrowExitApplicationException(context, ExitReason.NoRulesLoaded);
            }
            return result;
        }

        protected virtual void AnalyzeTargets(
            TOptions options,
            IEnumerable<ISkimmer<TContext>> skimmers,
            TContext rootContext,
            IEnumerable<string> targets)
        {
            HashSet<string> disabledSkimmers = new HashSet<string>();

            foreach (string target in targets)
            {
                using (TContext context = DetermineApplicabilityAndAnalyze(options, skimmers, rootContext, target, disabledSkimmers))
                {
                    RuntimeErrors |= context.RuntimeErrors;
                }
            }
        }

        protected virtual TContext DetermineApplicabilityAndAnalyze(
            TOptions options,
            IEnumerable<ISkimmer<TContext>> skimmers,
            TContext rootContext,
            string target,
            HashSet<string> disabledSkimmers)
        {
            var context = CreateContext(options, rootContext.Logger, rootContext.RuntimeErrors, target);
            context.Policy = rootContext.Policy;

            if (context.TargetLoadException != null)
            {
                Errors.LogExceptionLoadingTarget(context);
                context.Dispose();
                return context;
            }
            else if (!context.IsValidAnalysisTarget)
            {
                Warnings.LogExceptionInvalidTarget(context);
                context.Dispose();
                return context;
            }

            // Analyzing '{0}'...
            context.Logger.AnalyzingTarget(context);

            IEnumerable<ISkimmer<TContext>> applicableSkimmers = DetermineApplicabilityForTarget(skimmers, context, disabledSkimmers);

            AnalyzeTarget(applicableSkimmers, context, disabledSkimmers);

            return context;
        }

        protected virtual void AnalyzeTarget(IEnumerable<ISkimmer<TContext>> skimmers, TContext context, HashSet<string> disabledSkimmers)
        {
            foreach (ISkimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Contains(skimmer.Id)) { continue; }

                context.Rule = skimmer;

                try
                {
                    skimmer.Analyze(context);
                }
                catch (Exception ex)
                {
                    RuntimeErrors |= Errors.LogUnhandledRuleExceptionAnalyzingTarget(disabledSkimmers, context, ex);
                }
            }
        }

        protected virtual IEnumerable<ISkimmer<TContext>> DetermineApplicabilityForTarget(
            IEnumerable<ISkimmer<TContext>> skimmers,
            TContext context,
            HashSet<string> disabledSkimmers)
        {
            var candidateSkimmers = new List<ISkimmer<TContext>>();

            foreach (ISkimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Contains(skimmer.Id)) { continue; }

                string reasonForNotAnalyzing = null;
                context.Rule = skimmer;

                AnalysisApplicability applicability = AnalysisApplicability.Unknown;

                try
                {
                    applicability = skimmer.CanAnalyze(context, out reasonForNotAnalyzing);
                }
                catch (Exception ex)
                {
                    Errors.LogUnhandledRuleExceptionAssessingTargetApplicability(disabledSkimmers, context, ex);
                    continue;
                }
                finally
                {
                    RuntimeErrors |= context.RuntimeErrors;
                }

                switch (applicability)
                {
                    case AnalysisApplicability.NotApplicableToSpecifiedTarget:
                        {
                            Notes.LogNotApplicableToSpecifiedTarget(context, reasonForNotAnalyzing);
                            break;
                        }

                    case AnalysisApplicability.ApplicableToSpecifiedTarget:
                        {
                            candidateSkimmers.Add(skimmer);
                            break;
                        }
                }
            }
            return candidateSkimmers;
        }

        protected void ThrowExitApplicationException(TContext context, ExitReason exitReason, Exception innerException = null)
        {
            RuntimeErrors |= context.RuntimeErrors;

            throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, innerException)
            {
                ExitReason = exitReason
            };
        }

        protected virtual HashSet<ISkimmer<TContext>> InitializeSkimmers(HashSet<ISkimmer<TContext>> skimmers, TContext context)
        {
            HashSet<ISkimmer<TContext>> disabledSkimmers = new HashSet<ISkimmer<TContext>>();

            // ONE-TIME initialization of skimmers. Do not call 
            // Initialize more than once per skimmer instantiation
            foreach (ISkimmer<TContext> skimmer in skimmers)
            {
                try
                {
                    context.Rule = skimmer;
                    skimmer.Initialize(context);
                }
                catch (Exception ex)
                {
                    RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
                    Errors.LogUnhandledExceptionInitializingRule(context, ex);
                    disabledSkimmers.Add(skimmer);
                }
            }

            foreach (ISkimmer<TContext> disabledSkimmer in disabledSkimmers)
            {
                skimmers.Remove(disabledSkimmer);
            }

            return skimmers;
        }

        protected static void LogToolNotification(
            IAnalysisLogger logger,
            string message,
            NotificationLevel level = NotificationLevel.Note,
            Exception ex = null)
        {
            ExceptionData exceptionData = null;
            if (ex != null)
            {
                exceptionData = new ExceptionData
                {
                    Kind = ex.GetType().FullName,
                    Message = ex.Message,
                    Stack = Stack.CreateStacks(ex).FirstOrDefault()
                };
            }

            TextWriter writer = level == NotificationLevel.Error ? Console.Error : Console.Out;
            writer.WriteLine(message);

            logger.LogToolNotification(new Notification
            {
                Level = level,
                Message = message,
                Exception = exceptionData
            });
        }
    }
}