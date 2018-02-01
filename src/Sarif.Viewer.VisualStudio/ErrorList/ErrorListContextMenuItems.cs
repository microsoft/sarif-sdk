using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.Sarif.Viewer.ErrorList
{
    internal sealed class ErrorListContextMenuItems
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int ClearSarifResultsCommandId = 0x0100;

        private static int[] s_commands = new int[]
        {
            ClearSarifResultsCommandId
        };

        private static Lazy<ErrorListContextMenuItems> _instance = null;

        private ErrorListContextMenuItems(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            this.package = package;
        }

        public static ErrorListContextMenuItems Instance
        {
            get { return _instance.Value; }
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
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            if (_instance == null)
            {
                _instance = new Lazy<ErrorListContextMenuItems>(() => new ErrorListContextMenuItems(package));
            }
        }

        /// <summary>
        /// The callback used to execute the command when a menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
        }
    }
}
