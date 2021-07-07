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

        protected virtual bool ProcessBaseline(T driverOptions, IFileSystem fileSystem)
        {
            if (!(driverOptions is AnalyzeOptionsBase options))
            {
                return false;
            }

            if (string.IsNullOrEmpty(options.BaselineSarifFile) || string.IsNullOrEmpty(options.OutputFilePath))
            {
                return false;
            }


            var serializer = new JsonSerializer();
            serializer.Formatting = options.PrettyPrint || (!options.PrettyPrint && !options.Minify) ?
                                    Formatting.Indented :
                                    Formatting.None;

            SarifLog baselineFile;
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(fileSystem.FileOpenRead(options.BaselineSarifFile))))
            {
                baselineFile = serializer.Deserialize<SarifLog>(reader);
            }

            SarifLog currentSarifLog;
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(fileSystem.FileOpenRead(options.OutputFilePath))))
            {
                currentSarifLog = serializer.Deserialize<SarifLog>(reader);
            }
            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();

            SarifLog baseline = matcher.Match(new SarifLog[] { baselineFile }, new SarifLog[] { currentSarifLog }).First();

            string targetFile = options.Inline ? options.BaselineSarifFile : options.OutputFilePath;
            using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(fileSystem.FileCreate(targetFile))))
            {
                serializer.Serialize(writer, baseline);
            }

            return true;
        }
    }
}
