// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// Helper class for working with EnvDTE projects
    /// </summary>
    internal static class ProjectHelper
    {
        private const uint VSITEMID_ROOT = unchecked((uint)-2);
        private const int VSHPROPID_ExtObject = -2027;
        private static readonly Guid WEB_PROJECT_GUID = new Guid("{E24C65DC-7377-472b-9ABA-BC803B73C61A}");
        private static readonly Guid VC_PROJECT_GUID = new Guid("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");
        private static readonly Guid VB_PROJECT_GUID = new Guid("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");
        private static readonly Guid CSHARP_PROJECT_GUID = new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        private static readonly Guid SOLUTIONFOLDER_PROJECT_GUID = new Guid("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}");

        private static readonly Guid MISCFILES_PROJECT_GUID = new Guid(EnvDTE.Constants.vsProjectKindMisc);

        private static readonly Guid VC_PROJECTITEM_GUID = new Guid(CodeModelLanguageConstants.vsCMLanguageVC);
        private static readonly Guid VB_PROJECTITEM_GUID = new Guid(CodeModelLanguageConstants.vsCMLanguageVB);
        private static readonly Guid CSHARP_PROJECTITEM_GUID = new Guid(CodeModelLanguageConstants.vsCMLanguageCSharp);

        internal static bool IsVCProject(Project project)
        {
            return IsProjectKind(project, VC_PROJECT_GUID);
        }

        internal static bool IsVBProject(Project project)
        {
            return IsProjectKind(project, VB_PROJECT_GUID);
        }

        internal static bool IsCSharpProject(Project project)
        {
            return IsProjectKind(project, CSHARP_PROJECT_GUID);
        }

        internal static bool IsWebProject(Project project)
        {
            return IsProjectKind(project, WEB_PROJECT_GUID);
        }

        internal static bool IsSolutionFolderProject(Project project)
        {
            return IsProjectKind(project, SOLUTIONFOLDER_PROJECT_GUID);
        }

        internal static bool IsMiscellaneousFilesProject(Project project)
        {
            return IsProjectKind(project, MISCFILES_PROJECT_GUID);
        }

        internal static bool IsVCProjectWithCLRSupport(Project project)
        {
            bool result = false;
            if (IsVCProject(project))
            {
                ConfigurationManager cfgManager = project.ConfigurationManager;
                if (cfgManager != null)
                {
                    Configuration activeConfig = cfgManager.ActiveConfiguration;
                    if (activeConfig != null && activeConfig.Properties != null)
                    {
                        Properties properties = activeConfig.Properties;

                        Property property = properties.Item("CLRSupport");
                        if (property != null && property.Value != null)
                        {
                            // If we can't parse value of this property than we will return by default 'false'
                            bool.TryParse(property.Value.ToString(), out result);
                        }
                    }
                }
            }
            return result;
        }

        internal static bool IsProjectKind(Project project, Guid projectKindGuid)
        {
            return projectKindGuid == new Guid(project.Kind);
        }

        internal static bool IsVCProjectItem(ProjectItem projectItem)
        {    
            return GetProjectItemLanguage(projectItem) == VC_PROJECTITEM_GUID;
        }

        internal static bool IsVBProjectItem(ProjectItem projectItem)
        {
            return GetProjectItemLanguage(projectItem) == VB_PROJECTITEM_GUID;
        }

        internal static bool IsCSharpProjectItem(ProjectItem projectItem)
        {
            return GetProjectItemLanguage(projectItem) == CSHARP_PROJECTITEM_GUID;
        }

        internal static bool IsSupportedForAspProjectItem(EnvDTE.ProjectItem projectItem)
        {
            Guid langGuid = GetProjectItemLanguage(projectItem);
            bool supportedForAsp = (langGuid == CSHARP_PROJECTITEM_GUID || langGuid == VB_PROJECTITEM_GUID || langGuid == Guid.Empty);
            return supportedForAsp;
        }

        internal static Guid GetProjectItemLanguage(ProjectItem projectItem)
        {
            FileCodeModel fileCodeModel = projectItem.FileCodeModel;
            if (fileCodeModel == null)
                return Guid.Empty;

            return new Guid(fileCodeModel.Language);
        }

        internal static ProjectItem FindProjectItem(ProjectItems projectItems, string lookupName)
        {
            int count = projectItems.Count;

            for (int i = 1; i <= count; i++)
            {
                ProjectItem projectItem = projectItems.Item(i);

                if (string.Equals(lookupName, projectItem.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return projectItem;
                }
            }

            return null;
        }

        internal static string GetProjectFullPath(EnvDTE.Project project)
        {
            string projectFullPath = null;

            if (project != null)
            {
                try
                {
                    // When accessing Project DTE, it might always blow up, for example when project failed to load, 
                    // it is represented by an object which will throw NotImplemented on many DTE calls, etc
                    projectFullPath = project.FullName;
                }
                catch (Exception e)
                {
                    if (e is ArgumentException || e is COMException)
                    {
                        // Sometimes if the project failed to load we might get E_INVALIDARG here from DTE
                        Debug.Fail("Error while trying to obtain project full path. \n" + e.ToString());
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return projectFullPath;
        }
    }
}
