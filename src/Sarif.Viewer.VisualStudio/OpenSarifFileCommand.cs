﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Forms;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.VisualStudio.Shell;

using Newtonsoft.Json;

namespace SarifViewer
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenLogFileCommands
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int OpenSarifFileCommandId = 0x0100;
        public const int OpenPREfastFileCommandId = 0x0101;
        public const int OpenFxCopFileCommandId = 0x0102;
        public const int OpenFortifyFileCommandId = 0x0103;
        public const int OpenCppCheckFileCommandId = 0x0104;
        public const int OpenClangFileCommandId = 0x0105;
        public const int OpenAndroidStudioFileCommandId = 0x0106;

        private static int[] s_commands = new int[]
        {
            OpenSarifFileCommandId, OpenPREfastFileCommandId, OpenFxCopFileCommandId, OpenFortifyFileCommandId,
            OpenCppCheckFileCommandId, OpenClangFileCommandId, OpenAndroidStudioFileCommandId
        };

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a236a757-af66-4cf0-a3c8-facbb61d5cf1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenLogFileCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenLogFileCommands(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                foreach (int command in s_commands)
                {
                    commandService.AddCommand(
                        new MenuCommand(
                            this.MenuItemCallback,
                            new CommandID(CommandSet, command))
                        );
                }
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenLogFileCommands Instance
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
                return _package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new OpenLogFileCommands(package);
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
            MenuCommand menuCommand = (MenuCommand)sender;
            ToolFormat toolFormat = ToolFormat.None;
            string title = "Open Static Analysis Results Interchange Format (SARIF) file";
            string filter = "SARIF files (*.sarif;*.sarif.json)|*.sarif;*.sarif.json";

            switch (menuCommand.CommandID.ID)
            {
                // These constants expressed in our VSCT
                case OpenSarifFileCommandId:
                    {
                        // Native SARIF. All our defaults above are fine
                        break;
                    }
                case OpenPREfastFileCommandId:
                    {
                        // PREfast. TODO. We don't have a distinct converter yet
                        // for this tool, only the native compiler support
                        toolFormat = ToolFormat.PREfast;
                        title = "Open PREfast XML log file";
                        filter = "PREfast log files (*.xml)|*.xml";
                        throw new NotImplementedException();
                    }
                case OpenFxCopFileCommandId:
                    {
                        // FxCop. TODO. We need project file support. FxCop
                        // fullMessages look broken.
                        toolFormat = ToolFormat.FxCop;
                        title = "Open FxCop XML log file";
                        filter = "FxCop report and project files (*.xml)|*.xml";
                        break;
                    }
                case OpenCppCheckFileCommandId:
                    {
                        toolFormat = ToolFormat.CppCheck;
                        title = "Open CppCheck XML log file";
                        filter = "CppCheck log files (*.xml)|*.xml";
                        break;
                    }
                case OpenClangFileCommandId:
                    {
                        toolFormat = ToolFormat.ClangAnalyzer;
                        title = "Open Clang XML log file";
                        filter = "Clang log files (*.xml)|*.xml";
                        break;
                    }
                case OpenAndroidStudioFileCommandId:
                    {
                        toolFormat = ToolFormat.AndroidStudio;
                        title = "Open Android Studio XML log file";
                        filter = "PREfast log files (*.xml)|*.xml";
                        break;
                    }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = title;
            openFileDialog.Filter = filter;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ErrorListService.ProcessLogFile(openFileDialog.FileName, toolFormat);
        }
    }
}
