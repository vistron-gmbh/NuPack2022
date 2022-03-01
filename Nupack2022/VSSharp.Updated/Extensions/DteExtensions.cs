using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;

namespace CnSharp.VisualStudio.Extensions
{
    public static class DteExtensions
    {
        public static Project GetActiveProejct(this _DTE dte)
        {
            Array activeSolutionProjects = (Array)dte.ActiveSolutionProjects;
            if ((activeSolutionProjects != null) && (activeSolutionProjects.Length >= 1))
            {
                return (activeSolutionProjects.GetValue(0) as Project);
            }
            return null;
        }

        public static IEnumerable<Project> GetSolutionProjects(this _DTE dte)
        {
            List<Project> list = new List<Project>();
            foreach (object obj2 in dte.Solution.Projects)
            {
                Project project = obj2 as Project;
                list.AddRange(project.GetValidProjects());
            }
            return list;
        }

        public static Project GetStartupProject(this _DTE dte)
        {
            Array startupProjects = (Array)dte.Solution.SolutionBuild.StartupProjects;
            if ((startupProjects != null) && (startupProjects.Length >= 1))
            {
                return (startupProjects.GetValue(0) as Project);
            }
            return null;
        }

        private static IEnumerable<Project> GetValidProjects(this Project project)
        {
            List<Project> list = new List<Project>();
            string fileName = string.Empty;
            try
            {
                fileName = project.FileName;
            }
            catch
            {
            }
            if ((project != null) && !string.IsNullOrEmpty(fileName))
            {
                list.Add(project);
            }
            if ((project != null) && (project.ProjectItems != null))
            {
                foreach (object obj2 in project.ProjectItems)
                {
                    ProjectItem item = obj2 as ProjectItem;
                    list.AddRange(item.SubProject.GetValidProjects());
                }
            }
            return list;
        }


        public static void OutputMessage(this DTE2 dte,string commandName, string message)
        {
            //get output window
            OutputWindow ow = dte.ToolWindows.OutputWindow;
            //create own pane type
            OutputWindowPane outputPane = null;
            foreach (OutputWindowPane pane in ow.OutputWindowPanes)
            {
                if (pane.Name == commandName)
                {
                    outputPane = pane;
                    break;
                }
            }
            if (outputPane == null)
                outputPane = ow.OutputWindowPanes.Add(commandName);
            //output message
            outputPane.OutputString(message);
            outputPane.Activate();
        }

     
    }
}