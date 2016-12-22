// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Microsoft.Sarif.Viewer.Sarif;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// This class is responsible for coordinating Code Analysis results end-to-end from the underlying
    /// implementation to the user interface activities.
    /// </summary>
    [Guid("4494F79A-6E9F-45EA-895B-7AE959B94D6A")]
    public sealed class CodeAnalysisResultManager : IVsSolutionEvents, IVsUpdateSolutionEvents2, IVsRunningDocTableEvents
    {
        internal const int E_FAIL = unchecked((int)0x80004005);
        internal const uint VSCOOKIE_NIL = 0;
        internal const int S_OK = 0;

        // Cookies for registration and unregistration
        private uint m_updateSolutionEventsCookie;
        private uint m_solutionEventsCookie;
        private uint m_runningDocTableEventsCookie;
        private Dictionary<string, Uri> _remappedUriBasePaths;
        private List<Tuple<string, string>> _remappedPathPrefixes;
        private Dictionary<string, NewLineIndex> _fileToNewLineIndexMap;
        private IList<SarifErrorListItem> _sarifErrors = new List<SarifErrorListItem>();
        private IVsRunningDocumentTable _runningDocTable;

        private CodeAnalysisResultManager()
        {
            this.SarifErrors = new List<SarifErrorListItem>();
            _remappedUriBasePaths = new Dictionary<string, Uri>();
            _remappedPathPrefixes = new List<Tuple<string, string>>();
            _fileToNewLineIndexMap = new Dictionary<string, NewLineIndex>();
        }

        private System.IServiceProvider ServiceProvider
        {
            get
            {
                return SarifViewerPackage.ServiceProvider;
            }
        }

        private SarifViewerPackage Package
        {
            get
            {
                return (SarifViewerPackage)SarifViewerPackage.ServiceProvider;
            }
        }

        public static CodeAnalysisResultManager Instance = new CodeAnalysisResultManager();

        public IList<SarifErrorListItem> SarifErrors
        {
            get
            {
                return _sarifErrors;
            }
            set
            {
                // Since we have a new set of Results in the Error List, clear all source code highlighting.
                DetachFromAllDocuments();
                _sarifErrors = value;
            }
        }

        SarifErrorListItem m_currentSarifError;
        public SarifErrorListItem CurrentSarifError
        {
            get
            {
                return m_currentSarifError;
            }
            set
            {
                ClearCurrentMarkers();
                m_currentSarifError = value;
            }
        }

        private void ClearCurrentMarkers()
        {
            if (CurrentSarifError != null)
            {
                CurrentSarifError.RemoveMarkers();
            }
        }

        internal void Register()
        {
            // Register this object to listen for IVsUpdateSolutionEvents
            IVsSolutionBuildManager2 buildManager = Package.GetService<SVsSolutionBuildManager, IVsSolutionBuildManager2>();
            if (buildManager == null)
            {
                throw Marshal.GetExceptionForHR(E_FAIL);
            }
            buildManager.AdviseUpdateSolutionEvents(this, out m_updateSolutionEventsCookie);

            // Register this object to listen for IVsSolutionEvents
            IVsSolution solution = Package.GetService<SVsSolution, IVsSolution>();
            if (solution == null)
            {
                throw Marshal.GetExceptionForHR(E_FAIL);
            }
            solution.AdviseSolutionEvents(this, out m_solutionEventsCookie);

            // Register this object to listen for IVsRunningDocTableEvents
            _runningDocTable = Package.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
            if (_runningDocTable == null)
            {
                throw Marshal.GetExceptionForHR(E_FAIL);
            }
            _runningDocTable.AdviseRunningDocTableEvents(this, out m_runningDocTableEventsCookie);
        }

        /// <summary>
        /// Unregister this provider from VS
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        internal void Unregister()
        {
            // Unregister this object from IVsUpdateSolutionEvents events
            if (m_updateSolutionEventsCookie != VSCOOKIE_NIL)
            {

                IVsSolutionBuildManager2 buildManager = Package.GetService<SVsSolutionBuildManager, IVsSolutionBuildManager2>();
                if (buildManager != null)
                {
                    buildManager.UnadviseUpdateSolutionEvents(m_updateSolutionEventsCookie);
                    m_updateSolutionEventsCookie = VSCOOKIE_NIL;
                }
            }

            // Unregister this object from IVsSolutionEvents events
            if (m_solutionEventsCookie != VSCOOKIE_NIL)
            {
                IVsSolution solution = Package.GetService<SVsSolution, IVsSolution>();
                if (solution != null)
                {
                    solution.UnadviseSolutionEvents(m_solutionEventsCookie);
                    m_solutionEventsCookie = VSCOOKIE_NIL;
                }
            }

            // Unregister this object from IVsRunningDocTableEvents events
            if (m_runningDocTableEventsCookie != VSCOOKIE_NIL)
            {
                IVsRunningDocumentTable runningDocTable = Package.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
                if (runningDocTable != null)
                {
                    runningDocTable.UnadviseRunningDocTableEvents(m_runningDocTableEventsCookie);
                    m_runningDocTableEventsCookie = VSCOOKIE_NIL;
                }
            }
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return S_OK;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return S_OK;
        }

        public bool TryRebaselineCurrentSarifError(string uriBaseId, string originalFilename)
        {
            if (CurrentSarifError == null)
            {
                return false;
            }

            string rebaselinedFile;

            if (Uri.IsWellFormedUriString(originalFilename, UriKind.Absolute))
            {
                rebaselinedFile = DownloadFile(originalFilename);
            }
            else
            {
                rebaselinedFile = GetRebaselinedFileName(uriBaseId, originalFilename);
            }

            if (String.IsNullOrEmpty(rebaselinedFile) || originalFilename.Equals(rebaselinedFile, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            CurrentSarifError.RemapFilePath(originalFilename, rebaselinedFile);
            return true;
        }

        internal string DownloadFile(string fileUrl)
        {
            if (String.IsNullOrEmpty(fileUrl))
            {
                return fileUrl;
            }

            Uri sourceUri = new Uri(fileUrl);

            string destinationFile = Path.Combine(CurrentSarifError.WorkingDirectory, sourceUri.LocalPath.Replace('/', '\\').TrimStart('\\'));
            string destinationDirectory = Path.GetDirectoryName(destinationFile);
            Directory.CreateDirectory(destinationDirectory);

            if (!File.Exists(destinationFile))
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(sourceUri, destinationFile);
                }
            }

            return destinationFile;
        }

        private static string DefaultUriBase = Guid.NewGuid().ToString();

        public string GetRebaselinedFileName(string uriBaseId, string fileName)
        {
            Uri relativeUri;

            uriBaseId = uriBaseId ?? DefaultUriBase;

            if (Uri.TryCreate(fileName, UriKind.Relative, out relativeUri))
            {
                // If the relative file path is relative to an unknown root,
                // we need to strip the leading slash, so that we can relate
                // the file path to an arbitrary remapped disk location.
                if (fileName.StartsWith("/"))
                {
                    fileName = fileName.Substring(1);
                }

                if (_remappedUriBasePaths.ContainsKey(uriBaseId))
                {
                    return new Uri(_remappedUriBasePaths[uriBaseId], fileName).LocalPath;
                }
            }
            else
            {
                // First, we'll traverse our remappings and see if we can
                // make rebaseline from existing data
                foreach (Tuple<string, string> remapping in _remappedPathPrefixes)
                {
                    string remapped = fileName.Replace(remapping.Item1, remapping.Item2);
                    if (File.Exists(remapped))
                    {
                        return remapped;
                    }
                }
            }

            // Opening the OpenFileDialog causes the TreeView to lose focus, 
            // which in turn causes the TreeViewItem selection to be unpredictable 
            // (because the selection event relies on the TreeViewItem focus.)
            // We'll save the element which currently has focus and then restore
            // focus after the OpenFileDialog is closed.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
            
            OpenFileDialog openFileDialog = new OpenFileDialog();

            string fullPath = Path.GetFullPath(fileName);
            string shortName = Path.GetFileName(fullPath);

            openFileDialog.Title = "Locate missing file: " + fullPath;
            openFileDialog.Filter = shortName + "|" + shortName;
            openFileDialog.RestoreDirectory = true;

            try
            {
                bool? dialogResult = openFileDialog.ShowDialog();

                if (!dialogResult.HasValue || !dialogResult.Value)
                {
                    return fileName;
                }
            }
            finally
            {
                if (elementWithFocus != null)
                {
                    elementWithFocus.Focus();
                }
            }

            string resolvedPath = openFileDialog.FileName;

            string resolvedFileName = Path.GetFileName(resolvedPath);

            // If remapping has somehow altered the file name itself,
            // we will bail on attempting to do any remapping
            if (!Path.GetFileName(fullPath).Equals(resolvedFileName, StringComparison.OrdinalIgnoreCase))
            {
                return fileName;
            }

            int offset = resolvedFileName.Length;
            while ((resolvedPath.Length - offset) >= 0 &&
                   (fullPath.Length - offset) >= 0)
            {
                if (!resolvedPath[resolvedPath.Length - offset].ToString().Equals(fullPath[fullPath.Length - offset].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                offset++;
            }

            offset--;

            // At this point, we've got our hands on the common suffix for both the 
            // original file path and the resolved location. we trim this off both
            // values and then add a remapping that converts one to the other
            string originalPrefix = fullPath.Substring(0, fullPath.Length - offset);
            string resolvedPrefix = resolvedPath.Substring(0, resolvedPath.Length - offset);

            if (relativeUri != null)
            {                
                _remappedUriBasePaths[uriBaseId] = new Uri(resolvedPath.Substring(0, resolvedPath.IndexOf(fileName.Replace("/", @"\"))), UriKind.Absolute);
            }
            else
            {
                _remappedPathPrefixes.Add(new Tuple<string, string>(originalPrefix, resolvedPrefix));
            }

            return resolvedPath;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return S_OK;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            return S_OK;
        }

        internal static bool CanNavigateTo(SarifErrorListItem sarifError)
        {
            throw new NotImplementedException();
        }

        internal void RemapFileNames(string originalPath, string remappedPath)
        {
            foreach (SarifErrorListItem sarifError in SarifErrors)
            {
                sarifError.RemapFilePath(originalPath, remappedPath);
            }
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            return S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            AttachToDocumentChanges(docCookie, pFrame);
            return S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            DetachFromDocumentChanges(docCookie);
            return S_OK;
        }

        /// <summary>
        /// Try to get documentname for current document with <param name="docCookie" />
        /// and invoke attach for each item in analysis results collection. 
        /// </summary>
        private void AttachToDocumentChanges(uint docCookie, IVsWindowFrame pFrame)
        {
            string documentName = GetDocumentName(docCookie, pFrame);

            if (!string.IsNullOrEmpty(documentName))
            {
                if (SarifErrors != null)
                {
                    foreach (SarifErrorListItem sarifError in SarifErrors)
                    {
                        sarifError.AttachToDocument(documentName, (long)docCookie, pFrame);
                    }
                }
            }
        }

        /// <summary>
        /// Invoke detach for each item in analysis results collection
        /// </summary>
        private void DetachFromDocumentChanges(uint docCookie)
        {
            if (SarifErrors != null)
            {
                foreach (SarifErrorListItem sarifError in SarifErrors)
                {
                    sarifError.DetachFromDocument((long)docCookie);
                }
            }
        }

        // Detaches the SARIF results from all documents.
        public void DetachFromAllDocuments()
        {
            IEnumRunningDocuments documentsEnumerator;

            if (_runningDocTable != null)
            {
                _runningDocTable.GetRunningDocumentsEnum(out documentsEnumerator);

                if (documentsEnumerator != null)
                {
                    uint requestedCount = 1;
                    uint actualCount;
                    uint[] cookies = new uint[requestedCount];

                    while (true)
                    {
                        documentsEnumerator.Next(requestedCount, cookies, out actualCount);
                        if (actualCount == 0)
                        {
                            // There are no more documents to process.
                            break;
                        }

                        // Detach from document.
                        DetachFromDocumentChanges(cookies[0]);
                    }
                }
            }
        }

        /// <summary>
        /// 
        private string GetDocumentName(uint docCookie, IVsWindowFrame pFrame)
        {
            string documentName = null;
            IVsRunningDocumentTable runningDocTable = SdkUiUtilities.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>(ServiceProvider);
            if (runningDocTable != null)
            {
                uint grfRDTFlags;
                uint dwReadLocks;
                uint dwEditLocks;
                IVsHierarchy pHier;
                uint itemId;
                IntPtr docData = IntPtr.Zero;
                try
                {
                    int hr = runningDocTable.GetDocumentInfo(docCookie,
                                            out grfRDTFlags,
                                            out dwReadLocks,
                                            out dwEditLocks,
                                            out documentName,
                                            out pHier,
                                            out itemId,
                                            out docData);

                }
                finally
                {
                    if (docData != IntPtr.Zero)
                    {
                        Marshal.Release(docData);
                    }
                }
            }
            return documentName;
        }

    }
}