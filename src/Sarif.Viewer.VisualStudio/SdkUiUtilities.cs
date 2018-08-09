// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;

namespace Microsoft.Sarif.Viewer
{

    public static class SdkUIUtilities
    {
        private static string s_staticAnalysisToolsDirectory;
        private static string[] s_ruleSetDirectories;
        private static string s_builtInRuleSetDirectory;
        private static string s_plugInsDirectory;
        private static readonly Guid s_appIdUsesIsolatedCLR = new System.Guid("{074a44d3-1d9d-406c-9f91-c2a4982b1974}");
        internal static readonly Guid s_enviornmentThemeCategory = new System.Guid("624ed9c3-bdfd-41fa-96c3-7c824ea32e3d");

        // Embedded link format: [link text](n) where n is a non-negative integer
        private const string EmbeddedLinkPattern = @"\[(?<link>[^\\\]]+)\]\((?<index>\d+)\)";

        internal const string RuleSetFileExtension = ".ruleset";
        /// <summary>
        /// Default Rule Set for Express SKU, it is used by VS and VB projects
        /// </summary>
        internal const string ManagedMinimumRulesetFileName = "ManagedMinimumRules.ruleset";
        /// <summary>
        /// Default Rule Set for Express SKU, it is used by VC projects
        /// </summary>
        internal const string NativeMinimumRulesetFileName = "NativeMinimumRules.ruleset";
        /// <summary>
        /// Default Rule Set for Express SKU, it is used by VC + CLR projects
        /// </summary>
        internal const string MixedMinimumRulesetFileName = "MixedMinimumRules.ruleset";

        /// <summary>
        /// Gets the requested service of type S from the service provider.
        /// </summary>
        /// <typeparam name="S">The service interface to retrieve</typeparam>
        /// <param name="provider">The IServiceProvider implementation</param>
        /// <returns>A reference to the service</returns>
        internal static S GetService<S>(IServiceProvider provider) where S : class
        {
            return GetService<S, S>(provider);
        }

        /// <summary>
        /// Gets the requested service of type S and cast to type T from the service provider.
        /// </summary>
        /// <typeparam name="S">The service to retrieve</typeparam>
        /// <typeparam name="T">The interface to cast to</typeparam>
        /// <param name="provider">The IServiceProvider implementation</param>
        /// <returns>A reference to the service</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static T GetService<S, T>(IServiceProvider provider) where S : class where T : class
        {
            try
            {
                return (T)provider.GetService(typeof(S));
            }
            catch (Exception)
            {
                // If anything went wrong, just ignore it
            }

            return null;
        }

        // The cached registry root
        private static string s_registryRoot;

        /// <summary>
        /// Reads the registry root from the IDE
        /// </summary>
        internal static string GetRegistryRoot(IServiceProvider provider)
        {
            if (s_registryRoot == null)
            {
                IVsShell vsh = GetService<IVsShell>(provider);
                if (vsh == null)
                    return null;

                object obj;
                if (VSConstants.S_OK == vsh.GetProperty((int)__VSSPROPID.VSSPROPID_VirtualRegistryRoot, out obj))
                {
                    s_registryRoot = obj.ToString();
                }
            }
            return s_registryRoot;
        }

        /// <summary>
        /// Returns the current VS font from the provider
        /// </summary>
        /// <param name="provider">An IServiceProvider that contains IUIHostLocale</param>
        /// <returns>The current VS font</returns>
        internal static Font GetVsFont(IServiceProvider provider)
        {
            if (provider != null)
            {
                IUIHostLocale service = (IUIHostLocale)provider.GetService(typeof(IUIHostLocale));
                UIDLGLOGFONT[] dlgFont = new UIDLGLOGFONT[1];

                if (service != null && 0 == service.GetDialogFont(dlgFont))
                {
                    try
                    {
                        return Font.FromLogFont(dlgFont[0]);
                    }
                    catch (ArgumentException)
                    {
                        // This can happen if a non-TrueType font is set as the system Icon font.
                        // Eat the exception and use the system dialog font.
                    }
                }
            }
            return System.Drawing.SystemFonts.DialogFont;
        }

