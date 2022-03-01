﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CnSharp.VisualStudio.Extensions;
using EnvDTE;

namespace CnSharp.VisualStudio.NuPack.Extensions
{
    public class SolutionDataCache : ConcurrentDictionary<string, SolutionProperties>
    {
        private static SolutionDataCache instance;
        protected SolutionDataCache()
        {

        }

        public static SolutionDataCache Instance => instance ?? (instance = new SolutionDataCache());

        public SolutionProperties GetSolutionProperties(string solutionFile)
        {
            SolutionProperties sp;
            while (!TryGetValue(solutionFile, out sp))
            {
                System.Threading.Thread.Sleep(500);
            }
            return sp;
        }
    }

    public class SolutionProperties
    {
        private List<Project> _projects;

        public List<Project> Projects
        {
            get { return _projects; }
            set
            {
                _projects = value;
                if (_projects != null)
                {
                    //Removed
                    //ClassicProjects.AddRange(_projects?.Where(p => p.IsUAP() || p.IsNetFrameworkProject()));
                    SdkBasedProjects.AddRange(_projects?.Where(p => p.IsSdkBased()));
                }
                else
                {
                    ClassicProjects.Clear();
                    SdkBasedProjects.Clear();
                }
            }
        }

        public List<Project> ClassicProjects { get; private set; } = new List<Project>();
        public List<Project> SdkBasedProjects { get; private set; } = new List<Project>();
        public bool HasClassicProjects => ClassicProjects?.Any() == true;
        public bool HasSdkBasedProjects => SdkBasedProjects?.Any() == true;

        public void AddProject(Project project)
        {
            if (_projects == null) _projects = new List<Project>();
            _projects.Add(project);

            //removed
            //if (project.IsUAP() || project.IsNetFrameworkProject())

            if (project.IsNetFrameworkProject())
            {
                ClassicProjects.Add(project);
            }
            else if (project.IsSdkBased())
            {
                SdkBasedProjects.Add(project);
            }
        }

        public void RemoveProject(Project project)
        {
            _projects.Remove(project);
            //removed
            //   if (project.IsUAP() || project.IsNetFrameworkProject())

            if (project.IsNetFrameworkProject())
            {
                ClassicProjects.Remove(project);
            }
            else if (project.IsSdkBased())
            {
                SdkBasedProjects.Remove(project);
            }
        }
    }
}
