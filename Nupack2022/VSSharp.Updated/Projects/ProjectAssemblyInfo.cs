using System;
using System.Xml.Serialization;
using EnvDTE;

namespace CnSharp.VisualStudio.Extensions.Projects
{
    [Serializable]
    public class ProjectAssemblyInfo : CommonAssemblyInfo, IComparable<ProjectAssemblyInfo>
    {
        public ProjectAssemblyInfo()
        {
            
        }

        public ProjectAssemblyInfo(Project project)
        {
            Project = project;
        }

        private Project _project;

        [XmlIgnore]
        public Project Project
        {
            get { return _project; }
            set
            {
                _project = value;
                Title = _project.GetPropertyValue("Title");
                Company = _project.GetPropertyValue("Company");
                Description = _project.GetPropertyValue("Description");
                Copyright = _project.GetPropertyValue("Copyright");
                Version = _project.GetPropertyValue("AssemblyVersion");
                FileVersion = _project.GetPropertyValue("AssemblyFileVersion");
                Product = _project.GetPropertyValue("Product");
            }
        }

        public string ProjectName => Project == null ? string.Empty : Project.Name;

        public string FileVersion { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int CompareTo(ProjectAssemblyInfo other)
        {
            var result = base.CompareTo(other);
            if (result != 0) return result;
            if (String.CompareOrdinal(FileVersion, other.FileVersion) != 0)
                return -1;
            if (String.CompareOrdinal(Title, other.Title) != 0)
                return -1;
            if (String.CompareOrdinal(Description, other.Description) != 0)
                return -1;
            return 0;
        }

        public ProjectAssemblyInfo Copy()
        {
            return new ProjectAssemblyInfo
            {
                Project = Project,
                Company = Company,
                Copyright = Copyright,
                FileVersion = FileVersion,
                Product = ProjectName,
                Title = Title,
                Version = Version
            };
        }

        public void Merge(CommonAssemblyInfo other)
        {
            if (!string.IsNullOrEmpty(other.Company) && string.IsNullOrEmpty(Company))
                Company = other.Company;
            if (!string.IsNullOrEmpty(other.Product) && string.IsNullOrEmpty(Product))
                Product = other.Product;
            if (!string.IsNullOrEmpty(other.Copyright) && string.IsNullOrEmpty(Copyright))
                Copyright = other.Copyright;
            if (!string.IsNullOrEmpty(other.Trademark) && string.IsNullOrEmpty(Trademark))
                Trademark = other.Trademark;
            if (!string.IsNullOrEmpty(other.Version) && string.IsNullOrEmpty(Version))
                Version = other.Version;
        }
    }
}