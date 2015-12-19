// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    internal static class DriverUtilities
    {
        public static ImmutableArray<T> GetExports<T>(IEnumerable<Assembly> assemblies)
        {
            var container = CreateCompositionContainer<T>(assemblies);
            return container.GetExports<T>().ToImmutableArray();
        }

        private static CompositionHost CreateCompositionContainer<T>(IEnumerable<Assembly> assemblies)
        {
            ConventionBuilder conventions = GetConventions<T>();

            return new ContainerConfiguration()
                .WithAssemblies(assemblies, conventions)
                .CreateContainer();
        }

        private static ConventionBuilder GetConventions<T>()
        {
            var conventions = new ConventionBuilder();

            // New per-analyzer options mechanism 
            conventions.ForTypesDerivedFrom<T>()
                .Export<T>();

            return conventions;
        }
    }
}
