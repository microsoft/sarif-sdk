// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class Errors
    {
        private const string ERR0997 = "ERR0997";
        private const string ERR0998 = "ERR0998";
        private const string ERR0999 = "ERR0998";
        private const string ERR1001 = "ERR1001";

        public static IRule InvalidConfiguration = new Rule()
        {
            Id = ERR0997,
            Name = nameof(InvalidConfiguration),
            FullDescription = SdkResources.ERR0997_InvalidConfiguration_Description,
            MessageFormats = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR0997_ExceptionAccessingFile),
                    nameof(SdkResources.ERR0997_ExceptionLoadingPdb),
                    nameof(SdkResources.ERR0997_ExceptionLoadingPlugIn),
                    nameof(SdkResources.ERR0997_ExceptionCreatingLogFile),
                    nameof(SdkResources.ERR0997_ExceptionLoadingAnalysisTarget),
                    nameof(SdkResources.ERR0997_ExceptionInstantiatingSkimmers),
                    nameof(SdkResources.ERR0997_MissingRuleConfiguration),
                    nameof(SdkResources.ERR0997_MissingFile),
                    nameof(SdkResources.ERR0997_NoRulesLoaded),
                    nameof(SdkResources.ERR0997_NoValidAnalysisTargets)
               }, ERR0997)
        };

        public static IRule RuleDisabled = new Rule()
        {
            Id = ERR0998,
            Name = nameof(RuleDisabled),
            FullDescription = SdkResources.ERR0998_RuleDisabled_Description,
            MessageFormats = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR0998_ExceptionInCanAnalyze),
                    nameof(SdkResources.ERR0998_ExceptionInInitialize),
                    nameof(SdkResources.ERR0998_ExceptionInAnalyze)
                }, ERR0998)
        };

        public static IRule AnalysisHalted = new Rule()
        {
            Id = ERR0999,
            Name = nameof(AnalysisHalted),
            FullDescription = SdkResources.ERR0999_AnalysisHalted_Description,
            MessageFormats = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR0999_UnhandledEngineException)
                }, ERR0999)
        };

        public static IRule ParseError = new Rule()
        {
            Id = ERR1001,
            Name = nameof(ParseError),
            FullDescription = SdkResources.ERR1001_ParseError_Description,
            MessageFormats = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR1001_Default)
                }, ERR1001)
        };

        public static void LogExceptionLoadingTarget(IAnalysisContext context)
        {
            context.Rule = Errors.InvalidConfiguration;

            // An exception was raised attempting to load analysis target '{0}'. Exception information:
            // {1}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_ExceptionLoadingAnalysisTarget),
                    context.TargetLoadException.ToString()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingTargetFile;
        }

        public static void LogExceptionLoadingPdb(IAnalysisContext context, string exceptionMessage)
        {
            string ruleName = context.Rule.Name;
            context.Rule = Errors.InvalidConfiguration;

            // '{0}' was not evaluated for check '{1}' as an exception occurred loading its pdb: '{2}'
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_ExceptionLoadingPdb),
                    ruleName,
                   exceptionMessage));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingPdb;
        }

        public static void LogExceptionInstantiatingSkimmers(
            IAnalysisContext context,
            IEnumerable<Assembly> skimmerAssemblies,
            Exception ex)
        {
            context.Rule = Errors.InvalidConfiguration;

            var sb = new StringBuilder();
            foreach (Assembly assembly in skimmerAssemblies)
            {
                sb.Append(Path.GetFileName(assembly.Location) + (sb.Length > 0 ? "," : ""));
            }

            // An exception was raised attempting to instantiate skimmers from
            // the following locations: '{0}'. Exception information:
            // {1}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_ExceptionInstantiatingSkimmers),
                    sb.ToString(),
                    ex.ToString()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionInstantiatingSkimmers;
        }

        public static void LogNoRulesLoaded(IAnalysisContext context)
        {
            context.Rule = Errors.InvalidConfiguration;

            // No analysis rules could be instantiated.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_NoRulesLoaded)));

            context.RuntimeErrors |= RuntimeConditions.NoRulesLoaded;
        }

        public static void LogNoValidAnalysisTargets(IAnalysisContext context)
        {
            context.Rule = Errors.InvalidConfiguration;

            // No valid analysis targets were specified.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_NoValidAnalysisTargets)));

            context.RuntimeErrors |= RuntimeConditions.NoValidAnalysisTargets;
        }

        public static void LogExceptionCreatingLogFile(IAnalysisContext context, string fileName, Exception ex)
        {
            context.Rule = Errors.InvalidConfiguration;

            // An exception was raised attempting to create output file: '{0}'. Exception information:
            // {1}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_ExceptionCreatingLogFile),
                    fileName,
                    ex.ToString()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionCreatingLogfile;
        }

        public static void LogMissingFile(IAnalysisContext context, string fileName)
        {
            context.Rule = Errors.InvalidConfiguration;

            // A required file specified on the command line could not be found:'{0}'. 
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_MissingFile),
                    fileName));

            context.RuntimeErrors |= RuntimeConditions.MissingFile;
        }

        public static void LogExceptionAccessingFile(IAnalysisContext context, string fileName, Exception ex)
        {
            context.Rule = Errors.InvalidConfiguration;

            // An exception was raised accessing a file specified on the command-line: '{0}'. Exception information:
            // {1}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_ExceptionAccessingFile),
                    fileName,
                    ex.ToString()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionAccessingFile;
        }


        public static void LogMissingRuleConfiguration(IAnalysisContext context, string reasonForNotAnalyzing)
        {            
            string ruleName = context.Rule.Name;
            context.Rule = Errors.InvalidConfiguration;

            Assembly assembly = Assembly.GetEntryAssembly();
            assembly = assembly ?? Assembly.GetExecutingAssembly();
            string exeName = Path.GetFileName(assembly.Location);

            // Check '{1}' was disabled while analyzing '{0}' because the analysis
            // was not configured with required policy ({1}). To resolve this,
            // configure and provide a policy file on the {2} command-line using
            // the --policy argument (recommended), or pass '--config default'
            // to invoke built-in settings. Invoke the {2} 'exportConfig' command
            // to produce an initial configuration file that can be edited, if
            // necessary, and passed back into the tool.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_MissingRuleConfiguration),
                    ruleName,
                    reasonForNotAnalyzing,
                    exeName));

            context.RuntimeErrors |= RuntimeConditions.RuleMissingRequiredConfiguration;
        }

        public static void LogExceptionLoadingPlugIn(string plugInFilePath, IAnalysisContext context, Exception ex)
        {
            context.Rule = Errors.InvalidConfiguration;
            context.TargetUri = new Uri(plugInFilePath);

            // An exception was raised attempting to load plug-in '{0}'. Exception information:
            // {1}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.ConfigurationError, context, null,
                    nameof(SdkResources.ERR0997_ExceptionLoadingPlugIn),
                    ex.ToString()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionLoadingAnalysisPlugIn;
        }

        public static void LogUnhandledRuleExceptionAssessingTargetApplicability(
            HashSet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception ex)
        {
            string ruleId = context.Rule.Id;
            string ruleName = context.Rule.Name;
            context.Rule = Errors.RuleDisabled;

            // An unhandled exception was raised attempting to determine whether '{0}' 
            // is a valid analysis target for check '{1}' (which has been disabled 
            // for the remainder of the analysis). The exception may have resulted 
            // from a problem related to parsing image metadata and not specific to 
            // the rule, however. Exception information:
            // {2}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.InternalError, context, null,
                    nameof(SdkResources.ERR0998_ExceptionInCanAnalyze),
                    ruleName,
                    ex.ToString()));

            if (disabledSkimmers != null) { disabledSkimmers.Add(ruleId); }

            context.RuntimeErrors |= RuntimeConditions.ExceptionRaisedInSkimmerCanAnalyze;
        }


        public static void LogUnhandledExceptionInitializingRule(IAnalysisContext context, Exception ex)
        {
            string ruleId = context.Rule.Id;
            string ruleName = context.Rule.Name;
            // An unhandled exception was encountered initializing check '{0}:', which 
            // has been disabled for the remainder of the analysis. Exception information:
            // {1}

            var errorContext = new AnalysisContext();
            errorContext.Rule = Errors.RuleDisabled;

            context.Logger.Log(errorContext.Rule,
                RuleUtilities.BuildResult(ResultKind.InternalError, errorContext, null,
                    nameof(SdkResources.ERR0998_ExceptionInInitialize),
                    ruleName,
                    ex.ToString()));

            context.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
        }

        public static RuntimeConditions LogUnhandledRuleExceptionAnalyzingTarget(
            HashSet<string> disabledSkimmers,
            IAnalysisContext context,
            Exception ex)
        {
            string ruleId = context.Rule.Id;
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

            if (disabledSkimmers != null) { disabledSkimmers.Add(ruleId); }

            return RuntimeConditions.ExceptionInSkimmerAnalyze;
        }

        public static RuntimeConditions LogUnhandledEngineException(IAnalysisContext context, Exception ex)
        {
            context.Rule = Errors.AnalysisHalted;

            // An unhandled exception was raised during analysis: {0}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.InternalError, context, null,
                    nameof(SdkResources.ERR0999_UnhandledEngineException),
                    ex.ToString()));

            return RuntimeConditions.ExceptionInEngine;
        }

        public static void LogTargetParseError(IAnalysisContext context, Region region, string message)
        {
            context.Rule = Errors.ParseError;

            // An error occurred parsing '{0}': {1}
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.Error, context, region,
                    nameof(SdkResources.ERR1001_Default),
                    message));

            context.RuntimeErrors |= RuntimeConditions.TargetParseError;
        }
    }
}
