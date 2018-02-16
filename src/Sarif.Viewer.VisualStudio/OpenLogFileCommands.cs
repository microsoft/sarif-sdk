// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.Sarif.Viewer.ErrorList;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Sarif.Viewer
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
        public const int OpenStaticDriverVerifierFileCommandId = 0x0102;
        public const int OpenFxCopFileCommandId = 0x0103;
        public const int OpenFortifyFileCommandId = 0x0104;
        public const int OpenCppCheckFileCommandId = 0x0105;
        public const int OpenClangFileCommandId = 0x0106;
        public const int OpenAndroidStudioFileCommandId = 0x0107;
        public const int OpenSemmleFileCommandId = 0x0108;
        public const int OpenTSLintFileCommand = 0x0109;
        public const int OpenPylintFileCommand = 0x010A;

        private static int[] s_commands = new int[]
        {
            OpenSarifFileCommandId,
            OpenPREfastFileCommandId,
            OpenStaticDriverVerifierFileCommandId,
            OpenFxCopFileCommandId,
            OpenFortifyFileCommandId,
            OpenCppCheckFileCommandId,
            OpenClangFileCommandId,
            OpenAndroidStudioFileCommandId,
            OpenSemmleFileCommandId,
            OpenTSLintFileCommand,
            OpenPylintFileCommand
        };

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a236a757-af66-4cf0-a3c8-facbb61d5cf1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

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

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                foreach (int command in s_commands)
                {
                    OleMenuCommand oleCommand = new OleMenuCommand(
                            this.MenuItemCallback,
                            new CommandID(CommandSet, command));
                    oleCommand.ParametersDescription = "$";

                    commandService.AddCommand(oleCommand);
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
                return this.package;
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
            OleMenuCommand menuCommand = (OleMenuCommand)sender;
            OleMenuCmdEventArgs menuCmdEventArgs = (OleMenuCmdEventArgs)e;

            string inputFile = menuCmdEventArgs.InValue as String;
            string logFile = null;

            if (!String.IsNullOrWhiteSpace(inputFile))
            {
                // If the input file is a URL, download the file.
                if (Uri.IsWellFormedUriString(inputFile, UriKind.Absolute))
                {
                    TryDownloadFile(inputFile, out logFile);
                }
                else
                {
                    // Verify if the input file is valid. i.e. it exists and has a valid file extension.
                    string logFileExtension = Path.GetExtension(inputFile);

                    // Since we don't have a tool format, only accept *.sarif and *.json files as command input files.
                    if (logFileExtension.Equals(".sarif", StringComparison.OrdinalIgnoreCase) || logFileExtension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(inputFile))
                        {
                            logFile = inputFile;
                        }
                    }
                }
            }

            string toolFormat = ToolFormat.None;

            if (logFile == null)
            {
                string title = "Open Static Analysis Results Interchange Format (SARIF) file";
                string filter = "SARIF files (*.sarif)|*.sarif";

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
                        toolFormat = ToolFormat.PREfast;
                        title = "Open PREfast XML log file";
                        filter = "PREfast log files (*.xml)|*.xml";
                        break;
                    }
                    case OpenStaticDriverVerifierFileCommandId:
                    {
                        toolFormat = ToolFormat.StaticDriverVerifier;
                        title = "Open Static Driver Verifier trace log file";
                        filter = "Static Driver Verifier log files (*.tt)|*.tt";
                        break;
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
                        filter = "Android Studio log files (*.xml)|*.xml";
                        break;
                    }
                    case OpenSemmleFileCommandId:
                    {
                        toolFormat = ToolFormat.SemmleQL;
                        title = "Open Semmle QL CSV log file";
                        filter = "Semmle QL log files (*.csv)|*.csv";
                        break;
                    }
                    case OpenPylintFileCommand:
                    {
                        toolFormat = ToolFormat.Pylint;
                        title = "Open Pylint JSON log file";
                        filter = "Pylint log files (*.json)|*.json";
                        break;
                    }
                    case OpenTSLintFileCommand:
                    {
                        toolFormat = ToolFormat.TSLint;
                        title = "Open TSLint JSON log file";
                        filter = "TSLint log files (*.json)|*.json";
                        break;
                    }
                }

                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Title = title;
                openFileDialog.Filter = filter;
                openFileDialog.RestoreDirectory = true;

                if (!String.IsNullOrWhiteSpace(inputFile))
                {
                    openFileDialog.FileName = Path.GetFileName(inputFile);
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(inputFile);
                }

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                logFile = openFileDialog.FileName;
            }

            TelemetryProvider.WriteMenuCommandEvent(toolFormat);

            try
            {
                ErrorListService.ProcessLogFile(logFile, SarifViewerPackage.Dte.Solution, toolFormat);
            }
            catch (InvalidOperationException)
            {
                VsShellUtilities.ShowMessageBox(SarifViewerPackage.ServiceProvider,
                                                string.Format(Resources.LogOpenFail_InvalidFormat_DialogMessage, Path.GetFileName(logFile)),
                                                null, // title
                                                OLEMSGICON.OLEMSGICON_CRITICAL,
                                                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        bool IsSarifProtocol(string path)
        {
            return path.StartsWith("sarif://", StringComparison.OrdinalIgnoreCase);
        }

        string ConvertSarifProtocol(string inputUrl)
        {
            int sarifProtocolLength;
            string replacementProtocol;

            // sarif:/// ==> file://
            // sarif://  ==> http://
            if (inputUrl.StartsWith("sarif:///", StringComparison.OrdinalIgnoreCase))
            {
                sarifProtocolLength = 9;
                replacementProtocol = "file://";
            }
            else if (inputUrl.StartsWith("sarif://", StringComparison.OrdinalIgnoreCase))
            {
                sarifProtocolLength = 8;
                replacementProtocol = "http://";
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(inputUrl), $"The input URL does not use a known protocol. {inputUrl}");
            }

            string newUrl = inputUrl.Substring(sarifProtocolLength);
            newUrl = replacementProtocol + newUrl;
            return newUrl;
        }

        bool TryDownloadFile(string inputUrl, out string downloadedFilePath)
        {
            Uri inputUri = new Uri(inputUrl, UriKind.Absolute);
            downloadedFilePath = Path.GetTempFileName();
            string downloadUrl = null;

            if (inputUri.Scheme.Equals("sarif", StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = ConvertSarifProtocol(inputUrl);
            }
            else if (inputUri.Scheme.Equals("http://", StringComparison.OrdinalIgnoreCase) || inputUri.Scheme.Equals("file://", StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = inputUrl;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(inputUrl), $"The input URL does not use a known protocol. {inputUrl}");
            }

            if (downloadUrl != null)
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.UseDefaultCredentials = true;
                        webClient.DownloadFile(downloadUrl, downloadedFilePath);
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    File.Delete(downloadedFilePath);
                    downloadedFilePath = null;
                }
            }

            return File.Exists(downloadedFilePath);
        }
    }
}
