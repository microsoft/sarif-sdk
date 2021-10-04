// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

                    WriteSarifFile(fileSystem, visitor.SarifLogVersionOne, targetFile, options.Minify, SarifContractResolverVersionOne.Instance);
                }
                else
                {
                    WriteSarifFile(fileSystem, baseline, targetFile, options.Minify);
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
    }
}
