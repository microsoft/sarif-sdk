// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.WorkItems.Configuration;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemContext : IDisposable
    {
        private string m_organizationOverride;
        private string m_projectOverride;
        private string m_workItemPersonalAccessTokenOverride;
        private SplittingStrategy? m_splittingStrategyOverride;
        private bool? m_syncWorkItemMetadataOverride;
        private bool? m_shouldFileUnchangedOverride;

        public SarifWorkItemContext(SurffConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.WorkItemProvider = FilingClient.WorkItemProvider.AzureDevOps;
            this.AssemblyLocationMap = new Dictionary<string, string>();

            this.Configuration = configuration;
        }

        public FilingClient.WorkItemProvider WorkItemProvider { get; set; }

        protected Dictionary<string, string> AssemblyLocationMap { get; }

        public SurffConfiguration Configuration { get; }

        // BUGBUG: This pattern (used many times in this file) is confusing.
        //         The person using a SarifWorkItemContext won't know when to 
        //         use values from the context vs values from the configuration.
        //         e.g. this.Organization vs this.Configuration.WorkItem.Organization
        //         Maybe the user should override the configuration before constructing
        //         the SarifWorkItemContext. Then the callers always use this.Configuration.WorkItem.Organization
        //         The downside of this approach is that we won't know if the setting came from the
        //         original configuration or was manually overridden.
        public string Organization
        {
            get
            {
                if (string.IsNullOrEmpty(this.Configuration.WorkItem.Organization))
                {
                    return m_organizationOverride;
                }
                else
                {
                    return this.Configuration.WorkItem.Organization;
                }
            }
            set
            {
                m_organizationOverride = value;
            }
        }

        public string Project
        {
            get
            {
                if (string.IsNullOrEmpty(this.Configuration.WorkItem.Project))
                {
                    return m_projectOverride;
                }
                else
                {
                    return this.Configuration.WorkItem.Project;
                }
            }
            set
            {
                m_projectOverride = value;
            }
        }

        public string PersonalAccessToken
        {
            get
            {
                if (m_workItemPersonalAccessTokenOverride != null)
                {
                    return m_workItemPersonalAccessTokenOverride;
                }
                else
                {
                    // BUGBUG: We should have a configuration setting for a keyvault to read the value from.
                    return this.Configuration.WorkItem.PersonalAccessToken;
                }
            }
            set
            {
                m_workItemPersonalAccessTokenOverride = value;
            }
        }

        public SplittingStrategy SplittingStrategy
        {
            get
            {
                if (m_splittingStrategyOverride.HasValue)
                {
                    return m_splittingStrategyOverride.Value;
                }
                else
                {
                    return (SplittingStrategy)Enum.Parse(typeof(SplittingStrategy), this.Configuration.WorkItem.SplittingStrategy);
                }
            }
            set
            {
                m_splittingStrategyOverride = value;
            }
        }

        public bool SyncWorkItemMetadata
        {
            get
            {
                if (m_syncWorkItemMetadataOverride.HasValue)
                {
                    return m_syncWorkItemMetadataOverride.Value;
                }
                else
                {
                    return this.Configuration.WorkItem.SyncWorkItemMetadata;
                }
            }
            set
            {
                (m_syncWorkItemMetadataOverride = value;
            }
        }

        public bool ShouldFileUnchanged
        {
            get
            {
                if (m_shouldFileUnchangedOverride.HasValue)
                {
                    return m_shouldFileUnchangedOverride.Value;
                }
                else
                {
                    return this.Configuration.WorkItem.ShouldFileUnchanged;
                }
            }
            set
            {
                m_shouldFileUnchangedOverride = value;
            }
        }

        public Uri HostUri
        {
            get
            {
                return new Uri($"https://dev.azure.com/{this.Organization}/{this.Project}/");
            }
        }

        public string DescriptionFooter
        {
            get
            {
                return
                    WorkItemProvider == FilingClient.WorkItemProvider.AzureDevOps
                        ? this.Configuration.WorkItem.AzureDevOpsDescriptionFooter
                        : this.Configuration.WorkItem.GitHubDescriptionFooter;
            }
        }

        public string[] PluginAssemblyLocations
        {
            get
            {
                return this.Configuration.Extensions.Select(ext => ext.AssemblyPath).ToArray();
            }
        }

        public string[] PluginAssemblyQualifiedNames
        {
            get
            {
                return this.Configuration.Extensions.Select(ext => ext.FullyQualifiedTypeName).ToArray();
            }
        }

        public IReadOnlyList<SarifWorkItemModelTransformer> Transformers
        {
            get { return PopulateWorkItemModelTransformers(); }
        }

        public string CreateLinkText(string text, string url)
        {
            if (this.WorkItemProvider == Microsoft.WorkItems.FilingClient.WorkItemProvider.AzureDevOps)
            {
                return string.Format(WorkItemsResources.HtmlLinkTemplate, text, url);
            }
            else
            {
                return string.Format(WorkItemsResources.MarkdownLinkTemplate, text, url);
            }
        }

        private List<SarifWorkItemModelTransformer> workItemModelTransformers;
        private IReadOnlyList<SarifWorkItemModelTransformer> PopulateWorkItemModelTransformers()
        {
            if (this.workItemModelTransformers == null)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                this.workItemModelTransformers = new List<SarifWorkItemModelTransformer>();

                var loadedAssemblies = new Dictionary<string, Assembly>();
                string thisAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                foreach (string assemblyLocation in this.PluginAssemblyLocations)
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

                foreach (string assemblyAndTypeName in this.PluginAssemblyQualifiedNames)
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
    }
}