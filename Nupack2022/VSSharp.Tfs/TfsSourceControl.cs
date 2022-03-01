using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CnSharp.VisualStudio.Extensions.SourceControl;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace CnSharp.VisualStudio.SourceControl.Tfs
{
    public class TfsSourceControl : ISourceControl
    {
        private static readonly Hashtable Table = Hashtable.Synchronized(new Hashtable());

        public int CheckOut(string projectDir,string file)
        {
            var workspace = GetWorkspace(projectDir);
            if(workspace == null)
                return -1;
            return workspace.PendEdit(file);
        }

        public IEnumerable<string> GetPendingFiles(string projectDir)
        {
            var workspace = GetWorkspace(projectDir);
            if (workspace == null)
                return null;
            return workspace.GetPendingChangesEnumerable().Select(m => m.FileName);
        } 

        public static Workspace GetWorkspace(string projectDir)
        {
            if (Table.Contains(projectDir))
            {
                return Table[projectDir] as Workspace;
            }
       
            var projectCollections = new List<RegisteredProjectCollection>((RegisteredTfsConnections.GetProjectCollections()));
            var onlineCollections = projectCollections.Where(c => !c.Offline).ToList();

            // fail if there are no registered collections that are currently on-line
            if (!onlineCollections.Any())
            {
                Table.Add(projectDir,null);
                return null;
            }
            Workspace workspace = null;
            // find a project collection with at least one team project
            foreach (var registeredProjectCollection in onlineCollections)
            {
                var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(registeredProjectCollection);
                projectCollection.EnsureAuthenticated();
                var versionControl = (VersionControlServer)projectCollection.GetService(typeof(VersionControlServer));

                var teamProjects = new List<TeamProject>(versionControl.GetAllTeamProjects(false));


                // if there are no team projects in this collection, skip it
                if (teamProjects.Count < 1) continue;

         
                try
                {
                    Workspace[] workspaces = versionControl.QueryWorkspaces(null, Environment.UserName,
                        Environment.MachineName);
                    foreach (var ws in workspaces)
                    {
                        foreach (var folder in ws.Folders)
                        {
                            if (projectDir.StartsWith(folder.LocalItem))
                            {
                                workspace = ws;
                                break;
                            }
                        }
                        if (workspace != null && workspace.HasUsePermission)
                            break;
                    }
                }
                catch
                {
                    continue;
                }
              

       #region another implementation
                //var dir = new DirectoryInfo(projectDir);
                //while (workspace == null)
                //{
                //    workspace = versionControl.TryGetWorkspace(dir.FullName);
                //    if (dir.Parent == null)
                //        break;
                //    dir = dir.Parent;
                //}
       #endregion
                if (workspace != null && workspace.HasUsePermission)
                    break;
            }
            Table.Add(projectDir, workspace);
            return workspace;
        }
    }
}