        /// <summary>
        /// Reads the contents of an isolated storage file and deserializes it to an object.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized object.</typeparam>
        /// <param name="storageFileName">The isolated storage file.</param>
        /// <returns></returns>
        internal static T GetStoredObject<T>(string storageFileName) where T : class
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            if (store.FileExists(storageFileName))
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(storageFileName, FileMode.Open, store))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Serializes an object and writes it to an isolated storage file.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="t">The object to serialize.</param>
        /// <param name="storageFileName">The isolated storage file.</param>
        internal static void StoreObject<T>(T t, string storageFileName)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(storageFileName, FileMode.Create, store))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(JsonConvert.SerializeObject(t, Formatting.Indented));
                }
            }
        }

        /// <summary>
        /// Gets a property from the DTE.
        ///
        /// See http://msdn.microsoft.com/en-us/library/ms165643.aspx for
        /// a description of the parameter values.
        /// </summary>
        /// <returns>
        /// True if the property exists and was successfully cast to the requested type,
        /// otherwise false.
        /// </returns>
        internal static bool TryGetDteProperty<T>
        (
            EnvDTE.DTE dte,
            string category,
            string page,
            string propertyName,
            out T retVal
        )
        {
            retVal = default(T);

            if (dte == null)
                return false;

            EnvDTE.Properties properties;
            try
            {
                properties = dte.get_Properties(category, page);
            }
            catch (COMException)
            {
                // EnvDTE.Properties.get_Properties throws this exception when
                // the category or page does not exist.
                return false;
            }

            EnvDTE.Property property;
            try
            {
                property = properties.Item(propertyName);
            }
            catch (ArgumentException)
            {
                // EnvDTE.Property.Item throws this exception when the property
                // does not exist.
                return false;
            }

            // Convert the property to the requested type
            try
            {
                retVal = (T)property.Value;
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        private static IWin32Window s_ownerDialogWindow;
        private class WindowHandleWrapper : IWin32Window
        {
            private IntPtr m_hwnd;
            internal WindowHandleWrapper(IntPtr hwnd)
            {
                m_hwnd = hwnd;
            }

            IntPtr IWin32Window.Handle
            {
                get { return m_hwnd; }
            }
        }

        /// <summary>
        /// Gets the VS dialog owner window as an IWin32Window that can be passed to Form.ShowDialog
        /// </summary>
        [SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="Microsoft.VisualStudio.Shell.Interop.IVsUIShell.GetDialogOwnerHwnd(System.IntPtr@)")]
        internal static IWin32Window GetVsDialogOwner(IServiceProvider provider)
        {
            if (s_ownerDialogWindow == null)
            {
                IVsUIShell shell = GetService<SVsUIShell, IVsUIShell>(provider);
                if (shell == null)
                {
                    throw Marshal.GetExceptionForHR(VSConstants.E_FAIL);
                }

                // Get the dialog owner window from the shell
                IntPtr hwnd;
                shell.GetDialogOwnerHwnd(out hwnd);
                s_ownerDialogWindow = new WindowHandleWrapper(hwnd);
            }
            return s_ownerDialogWindow;
        }

        /// <summary>
        /// Gets the project at the root of a VS hieararchy object
        /// </summary>
        /// <param name="hierarchy">The VS hierarchy object</param>
        /// <returns>The project at the root</returns>
        internal static Project GetProjectFromHierarchy(IVsHierarchy hierarchy)
        {
            Debug.Assert(hierarchy != null);

            object obj;

            int hr = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            if (ErrorHandler.Succeeded(hr))
            {
                return obj as Project;
            }

            return null;
        }

        /// <summary>
        /// Gets the VS hierarchy which a given project belongs to
        /// </summary>
        /// <param name="project">The project from which to figure out the VS hierarchy</param>
        /// <param name="provider">The IServiceProvider object for the DTE</param>
        /// <returns>The hierarchy for the given project</returns>
        [SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="Microsoft.VisualStudio.Shell.Interop.IVsSolution.GetProjectOfUniqueName(System.String,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy@)")]
        internal static IVsHierarchy GetHierarchyFromProject(Project project, IServiceProvider provider)
        {
            Debug.Assert(project != null);

            IVsHierarchy hierarchy = null;
            string uniqueName = project.UniqueName;

            IVsSolution solution = provider.GetService(typeof(IVsSolution)) as IVsSolution;
            if (solution == null)
            {
                throw Marshal.GetExceptionForHR(VSConstants.E_FAIL);
            }

            solution.GetProjectOfUniqueName(uniqueName, out hierarchy);
            return hierarchy;
        }

        /// <summary>
        /// Gets the loaded EnvDTE.Project object for a given project file path
        /// </summary>
        /// <param name="projectFile">The full path to the project file</param>
        /// <param name="provider">The IServiceProvider for the DTE</param>
        /// <returns>
        /// The Project object for the given project file, or null if the project
        /// is not loaded.
        /// </returns>
        internal static Project GetProjectFromFileName(string projectFile, IServiceProvider provider)
        {
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            // Get the DTE service and make sure there is an open solution
            EnvDTE.DTE dte = provider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            if (dte == null || dte.Solution == null || dte.Solution.Projects == null)
            {
                return null;
            }

            // In some rare cases (e.g. when you open Silverlight project, but don't have Silverlight tools installed)
            // project DTE object can throw NotImplementedException and crash VS
            try
            {
                // Find the project in the current solution
                foreach (Project project in dte.Solution.Projects)
                {
                    Project targetProject = FindProjectByFileName(projectFile, project);
                    if (targetProject != null)
                    {
                        return targetProject;
                    }
                }
            }
            catch (System.NotImplementedException e)
            {
                Debug.Fail(e.ToString());
                return null;
            }
            

            return null;
        }

        /// <summary>
        /// Recursively searches for a project with a FileName property that matches the given name
        /// </summary>
        internal static Project FindProjectByFileName(string projectName, Project project)
        {
            return FindProjectByName(projectName, project, true);
        }

        /// <summary>
        /// Recursively searches for a project with a FullName property that matches the given name
        /// </summary>
        internal static Project FindProjectByFullName(string projectName, Project project)
        {
            return FindProjectByName(projectName, project, false);
        }

        private static readonly Guid SOLUTIONFOLDER_PROJECT_GUID = new Guid("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}");

        internal static bool IsProjectKind(Project project, Guid projectKindGuid)
        {
            return projectKindGuid == new Guid(project.Kind);
        }

        private static bool IsSolutionFolderProject(Project project)
        {
            return IsProjectKind(project, SOLUTIONFOLDER_PROJECT_GUID);
        }

        /// <summary>
        /// Recursively searches for a project that matches the given name
        /// </summary>
        private static Project FindProjectByName(string projectName, Project project, bool useFileName)
        {
            if (IsSolutionFolderProject(project))
            {
                foreach (ProjectItem subItem in project.ProjectItems)
                {
                    if (subItem.SubProject != null)
                    {
                        Project targetProject = FindProjectByName(projectName, subItem.SubProject, useFileName);
                        if (targetProject != null)
                        {
                            return targetProject;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    string projectProperty;
                    if (useFileName)
                    {
                        projectProperty = project.FileName;
                    }
                    else
                    {
                        projectProperty = project.FullName;
                    }

                    if (string.Equals(projectProperty, projectName, StringComparison.OrdinalIgnoreCase))
                    {
                        return project;
                    }
                }
                catch (NotImplementedException)
                {
                    // If the project has been unloaded, attempting to reference FileName or FullName or other properties
                    // causes the NotImplementedException to be thrown.
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns currently loaded solution
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static EnvDTE.Solution GetCurrentSolution(IServiceProvider provider)
        {
            // Get the DTE service and make sure there is an open solution
            EnvDTE.DTE dte = provider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            if (dte == null || dte.Solution == null)
            {
                return null;
            }
            else
            {
                return dte.Solution;
            }
        }

        /// <summary>
        /// Returns the currently selected project in the current user context
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static EnvDTE.Project GetSelectedProject(IServiceProvider provider)
        {
            // Get the DTE service and make sure there is an open solution
            EnvDTE.DTE dte = provider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            if (dte == null || dte.Solution == null)
            {
                return null;
            }

            EnvDTE.Project project = null;
            IntPtr selectionHierarchy = IntPtr.Zero;
            IntPtr selectionContainer = IntPtr.Zero;

            // Get the current selection in the shell
            IVsMonitorSelection monitorSelection = provider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (monitorSelection != null)
            {
                try
                {
                    uint itemId;
                    IVsMultiItemSelect multiSelect;

                    monitorSelection.GetCurrentSelection(out selectionHierarchy, out itemId, out multiSelect, out selectionContainer);
                    if (selectionHierarchy != IntPtr.Zero)
                    {
                        IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(selectionHierarchy) as IVsHierarchy;
                        Debug.Assert(hierarchy != null);
                        project = GetProjectFromHierarchy(hierarchy);
                    }
                }
                catch (Exception)
                {
                    // If anything went wrong, just ignore it
                }
                finally
                {
                    // Make sure we release the COM pointers in any case
                    if (selectionHierarchy != IntPtr.Zero)
                    {
                        Marshal.Release(selectionHierarchy);
                    }
                    if (selectionContainer != IntPtr.Zero)
                    {
                        Marshal.Release(selectionContainer);
                    }
                }
            }

            return project;
        }

        /// <summary>
        /// Returns the currently selected project hierarchy in the current user context
        /// </summary>
        internal static IVsHierarchy GetSelectedProjectHierarchy(IServiceProvider provider)
        {
            EnvDTE.Project project = provider != null ? GetSelectedProject(provider) : null;
            return project != null ? GetHierarchyFromProject(project, provider) : null;
        }

        /// <summary>
        /// Returns the project directory for a given project
        /// </summary>
        internal static string GetProjectDirectory(EnvDTE.Project project)
        {
            if (project == null)
            {
                return string.Empty;
            }

            if (ProjectHelper.IsWebProject(project))
            {
                // If it is a web project then get the full path from the Properties collection.
                // The FullName property can be a URI for IIS web projects.
                return project.Properties.Item("FullPath").Value as string;
            }
            else
            {
                // For other projects get the full path to the project file and return the directory portion.
                return Path.GetDirectoryName(project.FullName);
            }
        }

        /// <summary>
        /// Returns the AppId setting for AppIdUsesIsolatedCLR
        /// </summary>
        internal static bool UsingRascalPro(IServiceProvider provider)
        {
            return GetAppidSetting(provider, s_appIdUsesIsolatedCLR);
        }

        /// <summary>
        /// Returns the AppId setting for the given AppId guid
        /// </summary>
        internal static bool GetAppidSetting(IServiceProvider provider, Guid setting)
        {
            int isActive = 0;

            // Get the command UI context from the monitor service
            IVsMonitorSelection monitor = provider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (monitor != null)
            {
                uint cookie = 0;
                if (monitor.GetCmdUIContextCookie(ref setting, out cookie) == VSConstants.S_OK)
                {
                    int hr = monitor.IsCmdUIContextActive(cookie, out isActive);
                    if (VSConstants.S_OK != hr)
                    {
                        Debug.Fail("IVsMonitorSelection.IsCmdUIContextActive failed.");
                        // If the call fails then default to the context not being active.
                        isActive = 0;
                    }
                }
            }

            return isActive != 0;
        }

        /// <summary>
        /// Open the document associated with this task item
        /// </summary>
        /// <returns>The window frame of the opened document</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        internal static IVsWindowFrame OpenDocument(IServiceProvider provider, string file, bool usePreviewPane)
        {
            if (string.IsNullOrEmpty(file))
            {
                // No place to go
                return null;
            }

            // We should not throw exceptions if we cannot find the file 
            if (!File.Exists(file))
            {
                return null;
            }

            TelemetryProvider.WriteEvent(TelemetryEvent.TaskItemDocumentOpened);

            try
            {
                if (usePreviewPane)
                {
                    // The scope below ensures that if a document is not yet open, it is opened in the preview pane.
                    // For documents that are already open, they will remain in their current pane, which may be the preview
                    // pane or the full editor pane.
                    using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE.NDS_Provisional | __VSNEWDOCUMENTSTATE.NDS_NoActivate, Microsoft.VisualStudio.VSConstants.NewDocumentStateReason.Navigation))
                    {
                        return OpenDocumentInCurrentScope(provider, file);
                    }
                }
                else
                {
                    return OpenDocumentInCurrentScope(provider, file);
                }
            }
            catch (COMException)
            {
                string fname = Path.GetFileName(file);
                if (System.Windows.Forms.MessageBox.Show(string.Format(Resources.FileOpenFail_DialogMessage, fname),
                                                                       Resources.FileOpenFail_DialogCaption,
                                                                       MessageBoxButtons.YesNo,
                                                                       MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(file));
                }

                return null;
            }
        }

        /// <summary>
        /// Open the file using the current document state scope
        /// </summary>
        private static IVsWindowFrame OpenDocumentInCurrentScope(IServiceProvider provider, string file)
        {
            IVsUIShellOpenDocument openDoc = SdkUIUtilities.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>(provider);
            IVsRunningDocumentTable runningDocTable = SdkUIUtilities.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>(provider);
            if (openDoc == null || runningDocTable == null)
            {
                throw Marshal.GetExceptionForHR(VSConstants.E_FAIL);
            }

            uint cookieDocLock = FindDocument(runningDocTable, file);

            IVsWindowFrame windowFrame;
            Guid textViewGuid = VSConstants.LOGVIEWID_TextView;
            // Unused variables
            IVsUIHierarchy uiHierarchy;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider;
            uint itemId;
            int hr = openDoc.OpenDocumentViaProject(file, ref textViewGuid, out serviceProvider, out uiHierarchy, out itemId, out windowFrame);
            if (ErrorHandler.Failed(hr))
            {
                throw Marshal.GetExceptionForHR(hr);
            }

            if (cookieDocLock == 0) // Document was not open earlier, and should be open now.
            {
                cookieDocLock = FindDocument(runningDocTable, file);
            }

            if (windowFrame != null)
            {
                // This will make the document visible to the user and switch focus to it. ShowNoActivate doesn't help because for tabbed documents they
                // are not brought to the front if they are already opened.
                windowFrame.Show();
            }
            return windowFrame;
        }

        /// <summary>
        /// Find the document and return its cookie to the lock to the document
        /// </summary>
        /// <param name="runningDocTable">The object having a table of all running documents</param>
        /// <returns>The cookie to the document lock</returns>
        internal static uint FindDocument(IVsRunningDocumentTable runningDocTable, string file)
        {
            // Unused variables
            IVsHierarchy hierarchy;
            uint itemId;
            IntPtr docData = IntPtr.Zero;

            uint cookieDocLock;
            int hr = runningDocTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, file, out hierarchy, out itemId, out docData, out cookieDocLock);

            // Although we don't use it, we still need to release the it
            if (docData != IntPtr.Zero)
            {
                Marshal.Release(docData);
                docData = IntPtr.Zero;
            }

            if (ErrorHandler.Failed(hr))
            {
                throw Marshal.GetExceptionForHR(hr);
            }

            if (cookieDocLock > 0) // Document is already open
            {
                uint rdtFlags;

                // Unused variables
                uint readLocks;
                uint editLocks;
                string documentName;

                hr = runningDocTable.GetDocumentInfo(cookieDocLock, out rdtFlags, out readLocks, out editLocks, out documentName, out hierarchy, out itemId, out docData);

                // Although we don't use it, we still need to release the it
                if (docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                    docData = IntPtr.Zero;
                } 
                
                if (ErrorHandler.Failed(hr))
                {
                    throw Marshal.GetExceptionForHR(hr);
                }

                if ((rdtFlags & ((uint)_VSRDTFLAGS.RDT_ProjSlnDocument)) > 0)
                {
                    throw Marshal.GetExceptionForHR(VSConstants.E_FAIL);
                }
            }

            return cookieDocLock;
        }

        private static char[] s_directorySeparatorArray = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Creates a relative path from one directory to another directory or file
        /// </summary>
        /// <param name="fromDirectory">The directory that defines the start of the relative path.</param>
        /// <param name="toPath">The path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        internal static string GetRelativePath(string fromDirectory, string toPath)
        {
            // Both paths need to be rooted to calculate a relative path
            if (!Path.IsPathRooted(fromDirectory) ||
                !Path.IsPathRooted(toPath))
            {
                return toPath;
            }

            // If toPath is on a different drive then there is no relative path
            if (0 != string.Compare(Path.GetPathRoot(fromDirectory),
                                    Path.GetPathRoot(toPath),
                                    StringComparison.OrdinalIgnoreCase))
            {
                return toPath;
            }

            // Get the canonical path. This resolves directory names like "\.\" and "\..\".
            fromDirectory = Path.GetFullPath(fromDirectory);
            toPath = Path.GetFullPath(toPath);

            string[] fromDirectories = fromDirectory.Split(s_directorySeparatorArray, StringSplitOptions.RemoveEmptyEntries);
            string[] toDirectories = toPath.Split(s_directorySeparatorArray, StringSplitOptions.RemoveEmptyEntries);

            int length = Math.Min(fromDirectories.Length, toDirectories.Length);

            // We know at least the drive letter matches so start at index 1
            int firstDifference = 1;

            // Find the common root
            for (; firstDifference < length; firstDifference++)
            {
                if (0 != string.Compare(fromDirectories[firstDifference],
                                        toDirectories[firstDifference],
                                        StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            StringCollection relativePath = new StringCollection();

            // Add relative paths to get from fromDirectory to the common root
            for (int i = firstDifference; i < fromDirectories.Length; i++)
            {
                relativePath.Add("..");
            }

            // Add the relative paths from toPath
            for (int i = firstDifference; i < toDirectories.Length; i++)
            {
                relativePath.Add(toDirectories[i]);
            }

            // Create the relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);
            return string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);
        }

        /// <summary>
        /// Creates the shortest path with the greedy approach. That is, it makes the preference based on this order
        /// 1) If it's in the search paths, then just return the file name
        /// 2) If it shares the same common root with the relativePathBase, then it returns a relative path
        /// 3) Returns the full path as given
        /// </summary>
        /// <param name="fullPath">The full path to the file</param>
        /// <param name="searchPaths">The collection of search paths to use</param>
        /// <param name="relativePathBase">The base path for constructing the relative paths</param>
        /// <returns>The shortest path</returns>
        internal static string MakeShortestPath(string fullPath, IEnumerable<string> searchPaths, string relativePathBase)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return string.Empty;
            }

            string directory = Path.GetDirectoryName(fullPath);

            if (searchPaths != null)
            {
                foreach (string searchPath in searchPaths)
                {
                    if (directory.Equals(searchPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return Path.GetFileName(fullPath);
                    }
                }
            }

            if (!string.IsNullOrEmpty(relativePathBase))
            {
                return GetRelativePath(relativePathBase, fullPath);
            }

            return fullPath;            
        }

        /// <summary>
        /// This is the inverse of MakeShortestPath, it is taking a path, it may be full, relative, or just a file name, then
        /// make a full path out of it
        /// </summary>
        /// <param name="fileName">The file, can be a full, relative, or just a file name</param>
        /// <param name="searchPaths">The collection of search paths to use</param>
        /// <param name="relativePathBase">The base path for resolving the path</param>
        /// <returns>The full path</returns>
        internal static string MakeFullPath(string fileName, IEnumerable<string> searchPaths, string relativePathBase)
        {
            // Simple case, if it's rooted, just return it
            if (Path.IsPathRooted(fileName))
            {
                return fileName;
            }

            // Check if it is in the relative directory
            if (!string.IsNullOrEmpty(relativePathBase))
            {
                // Using GetFullPath to remove any \..\ in the path
                string path = Path.GetFullPath(Path.Combine(relativePathBase, fileName));
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // If it's just a file name, search the search paths
            if (searchPaths != null && fileName == Path.GetFileName(fileName))
            {
                foreach (string searchPath in searchPaths)
                {
                    string path = Path.Combine(searchPath, fileName);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        internal static bool IsFormOnScreen(System.Windows.Forms.Form form)
        {
            System.Drawing.Rectangle totalScreens = System.Drawing.Rectangle.Empty;
            //we calculate the screens again each time we're called as screens are not static
            System.Windows.Forms.Screen screen;
            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; ++i)
            {
                screen = System.Windows.Forms.Screen.AllScreens[i];
                totalScreens = System.Drawing.Rectangle.Union(totalScreens, screen.WorkingArea);
            }
            return totalScreens.Contains(form.DesktopBounds);
        }

        /// <summary>
        /// Gets the list of directories to search for rule sets
        /// </summary>
        internal static string[] GetRuleSetDirectories(IServiceProvider provider)
        {
            if (s_ruleSetDirectories == null)
            {
                // We will have more of these if we implement a Tools.Options dialog for rule set paths
                s_ruleSetDirectories = new string[] {GetBuiltInRuleSetDirectory(provider)};
            }

            return s_ruleSetDirectories;
        }

        /// <summary>
        /// Gets the built-in rule sets directory
        /// </summary>
        internal static string GetBuiltInRuleSetDirectory(IServiceProvider provider)
        {
            if (s_builtInRuleSetDirectory == null)
            {
                s_builtInRuleSetDirectory = Path.Combine(GetStaticAnalysisToolsDirectory(provider), @"Rule Sets");
            }

            return s_builtInRuleSetDirectory;
        }

        /// <summary>
        /// Gets the analyzer plug-ins directory
        /// </summary>
        internal static string GetPlugInFileDirectory(IServiceProvider provider)
        {
            if (s_plugInsDirectory == null)
            {
                s_plugInsDirectory = Path.Combine(GetStaticAnalysisToolsDirectory(provider), @"PlugIns");
            }

            return s_plugInsDirectory;
        }

        /// <summary>
        /// Gets the Static Analysis Tools directory
        /// </summary>
        private static string GetStaticAnalysisToolsDirectory(IServiceProvider provider)
        {
            if (s_staticAnalysisToolsDirectory == null)
            {
                string installDirectory = null;

                // Get the VS install directory
                IVsShell shell = (IVsShell)provider.GetService(typeof(IVsShell));
                if (shell != null)
                {
                    object value;
                    if (shell.GetProperty((int)__VSSPROPID2.VSSPROPID_InstallRootDir, out value) == VSConstants.S_OK)
                    {
                        installDirectory = value as string;
                    }
                }

                // If we failed to get the install directory through the shell service (this is possible if we are called
                // from the policy object and we are running in tf.exe and not devenv.exe), then just try to deduce the
                // install directory from the location of this assembly, which should be under common7\ide\privateassemblies
                if (string.IsNullOrEmpty(installDirectory))
                {
                    installDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\"));
                }

                s_staticAnalysisToolsDirectory = Path.Combine(installDirectory, @"Team Tools\Static Analysis Tools");
            }

            return s_staticAnalysisToolsDirectory;
        }

        /// <summary>
        /// Gets file paths for all the rule set files found in the rule set search paths
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static List<string> GetAllRuleSetFiles(IServiceProvider provider)
        {
            List<string> ruleSetFiles = new List<string>();
            List<string> fileNames = new List<string>();

            // Look through the search paths
            foreach (string path in GetRuleSetDirectories(provider))
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        string[] files = Directory.GetFiles(path, "*" + SdkUIUtilities.RuleSetFileExtension);
                        foreach (string file in files)
                        {
                            string fileName = Path.GetFileName(file).ToLowerInvariant();
                            if (!fileNames.Contains(fileName))
                            {
                                // Found a rule set with a unique file name
                                fileNames.Add(fileName);
                                ruleSetFiles.Add(file);
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    // If something went wrong while processing this path, then just skip it
                }
            }

            return ruleSetFiles;
        }

        internal static Color GetDesignerThemeColor(IVsUIShell5 uiShellService, Guid themeCategory, String themeColorName, __THEMEDCOLORTYPE colorType, Color defaultColor)
        {
            if (uiShellService != null)
            {
                UInt32 rgbaValue = 0;

                Int32 hr = Microsoft.VisualStudio.ErrorHandler.CallWithCOMConvention(
                    () =>
                    {
                        rgbaValue = uiShellService.GetThemedColor(themeCategory, themeColorName, (System.UInt32)colorType);
                    });

                if (Microsoft.VisualStudio.ErrorHandler.Succeeded(hr))
                {
                    return RGBAToColor(rgbaValue);
                }
            }

            return defaultColor;
        }

        private static Color RGBAToColor(UInt32 rgbaValue)
        {
            return Color.FromArgb(
                (int)((rgbaValue & 0xFF000000U) >> 24),
                (int)(rgbaValue & 0xFFU),
                (int)((rgbaValue & 0xFF00U) >> 8),
                (int)((rgbaValue & 0xFF0000U) >> 16));
        }


        /// <summary>
        /// Determines whether the shell is in command line mode.
        /// </summary>
        /// <param name="serviceProvider">A reference to a Service Provider.</param>
        /// <returns>true if the shell is in command line mode. false otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static bool IsShellInCommandLineMode(System.IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            IVsShell shell = serviceProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (shell == null)
            {
                throw new InvalidOperationException();
            }

            object isInCommandLineModeAsObject;
            ErrorHandler.ThrowOnFailure(shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out isInCommandLineModeAsObject));

            return ((bool)isInCommandLineModeAsObject);
        }

        /// <summary>
        /// Builds a set of Inline elements from the specified message, without embedded hyperlinks.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A collection of Inline elements that represent the specified message.</returns>
        internal static List<Inline> GetInlinesForErrorMessage(string message)
        {
            return GetMessageInlines(message, -1, null);
        }

        /// <summary>
        /// Builds a set of Inline elements from the specified message, optionally with embedded hyperlinks.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="index">The index of the error item</param>
        /// <param name="clickHandler">A delegate for the Hyperlink.Click event.</param>
        /// <returns>A collection of Inline elements that represent the specified message.</returns>
        internal static List<Inline> GetMessageInlines(string message, int index, RoutedEventHandler clickHandler)
        {
            var inlines = new List<Inline>();

            MatchCollection matches = Regex.Matches(message, EmbeddedLinkPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            int start = 0;

            if (matches.Count > 0)
            {
                Group group = null;

                foreach (Match match in matches)
                {
                    group = match.Groups["link"];

                    // Add the plain text segment between the end of the last group and the current link
                    inlines.Add(new Run(UnescapeBrackets(message.Substring(start, group.Index - 1 - start))));

                    if (clickHandler != null)
                    {
                        var link = new Hyperlink();

                        // Stash the error index and relative link id
                        link.Tag = new Tuple<int, int>(index, Convert.ToInt32(match.Groups["index"].Value));

                        // Set the hyperlink text
                        link.Inlines.Add(new Run($"{group.Value}"));
                        link.Click += clickHandler;

                        inlines.Add(link);
                    }
                    else
                    {
                        // Add the link text as plain text
                        inlines.Add(new Run($"{group.Value}"));
                    }

                    start = match.Index + match.Length;
                }
            }

            if (start < message.Length)
            {
                // Add the plain text segment after the last link
                inlines.Add(new Run(UnescapeBrackets(message.Substring(start))));
            }

            return inlines;
        }

        /// <summary>
        /// Removes escape backslashes that were used to suppress embedded linking.
        /// </summary>
        /// <param name="s">The string to be processed.</param>
        /// <returns></returns>
        internal static string UnescapeBrackets(string s)
        {
            return s.Replace(@"\[", "[").Replace(@"\]", "]");
        }
    }
}
