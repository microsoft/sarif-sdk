// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.WorkItemFiling;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class WorkItemFilingConfiguration<T> : PropertiesDictionary where T: new()
    {
        private List<IWorkItemModelTransformer<T>> workItemModelTransformers;

        public IReadOnlyList<IWorkItemModelTransformer<T>> WorkItemModelTransformers
        {
            get { return PopulateWorkItemModelTransformers(); }
        }

        public void AddWorkItemModelTransformer(IWorkItemModelTransformer<T> workItemModelTransformer)
        {
            StringSet assemblyAndTypeNames = this.GetProperty(PluginAssemblyAndTypesNames);

            string assemblyAndTypeName = workItemModelTransformer.GetType().AssemblyQualifiedName;

            if (!assemblyAndTypeNames.Contains(assemblyAndTypeName))
            {
                assemblyAndTypeNames.Add(assemblyAndTypeName);
                this.workItemModelTransformers = this.workItemModelTransformers ?? new List<IWorkItemModelTransformer<T>>();
                this.workItemModelTransformers.Add(workItemModelTransformer);
            }
        }

        private IReadOnlyList<IWorkItemModelTransformer<T>> PopulateWorkItemModelTransformers()
        {
            if (this.workItemModelTransformers == null) 
            {
                this.workItemModelTransformers = new List<IWorkItemModelTransformer<T>>();

                StringSet assemblyAndTypeNames = this.GetProperty(PluginAssemblyAndTypesNames);
                foreach (string assemblyAndTypeName in assemblyAndTypeNames)
                {
                    Type type = Type.GetType(assemblyAndTypeName);

                    IWorkItemModelTransformer<T> workItemModelTransformer =
                        (IWorkItemModelTransformer<T>)Activator.CreateInstance(type);

                    workItemModelTransformers.Add(workItemModelTransformer);
                }
            }
            return this.workItemModelTransformers.AsReadOnly();
        }

        public static PerLanguageOption<StringSet> PluginAssemblyAndTypesNames { get; } =
            new PerLanguageOption<StringSet>(
                "Extensibility", nameof(PluginAssemblyAndTypesNames), defaultValue: () => { return new StringSet(); });
    }
}
