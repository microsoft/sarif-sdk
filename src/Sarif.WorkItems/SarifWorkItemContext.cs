﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemContext : PropertiesDictionary, IDisposable
    {
        public SarifWorkItemContext()
        {
            this.AssemblyLocationMap = new Dictionary<string, string>();
        }

        public SarifWorkItemContext(SarifWorkItemContext initializer) : base(initializer) { }

        public FilingClient.SourceControlProvider CurrentProvider { get; set; }

        protected Dictionary<string, string> AssemblyLocationMap { get; }

        public Uri HostUri
        {
            get { return this.GetProperty(HostUriOption); }
            set { this.SetProperty(HostUriOption, value); }
        }

        public string BugFooter
        {
            get { return this.GetProperty(BugFooterOption); }
            set { this.SetProperty(BugFooterOption, value); }
        }

        public string PersonalAccessToken
        {
            get { return this.GetProperty(PersonalAccessTokenOption); }
            set { this.SetProperty(PersonalAccessTokenOption, value); }
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

        private List<SarifWorkItemModelTransformer> workItemModelTransformers;
        public IReadOnlyList<SarifWorkItemModelTransformer> Transformers
        {
            get { return PopulateWorkItemModelTransformers(); }
        }

        public void AddWorkItemModelTransformer(SarifWorkItemModelTransformer workItemModelTransformer)
        {
            StringSet assemblies = this.GetProperty(PluginAssemblyLocations);
            StringSet assemblyQualifiedNames = this.GetProperty(PluginAssemblyQualifiedNames);

            string assemblyLocation = workItemModelTransformer.GetType().Assembly.Location;
            string assemblyQualifiedName = workItemModelTransformer.GetType().AssemblyQualifiedName;

            assemblies.Add(assemblyLocation);
            assemblyQualifiedNames.Add(assemblyQualifiedName);

            this.workItemModelTransformers = this.workItemModelTransformers ?? new List<SarifWorkItemModelTransformer>();
            this.workItemModelTransformers.Add(workItemModelTransformer);
        }

        private IReadOnlyList<SarifWorkItemModelTransformer> PopulateWorkItemModelTransformers()
        {
            if (this.workItemModelTransformers == null)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                this.workItemModelTransformers = new List<SarifWorkItemModelTransformer>();

                var loadedAssemblies = new Dictionary<string, Assembly>();
                var thisAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                foreach (string assemblyLocation in this.GetProperty(PluginAssemblyLocations))
                {
                    string assemblyPath = assemblyLocation;
                    if (!Path.IsPathRooted(assemblyLocation))
                    {
                        assemblyPath = Path.GetFullPath(Path.Combine(thisAssemblyDirectory, assemblyLocation));
                    }

                    Assembly a = Assembly.LoadFrom(assemblyPath);
                    loadedAssemblies.Add(a.FullName, a);
                    this.AssemblyLocationMap.Add(a.FullName, Path.GetDirectoryName(Path.GetFullPath(a.Location)));
                }

                StringSet assemblyAndTypeNames = this.GetProperty(PluginAssemblyQualifiedNames);
                foreach (string assemblyAndTypeName in assemblyAndTypeNames)
                {
                    Type type = Type.GetType(assemblyAndTypeName);

                    if (type == null)
                    {
                        string assemblyName = assemblyAndTypeName.Substring(assemblyAndTypeName.IndexOf(',') + 1).Trim();
                        if (loadedAssemblies.TryGetValue(assemblyName, out Assembly loadedAssembly))
                        {
                            type = loadedAssembly.GetType(assemblyAndTypeName);
                        }
                    }

                    SarifWorkItemModelTransformer workItemModelTransformer =
                        (SarifWorkItemModelTransformer)Activator.CreateInstance(type);

                    workItemModelTransformers.Add(workItemModelTransformer);
                }
            }

            return this.workItemModelTransformers.AsReadOnly();
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // TODO: Thoughtfully instrument this location and others
            //       in pipeline via current logging mechanism.
            // 
            // https://github.com/microsoft/sarif-sdk/issues/1836

            Assembly a = null;

            if (this.AssemblyLocationMap.TryGetValue(args.RequestingAssembly.FullName, out string basePath))
            {
                string assemblyFileName = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                string assemblyFullPath = Path.Combine(basePath, assemblyFileName);

                if (File.Exists(assemblyFullPath))
                {
                    try
                    {
                        a = Assembly.Load(assemblyFullPath);
                    }
                    catch (IOException) { }
                    catch (TypeLoadException) { }
                    catch (BadImageFormatException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }

            return a;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        internal static PerLanguageOption<Uri> HostUriOption { get; } =
            new PerLanguageOption<Uri>(
                "Extensibility", nameof(HostUri),
                defaultValue: () => { return null; });

        internal static PerLanguageOption<string> PersonalAccessTokenOption { get; } =
            new PerLanguageOption<string>(
                "Extensibility", nameof(PersonalAccessToken),
                defaultValue: () => { return string.Empty; });

        public static PerLanguageOption<StringSet> PluginAssemblyLocations { get; } =
            new PerLanguageOption<StringSet>(
                "Extensibility", nameof(PluginAssemblyLocations),
                defaultValue: () => { return new StringSet(); });

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

        public static PerLanguageOption<SplittingStrategy> SplittingStrategyOption { get; } =
            new PerLanguageOption<SplittingStrategy>(
                "Extensibility", nameof(SplittingStrategy),
                defaultValue: () => { return 0; });

        public static PerLanguageOption<string> BugFooterOption { get; } =
            new PerLanguageOption<string>(
                "Extensibility", nameof(BugFooter),
                defaultValue: () => { return string.Empty; });

    }
}