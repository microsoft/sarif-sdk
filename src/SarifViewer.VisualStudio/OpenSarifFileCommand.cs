//------------------------------------------------------------------------------
// <copyright file="OpenSarifFileCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Newtonsoft.Json;

namespace SarifViewer
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenSarifFileCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a236a757-af66-4cf0-a3c8-facbb61d5cf1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSarifFileCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenSarifFileCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenSarifFileCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new OpenSarifFileCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Open Static Analysis Results Interchange Format (SARIF) file"; ;
            openFileDialog.Filter = "SARIF files (*.sarif;*.sarif.json)|*.sarif;*.sarif.json";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string sarifText = File.ReadAllText(openFileDialog.FileName);

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
            };

            // Make sure we can successfully deserialize what was just generated
            ResultLog log = JsonConvert.DeserializeObject<ResultLog>(sarifText, settings);

            AddResultsToErrorList(log);
        }

        private Dictionary<string, NewLineIndex> documentToLineIndexMap;
        private Dictionary<Task, Region> taskToRegionMap;
        private ErrorListProvider errorListProvider;

        private void AddResultsToErrorList(ResultLog log)
        {
            var dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));
            this.errorListProvider = new ErrorListProvider(ServiceProvider);

            this.documentToLineIndexMap = new Dictionary<string, NewLineIndex>();
            this.taskToRegionMap = new Dictionary<Task, Region>();
            this.errorListProvider.Tasks.Clear();

            foreach (RunLog runLog in log.RunLogs)
            {
                foreach (Result result in runLog.Results)
                {
                    TaskErrorCategory category = ConvertResultKindToTaskCategory(result.Kind);
                    string message;
                    string document;
                    Region region;
                    NewLineIndex newLineIndex;

                    foreach (Location location in result.Locations)
                    {
                        if (location.ResultFile != null)
                        {
                            // TODO helper to provide best representation of URI
                            document = location.ResultFile[0].Uri.LocalPath;

                            // TODO retrieve file from nested component
                            region = location.ResultFile[0].Region;
                        }
                        else
                        {
                            document = location.AnalysisTarget[0].Uri.LocalPath;
                            region = location.AnalysisTarget[0].Region;
                        }

                        if (!this.documentToLineIndexMap.TryGetValue(document, out newLineIndex))
                        {
                            this.documentToLineIndexMap[document] = newLineIndex = new NewLineIndex(File.ReadAllText(document));
                        }

                        if (region != null)
                        {
                            region.Populate(newLineIndex);
                        }

                        IRuleDescriptor rule = GetRule(runLog, result.RuleId);

                        message = result.GetMessageText(rule, concise: true);

                        var error = new ErrorTask()
                        {
                            ErrorCategory = category,
                            Text = message,
                            CanDelete = false,
                            Category = TaskCategory.BuildCompile,
                            Column = region != null ? region.StartColumn : 1,
                            Document = document,
                            Line = region != null ? region.StartLine : 1,
                            Priority = TaskPriority.Normal                             
                        };
                        error.Navigate += ErrorOnNavigate;
                        this.taskToRegionMap[error] = region;
                        this.errorListProvider.Tasks.Add(error);
                    }

                    if (result.RelatedLocations != null)
                    {
                        foreach (AnnotatedCodeLocation annotation in result.RelatedLocations)
                        {
                            region = annotation.PhysicalLocation[0].Region;
                            message = annotation.Message;
                            document = annotation.PhysicalLocation[0].Uri.LocalPath;

                            if (!this.documentToLineIndexMap.TryGetValue(document, out newLineIndex))
                            {
                                this.documentToLineIndexMap[document] = newLineIndex = new NewLineIndex(File.ReadAllText(document));
                            }

                            if (region != null)
                            {
                                region.Populate(newLineIndex);
                            }

                            var error = new ErrorTask()
                            {
                                ErrorCategory = TaskErrorCategory.Message,
                                Text = message,
                                CanDelete = false,
                                Category = TaskCategory.BuildCompile,
                                Column = region != null ? region.StartColumn : 1,
                                Document = document,
                                Line = region != null ? region.StartLine : 1,
                                Priority = TaskPriority.Normal
                            };
                        }
                    }
                }
            }

            this.errorListProvider.Show();
        }

        private IRuleDescriptor GetRule(RunLog runLog, string ruleId)
        {
            foreach (IRuleDescriptor ruleDescriptor in runLog.RuleInfo)
            {
                if (ruleDescriptor.Id == ruleId) { return ruleDescriptor; }
            }
            throw new InvalidOperationException();
        }

        private TaskErrorCategory ConvertResultKindToTaskCategory(ResultKind kind)
        {
            switch (kind)
            {
                case ResultKind.ConfigurationError:
                case ResultKind.InternalError:
                case ResultKind.Error:
                {
                    return TaskErrorCategory.Error;
                }

                case ResultKind.Warning:
                {
                    return TaskErrorCategory.Warning;
                }

                case ResultKind.NotApplicable:
                case ResultKind.Note:
                case ResultKind.Pass:
                {
                    return TaskErrorCategory.Message;
                }
            }

            throw new InvalidOperationException();
        }

        private void ErrorOnNavigate(object sender, EventArgs e)
        {
            Task task = (Task)sender;

            if (task == null)
            {
                throw new ArgumentException("sender");
            }

            // If the name of the file connected to the task is empty there is nowhere to navigate to   
            if (String.IsNullOrEmpty(task.Document))
            {
                return;
            }

            Guid logicalView = VSConstants.LOGVIEWID_Code;
            string vsViewKindCode = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";
            var dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));
            var window = dte.ItemOperations.OpenFile(task.Document, vsViewKindCode);

            // Get the VsTextBuffer   
            VsTextBuffer buffer = window.DocumentData as VsTextBuffer;
            if (buffer == null)
            {
                IVsTextBufferProvider bufferProvider = window.DocumentData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                    buffer = lines as VsTextBuffer;
                    Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");

                    if (buffer == null)
                    {
                        return;
                    }
                }
            }

            IVsTextManager mgr = ServiceProvider.GetService(typeof(VsTextManagerClass)) as IVsTextManager;
            if (mgr == null)
            {
                return;
            }

            Region region = this.taskToRegionMap[task];

            int startLine = region.StartLine;
            int startColumn = region.StartColumn;

            int endLine = region.EndLine > 0 ? region.EndLine : region.StartLine;
            int endColumn = region.EndColumn > 0 ? region.EndColumn : region.StartColumn;

            // Data is 1-indexed but VS navigation api is 0-indexed
            endLine--;
            endColumn--;
            startLine--;
            startColumn--;

            mgr.NavigateToLineAndColumn(buffer, ref logicalView, startLine, startColumn, endLine, endColumn);
        }
    }
}
