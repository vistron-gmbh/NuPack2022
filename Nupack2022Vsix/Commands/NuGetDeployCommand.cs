﻿using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CnSharp.VisualStudio.Extensions;
using CnSharp.VisualStudio.Extensions.Projects;
using CnSharp.VisualStudio.NuPack.Packaging;
using CnSharp.VisualStudio.NuPack.Util;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet;
using Package = Microsoft.VisualStudio.Shell.Package;

namespace CnSharp.VisualStudio.NuPack.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    public sealed class NuGetDeployCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("eadcfc27-6b6c-4ea4-bd28-13f77827468d");

        private Project _project;
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;
        private PackageProjectProperties _ppp;
        private ManifestMetadata _metadata;
        private string _nuspecFile;
        private ProjectAssemblyInfo _assemblyInfo;
        private DirectoryBuildProps _directoryBuildProps;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetDeployCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private NuGetDeployCommand(Package package, OleMenuCommandService commandService)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            //OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;      
                commandService.AddCommand(menuItem);

                this.package = package ?? throw new ArgumentNullException(nameof(package));
                commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));              
            }
        }

        private void MenuItemOnBeforeQueryStatus(object sender, EventArgs e)
        {
            var dte = Host.Instance.Dte2;
            if (dte == null) return;
            var prj = dte.GetActiveProejct();
            var cmd = (OleMenuCommand)sender;
            cmd.Visible = !string.IsNullOrWhiteSpace(prj.FileName) &&
                          Common.SupportedProjectTypes.Any(
                              t => prj.FileName.EndsWith(t, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NuGetDeployCommand Instance
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

        ///// <summary>
        ///// Initializes the singleton instance of the command.
        ///// </summary>
        ///// <param name="package">Owner package, not null.</param>
        //public static void Initialize(Package package)
        //{
        //    Instance = new NuGetDeployCommand(package);
        //}

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new NuGetDeployCommand(package, commandService);
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


            var dte = Host.Instance.Dte2;
            _project = dte.GetActiveProejct();

            //Common.CheckTfs(_project);
            _assemblyInfo = null;
            _ppp = null;
            if (_project.IsSdkBased())
            {
                _ppp = _project.GetPackageProjectProperties();
                _metadata = _ppp.ToManifestMetadata();
                _directoryBuildProps = Host.Instance.DTE.Solution.GetDirectoryBuildProps();
                var form = new MsbuildDeployWizard(_metadata, _ppp, _directoryBuildProps);
                form.StartPosition = FormStartPosition.CenterScreen;
                if (form.ShowDialog() == DialogResult.OK)
                    form.SaveAndBuild();
            }
            else
            {
                _nuspecFile = _project.GetNuSpecFilePath();
                if (!File.Exists(_nuspecFile))
                {
                    var dr = VsShellUtilities.ShowMessageBox(this.ServiceProvider,
                        $"Miss {NuGetDomain.NuSpecFileName} file,would you add it now?", "Warning",
                        OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    if (dr != 6)
                        return;
                    new AddNuSpecCommand().Execute();
                }

                _assemblyInfo = _project.GetProjectAssemblyInfo();
                if (string.IsNullOrWhiteSpace(_assemblyInfo.FileVersion))
                    _assemblyInfo.FileVersion = _assemblyInfo.Version;
                _metadata = _project.GetManifestMetadata();

                var form = new DeployWizard(_metadata, _assemblyInfo, _ppp);
                form.StartPosition = FormStartPosition.CenterScreen;
                if (form.ShowDialog() == DialogResult.OK)
                    form.SaveAndBuild();
            }
        }
    }
}
