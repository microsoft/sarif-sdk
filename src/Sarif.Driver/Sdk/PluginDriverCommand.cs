// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

using Newtonsoft.Json;

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

            var serializer = new JsonSerializer
            {
                Formatting = options.PrettyPrint || (!options.PrettyPrint && !options.Minify) ?
                                    Formatting.Indented :
                                    Formatting.None
            };

            SarifLog baselineFile;
            using (var reader = new JsonTextReader(new StreamReader(fileSystem.FileOpenRead(options.BaselineSarifFile))))
            {
                baselineFile = serializer.Deserialize<SarifLog>(reader);
            }

            SarifLog currentSarifLog;
            using (var reader = new JsonTextReader(new StreamReader(fileSystem.FileOpenRead(options.OutputFilePath))))
            {
                currentSarifLog = serializer.Deserialize<SarifLog>(reader);
            }

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
                using (var writer = new JsonTextWriter(new StreamWriter(fileSystem.FileCreate(targetFile))))
                {
                    serializer.Serialize(writer, baseline);
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
