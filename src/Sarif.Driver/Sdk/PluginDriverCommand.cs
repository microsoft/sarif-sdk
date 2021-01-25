// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

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
    }
}
