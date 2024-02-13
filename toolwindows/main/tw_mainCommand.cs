using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Debricked
{
    /// Command handler
    internal sealed class tw_mainCommand
    {
        /// Command ID.
        public const int CommandId = 0x0100;

        /// Command menu group (command set GUID).
        public static readonly Guid CommandSet = new Guid("c5539889-ee8c-49a9-85a6-1a852b942f9e");

        /// VS Package that provides this command, not null.
        private readonly AsyncPackage package;

        /// Initializes a new instance of the <see cref="tw_mainCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private tw_mainCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// Gets the instance of the command.
        public static tw_mainCommand Instance
        {
            get;
            private set;
        }

        /// Gets the service provider from the owner package.
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// Initializes the singleton instance of the command.
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in tw_mainCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new tw_mainCommand(package, commandService);
        }

        /// Shows the tool window when the menu item is clicked.
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            _=this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                ToolWindowPane window = await this.package.ShowToolWindowAsync(typeof(tw_main), 0, true, this.package.DisposalToken);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            });
        }
    }
}
