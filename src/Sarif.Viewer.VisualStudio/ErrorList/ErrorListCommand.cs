using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.Sarif.Viewer.ErrorList
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ErrorListCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int ClearSarifResultsCommandId = 0x0300;
        public const int ReportFalsePositiveCommandId = 0x0301;

        private static int[] s_commands = new int[]
        {
            ClearSarifResultsCommandId,
            ReportFalsePositiveCommandId
        };

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("76648814-13bf-4ecf-ad5c-2a7e2953e62f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorListCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ErrorListCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                foreach (int commandId in s_commands)
                {
                    var menuCommandId = new CommandID(CommandSet, commandId);
                    var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandId);
                    menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                    commandService.AddCommand(menuItem);
                }
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ErrorListCommand Instance
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
            Instance = new ErrorListCommand(package);
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
            MenuCommand menuCommand = sender as MenuCommand;

            if (menuCommand != null)
            {
                switch (menuCommand.CommandID.ID)
                {
                    case ClearSarifResultsCommandId:
                        SarifTableDataSource.Instance.CleanAllErrors();
                        CodeAnalysisResultManager.Instance.SarifErrors.Clear();
                        CodeAnalysisResultManager.Instance.FileDetails.Clear();
                        break;
                    case ReportFalsePositiveCommandId:
                        // TODO: Submit the report
                        break;
                }
            }
        }

        /// <summary>
        /// Handler for the menu items' BeforeQueryStatus event, which fires before the error list context menu is displayed.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;

            if (menuCommand != null)
            {
                switch (menuCommand.CommandID.ID)
                {
                    case ClearSarifResultsCommandId:
                        menuCommand.Enabled = SarifTableDataSource.Instance.HasErrors();
                        break;
                    case ReportFalsePositiveCommandId:
                        menuCommand.Enabled = IsSingleSarifErrorListItemSelected();
                        break;
                }
            }
        }

        /// <summary>
        /// Indicates whether the error list selection is a single SARIF result.
        /// </summary>
        /// <returns>true if a single SARIF error list result is selected; otherwise, false.</returns>
        private static bool IsSingleSarifErrorListItemSelected()
        {
            IErrorList errorList = SarifViewerPackage.Dte.ToolWindows.ErrorList as IErrorList;
            IEnumerable<ITableEntryHandle> selectedItems = errorList.TableControl.SelectedEntries;

            // If the user has right-clicked with a single item selected...
            if (selectedItems != null && selectedItems.Count() == 1)
            {
                ITableEntryHandle selectedItem = selectedItems.First();
                ITableEntriesSnapshot snapshot;
                int index;

                // ...and it's a SARIF result
                if (selectedItem.TryGetSnapshot(out snapshot, out index) && snapshot is SarifSnapshot)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
