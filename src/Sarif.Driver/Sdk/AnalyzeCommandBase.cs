// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public abstract class AnalyzeCommandBase<TContext, TOptions> : PlugInDriverCommand<TOptions>
        where TContext : IAnalysisContext, new()
        where TOptions : IAnalyzeOptions
    {
        private TContext _rootContext;

        public Exception ExecutionException { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

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
                    RuntimeErrors |= Errors.LogUnhandledEngineException(_rootContext, ex);
                    ExecutionException = ex;
                    return FAILURE;
                }
                finally
                {
                    logger.AnalysisStopped(RuntimeErrors);
                }
            }

            return ((RuntimeErrors & RuntimeConditions.Fatal) == RuntimeConditions.NoErrors) ? SUCCESS : FAILURE;
        }

        private void Analyze(TOptions analyzeOptions, AggregatingLogger logger)
        {
            // 0. Log analysis initiation
            logger.AnalysisStarted();

            // 1. Create our configuration property bag, which will be 
            //    shared with all rules during analysis
            PropertyBag policy = CreateConfigurationFromOptions(analyzeOptions);

            // 2. Create context object to pass to skimmers. The logger
            //    and configuration objects are common to all context
            //    instances and will be passed on again for analysis.
            _rootContext = CreateContext(analyzeOptions, logger, policy, RuntimeErrors);

            // 3. Produce a comprehensive set of analysis targets 
            HashSet<string> targets = CreateTargetsSet(analyzeOptions);

            // 4. Proactively validate that we can locate and 
            //    access all analysis targets. Helper will return
            //    a list that potentially filters out files which
            //    did not exist, could not be accessed, etc.
            targets = ValidateTargetsExist(_rootContext, targets);

            // 5. Initialize report file, if configured.
            InitializeOutputFile(analyzeOptions, _rootContext, targets);

            // 6. Instantiate skimmers.
            HashSet<ISkimmer<TContext>> skimmers = CreateSkimmers(_rootContext);

            // 7. Initialize skimmers. Initialize occurs a single time only.
            skimmers = InitializeSkimmers(skimmers, _rootContext);

            // 8. Run all analysis
            AnalyzeTargets(analyzeOptions, skimmers, _rootContext, targets);

            // 9. For test purposes, raise an unhandled exception if indicated
            if (RaiseUnhandledExceptionInDriverCode)
            {
                throw new InvalidOperationException(this.GetType().Name);
            }
        }

        internal AggregatingLogger InitializeLogger(IAnalyzeOptions analyzeOptions)
        {
            var logger = new AggregatingLogger();
            logger.Loggers.Add(new ConsoleLogger(analyzeOptions.Verbose));

            if (analyzeOptions.Statistics)
            {
                logger.Loggers.Add(new StatisticsLogger());
            }

            return logger;
        }


        protected virtual void InitializeFromOptions(TOptions analyzeOptions)
        {
        }

        private static HashSet<string> CreateTargetsSet(TOptions analyzeOptions)
        {
            HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string specifier in analyzeOptions.TargetFileSpecifiers)
            {
                // Currently, we do not filter on any extensions.
                var fileSpecifier = new FileSpecifier(specifier, recurse: analyzeOptions.Recurse, filter: "*");
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
            PropertyBag policy,
            RuntimeConditions runtimeErrors,
            string filePath = null)
        {
            var context = new TContext();
            context.Logger = logger;
            context.Policy = policy;
            context.RuntimeErrors = runtimeErrors;

            if (filePath != null)
            {
                context.TargetUri = new Uri(filePath);
            }

            return context;
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
                                analyzeOptions.Verbose,
                                targets,
                                analyzeOptions.ComputeTargetsHash,
                                Prerelease)),
                    (ex) =>
                    {
                        Errors.LogExceptionCreatingLogFile(context, filePath, ex);
                        ThrowExitApplicationException(context, ExitReason.ExceptionCreatingLogFile, ex);
                    }
                );
            }
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
            var context = CreateContext(options, rootContext.Logger, rootContext.Policy, rootContext.RuntimeErrors, target);

            if (context.TargetLoadException != null)
            {
                Errors.LogExceptionLoadingTarget(context);
                context.Dispose();
                return context;
            }
            else if (!context.IsValidAnalysisTarget)
            {
                Notes.LogExceptionInvalidTarget(context);
                context.Dispose();
                return context;
            }

            context.Logger.AnalyzingTarget(context);

            // Analyzing '{0}'...

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
                    RuntimeErrors |= LogUnhandledRuleExceptionAnalyzingTarget(disabledSkimmers, context, skimmer, ex);
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

                    case AnalysisApplicability.NotApplicableDueToMissingConfiguration:
                        {
                            Errors.LogMissingRuleConfiguration(context, reasonForNotAnalyzing);
                            disabledSkimmers.Add(skimmer.Id);
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

        private void ThrowExitApplicationException(TContext context, ExitReason exitReason, Exception innerException = null)
        {
            RuntimeErrors |= context.RuntimeErrors;

            throw new ExitApplicationException<ExitReason>(SdkResources.MSG_UnexpectedApplicationExit, innerException)
            {
                ExitReason = exitReason
            };
        }

        protected void LogUnhandledExceptionInitializingRule(TContext context, ISkimmer<TContext> skimmer, Exception ex)
        {
            string ruleName = context.Rule.Name;
            // An unhandled exception was encountered initializing check '{0}', which 
            // has been disabled for the remainder of the analysis. Exception information:
            // {1}

            var errorContext = new TContext();
            errorContext.Rule = Errors.RuleDisabled;

            context.Logger.Log(errorContext.Rule,
                RuleUtilities.BuildResult(ResultKind.InternalError, errorContext, null,
                    nameof(SdkResources.ERR0998_ExceptionInInitialize),
                    ruleName,
                    ex.ToString()));

            RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
        }

        public static RuntimeConditions LogUnhandledRuleExceptionAnalyzingTarget(
            HashSet<string> disabledSkimmers,
            TContext context,
            ISkimmer<TContext> skimmer,
            Exception ex)
        {
            string ruleName = context.Rule.Name;
            context.Rule = Errors.RuleDisabled;

            // An unhandled exception was encountered analyzing '{0}' for check '{1}', 
            // which has been disabled for the remainder of the analysis.The 
            // exception may have resulted from a problem related to parsing 
            // image metadata and not specific to the rule, however.
            // Exception information:
            // {2}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.InternalError, context, null,
                    nameof(SdkResources.ERR0998_ExceptionInAnalyze),
                    ruleName,
                    ex.ToString()));

            if (disabledSkimmers != null) { disabledSkimmers.Add(skimmer.Id); }

            return RuntimeConditions.ExceptionInSkimmerAnalyze;
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
                    LogUnhandledExceptionInitializingRule(context, skimmer, ex);
                    disabledSkimmers.Add(skimmer);
                }
            }

            foreach (ISkimmer<TContext> disabledSkimmer in disabledSkimmers)
            {
                skimmers.Remove(disabledSkimmer);
            }

            return skimmers;
        }


        private static PropertyBag CreateConfigurationFromOptions(TOptions analyzeOptions)
        {
            PropertyBag configuration = null;
            string configurationFilePath = analyzeOptions.ConfigurationFilePath;

            if (!string.IsNullOrEmpty(configurationFilePath))
            {
                configuration = new PropertyBag();
                if (!configurationFilePath.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    configuration.LoadFrom(configurationFilePath);
                }
            }

            return configuration;
        }
    }
}