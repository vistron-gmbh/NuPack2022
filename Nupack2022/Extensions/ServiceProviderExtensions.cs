﻿using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CnSharp.VisualStudio.NuPack.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void ShowError(this IServiceProvider serviceProvider, string message, string title)
        {
            VsShellUtilities.ShowMessageBox(
                serviceProvider,
                message, title,
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}