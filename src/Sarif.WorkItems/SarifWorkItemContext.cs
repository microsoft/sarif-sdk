// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemContext : PropertiesDictionary
    {
        public SarifWorkItemContext() { }

        public SarifWorkItemContext(SarifWorkItemContext initializer) : base(initializer) { }

        internal void InitializeFromLog(SarifLog sarifLog)
        {
            throw new NotImplementedException();
        }

        public string SecurityToken 
        {
            get { return this.GetProperty(SecurityTokenOption); }
            set { this.SetProperty(SecurityTokenOption, value); }
        }

        public SplittingStrategy SplittingStrategy
        {
            get { return this.GetProperty(SplittingStrategyOption); }
            set { this.SetProperty(SplittingStrategyOption, value); }
        }

        public OptionallyEmittedData DataToRemove
        {
            get { return this.GetProperty(DataToRemoveOption); }
            set { this.SetProperty(DataToRemoveOption, value); }
        }

        public OptionallyEmittedData DataToInsert
        {
            get { return this.GetProperty(DataToInsertOption); }
            set { this.SetProperty(DataToInsertOption, value); }
        }

        private List<SarifWorkItemModel> workItemModelTransformers;
        public IReadOnlyList<IWorkItemModelTransformer<SarifWorkItemContext>> WorkItemModelTransformers
        {
            get { return PopulateWorkItemModelTransformers(); }
        }

        public void AddWorkItemModelTransformer(IWorkItemModelTransformer<SarifWorkItemContext> workItemModelTransformer)
        {
            StringSet assemblyAndTypeNames = this.GetProperty(PluginAssemblyQualifiedNames);

            string assemblyQualifiedName = workItemModelTransformer.GetType().AssemblyQualifiedName;

            if (!assemblyAndTypeNames.Contains(assemblyQualifiedName))
            {
                assemblyAndTypeNames.Add(assemblyQualifiedName);
                this.workItemModelTransformers = this.workItemModelTransformers ?? new List<IWorkItemModelTransformer<SarifWorkItemContext>>();
                this.workItemModelTransformers.Add(workItemModelTransformer);
            }
        }

        private IReadOnlyList<IWorkItemModelTransformer<SarifWorkItemContext>> PopulateWorkItemModelTransformers()
        {
            if (this.workItemModelTransformers == null) 
            {
                this.workItemModelTransformers = new List<IWorkItemModelTransformer<SarifWorkItemContext>>();

                StringSet assemblyAndTypeNames = this.GetProperty(PluginAssemblyQualifiedNames);
                foreach (string assemblyAndTypeName in assemblyAndTypeNames)
                {
                    Type type = Type.GetType(assemblyAndTypeName);

                    IWorkItemModelTransformer<SarifWorkItemContext> workItemModelTransformer =
                        (IWorkItemModelTransformer<SarifWorkItemContext>)Activator.CreateInstance(type);

                    workItemModelTransformers.Add(workItemModelTransformer);
                }
            }
            return this.workItemModelTransformers.AsReadOnly();
        }

        internal static PerLanguageOption<string> SecurityTokenOption { get; } =
            new PerLanguageOption<string>(
                "Extensibility", nameof(SecurityToken),
                defaultValue: () => { return String.Empty; });


        public static PerLanguageOption<SplittingStrategy> SplittingStrategyOption { get; } =
            new PerLanguageOption<SplittingStrategy>(
                "Extensibility", nameof(SplittingStrategy),
                defaultValue: () => { return 0; });


        public static PerLanguageOption<StringSet> PluginAssemblyQualifiedNames { get; } =
            new PerLanguageOption<StringSet>(
                "Extensibility", nameof(PluginAssemblyQualifiedNames),
                defaultValue: () => { return new StringSet(); });

        public static PerLanguageOption<OptionallyEmittedData> DataToRemoveOption { get; } =
            new PerLanguageOption<OptionallyEmittedData>(
                "Extensibility", nameof(DataToRemove),
                defaultValue: () => { return 0; });

        public static PerLanguageOption<OptionallyEmittedData> DataToInsertOption { get; } =
            new PerLanguageOption<OptionallyEmittedData>(
                "Extensibility", nameof(DataToInsert),
                defaultValue: () => { return 0; });
    }
}
