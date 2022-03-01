using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CnSharp.VisualStudio.Extensions.Projects;
using EnvDTE;

namespace CnSharp.VisualStudio.Extensions
{
    public static class AssemblyInfoUtil
    {
        public static ProjectAssemblyInfo GetProjectAssemblyInfo(this Project project)
        {
            var pai = new ProjectAssemblyInfo(project);
            if (project.IsNetFrameworkProject())
            {
                if (string.IsNullOrEmpty(pai.Company) || string.IsNullOrEmpty(pai.Product) ||
                    string.IsNullOrEmpty(pai.Copyright))
                {
                    CommonAssemblyInfo commonInfo = null;
                    var commonInfoFileLinked = project.GetCommonAssemblyInfoFilePath();
                    if (commonInfoFileLinked != null)
                    {
                        commonInfo = ReadCommonAssemblyInfo(commonInfoFileLinked);
                        pai.Merge(commonInfo);
                        return pai;
                    }
                    //search common assembly info files
                    var slnDir = Path.GetDirectoryName(project.DTE.Solution.FullName);
                    var assemblyInfoFiles = Directory.GetFiles(slnDir, "*AssemblyInfo.*", SearchOption.AllDirectories);
                    foreach (var file in assemblyInfoFiles)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        if (string.Compare(fileName, "AssemblyInfo", StringComparison.InvariantCultureIgnoreCase) == 0)
                            continue;
                        commonInfo = ReadCommonAssemblyInfo(file);
                        pai.Merge(commonInfo);
                        return pai;
                    }
                }
            }
            return pai;
        }


        public static CommonAssemblyInfo ReadCommonAssemblyInfo(string assemblyInfoFile)
        {
            var manager = AssemblyInfoFileManagerFactory.Get(assemblyInfoFile);
            var info = manager.ReadCommonAssemblyInfo(assemblyInfoFile);
            return info;
        }



        public static void Save(this ProjectAssemblyInfo assemblyInfo,bool includeCommonInfo = false)
        {
            var assemblyInfoFile = assemblyInfo.Project.GetAssemblyInfoFileName();
            var manager = AssemblyInfoFileManagerFactory.Get(assemblyInfo.Project);
            manager.Save(assemblyInfo,assemblyInfoFile);

            if (!includeCommonInfo) return;

            var commonInfoFile = assemblyInfo.Project.GetCommonAssemblyInfoFilePath();
            if(commonInfoFile != null)
                Save(assemblyInfo,commonInfoFile);
        }

        public static void Save(this CommonAssemblyInfo assemblyInfo,string fileName)
        {
            var manager = AssemblyInfoFileManagerFactory.Get(fileName);
            manager.Save(assemblyInfo,fileName);
        }

        public static void RemoveCommonAssemblyInfoAnnotations(this Project project, params string[] skipProperties)
        {
            var assemblyInfoFile = project.GetAssemblyInfoFileName();
            var manager = AssemblyInfoFileManagerFactory.Get(project);
            manager.RemoveCommonInfo(assemblyInfoFile,skipProperties);
        }


        public static string GetAssemblyInfoFileName(this Project project)
        {
            string prjDir = Path.GetDirectoryName(project.FileName);
            string assemblyInfoFile = prjDir + "\\Properties\\AssemblyInfo.cs";
            if (!File.Exists(assemblyInfoFile))
            {
                assemblyInfoFile = prjDir + "\\AssemblyInfo.cs";
            }
            if (!File.Exists(assemblyInfoFile))
            {
                assemblyInfoFile = prjDir + "\\My Project\\AssemblyInfo.vb";
            }
            if (!File.Exists(assemblyInfoFile))
            {
                assemblyInfoFile = prjDir + "\\AssemblyInfo.vb";
            }
            if (!File.Exists(assemblyInfoFile))
            {
                throw new FileNotFoundException("AssemblyInfo file not found in this project.");
            }
            return assemblyInfoFile;
        }

    }


    public class AssemblyInfoFileManagerFactory
    {
        public static AssemblyInfoFileManager Get(Project project)
        {
            switch (project.CodeModel.Language)
            {
                case CodeModelLanguageConstants.vsCMLanguageCSharp:

                    return new AssemblyInfoCsManager();

                case CodeModelLanguageConstants.vsCMLanguageVB:

                   return new AssemblyInfoVbManager();
            }
            throw new NotSupportedException();
            //var ext = Path.GetExtension(file).TrimStart('.').ToLower();
            //return ext == "cs" ? (AssemblyInfoFileManager) new AssemblyInfoCsManager() : new AssemblyInfoVbManager();
        }

        public static AssemblyInfoFileManager Get(string file)
        {
            var ext = Path.GetExtension(file).TrimStart('.').ToLower();
            switch (ext)
            {
                case "cs":return new AssemblyInfoCsManager();
                case "vb": return new AssemblyInfoVbManager();
            }
            throw new NotSupportedException();
        }
    }

    public abstract class AssemblyInfoFileManager
    {
        protected string FindRegexPattern;
        protected string ReadRegexPattern;
        protected string WriteRegexPattern;
        protected string CommonInfoUsingTemplate;
        protected string CommonInfoAnnotationTempalte;
        public string FolderName { get;protected set; }

        public  ProjectAssemblyInfo Read(string file)
        {
            var assemblyInfo = ReadFile(file);
            if (string.IsNullOrEmpty(assemblyInfo))
                throw new FileLoadException("AssemblyInfo file content is empty.");
            assemblyInfo = Regex.Replace(assemblyInfo, "['|//].*", "");
            string fileVersion = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyFileVersion");

            string version = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyVersion");
            string productName = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyProduct");
            string companyName = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyCompany");
            string title = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyTitle");
            string description = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyDescription");
            string copyright = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyCopyright");
            return new ProjectAssemblyInfo
            {
                FileVersion = fileVersion,
                Version = version,
                Product = productName,
                Company = companyName,
                Title = title,
                Description = description,
                Copyright = copyright
            };
        }


        public CommonAssemblyInfo ReadCommonAssemblyInfo(string file)
        {
            var assemblyInfo = ReadFile(file);
            if (string.IsNullOrEmpty(assemblyInfo))
                throw new FileLoadException("AssemblyInfo file content is empty.");
            assemblyInfo = Regex.Replace(assemblyInfo, "['|//].*", "");
            var version = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyVersion");
            var productName = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyProduct");
            var companyName = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyCompany");
            var copyright = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyCopyright");
            var trademark = GetAssemblyAnnotationValue(assemblyInfo, "AssemblyTrademark");
            return new CommonAssemblyInfo
            {
                Version = version,
                Product = productName,
                Company = companyName,
                Copyright = copyright,
                Trademark = trademark
            };
        }

        protected string ReadFile(string file)
        {
            using (var sr = new StreamReader(file, Encoding.Default))
            {
                return sr.ReadToEnd();
            }
        }


        protected  string GetAssemblyAnnotationValue(string assemblyInfo, string attributeName)
        {
            return
                Regex.Match(assemblyInfo, string.Format(ReadRegexPattern, attributeName)).Groups["content"].Value;
        }

        public virtual void Save(ProjectAssemblyInfo assemblyInfo, string file)
        {
            var assemblyText = ReadFile(file);

            var sc = Host.Instance.SourceControl;
            sc?.CheckOut(Path.GetDirectoryName(Host.Instance.DTE.Solution.FullName), file);

            using (var sw = new StreamWriter(file, false, Encoding.Unicode))
            {
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyFileVersion", assemblyInfo.FileVersion);
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyVersion", assemblyInfo.Version);
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyProduct", assemblyInfo.Product);
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyCompany", assemblyInfo.Company);
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyTitle", assemblyInfo.Title);
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyDescription", assemblyInfo.Description);
                assemblyText = ReplaceAssemblyAnnotation(assemblyText, "AssemblyCopyright", assemblyInfo.Copyright);
                sw.Write(assemblyText);
            }
        }

        public virtual void Save(CommonAssemblyInfo assemblyInfo, string file)
        {
            if (File.Exists(file))
            {
                var sc = Host.Instance.SourceControl;
                sc?.CheckOut(Path.GetDirectoryName(Host.Instance.DTE.Solution.FullName), file);
            }

            using (var sw = new StreamWriter(file, false, Encoding.Unicode))
            {
                var sb = new StringBuilder(CommonInfoUsingTemplate);
                sb.AppendLine();
                var props = typeof(CommonAssemblyInfo).GetProperties().ToList();
                props.ForEach(p =>
                    {
                        var v = p.GetValue(assemblyInfo, null)?.ToString();
                        if (!string.IsNullOrWhiteSpace(v))
                        {
                            sb.AppendFormat(CommonInfoAnnotationTempalte, p.Name, v);
                            sb.AppendLine();
                        }
                    }
                );
                sw.Write(sb.ToString());
            }
        }

        public void RemoveCommonInfo(string file,params string[] skipProperties)
        {
            var assemblyText = ReadFile(file);
            var sc = Host.Instance.SourceControl;
            sc?.CheckOut(Path.GetDirectoryName(Host.Instance.DTE.Solution.FullName), file);
            using (var sw = new StreamWriter(file, false, Encoding.Unicode))
            {
                var props = typeof(CommonAssemblyInfo).GetProperties().Where(p => skipProperties == null || !skipProperties.Contains(p.Name)).Select(p => p.Name).ToList();
                props.ForEach(p => assemblyText = RemoveAssemblyAnnotation(assemblyText,"Assembly"+ p));
                sw.Write(assemblyText);
            }
        }

       protected string ReplaceAssemblyAnnotation(string assemblyText, string attributeName, string value)
        {
            //var text = Regex.Replace(assemblyText, $"[^/]\\[assembly:\\s*?{attributeName}\\(\".*?\"\\)\\]",
            //    $"[assembly: {attributeName}(\"{value}\")]\n");
            //return text.Replace("\r[", "\r\n[");//这里有个坑

            var text = assemblyText;
            var matches = Regex.Matches(text, string.Format(WriteRegexPattern,attributeName));
            foreach (Match m in matches)
            {
                if (m.Success)
                {
                    var newValue = Regex.Replace(m.Value, "\\(\".*?\"\\)", $"(\"{value}\")");
                    text = text.Replace(m.Value, newValue);
                }
            }
            return text;
        }

        protected string RemoveAssemblyAnnotation(string assemblyText, string attributeName)
        {
            return Regex.Replace(assemblyText, string.Format(FindRegexPattern, attributeName), string.Empty);
        }
    }

    public class AssemblyInfoCsManager : AssemblyInfoFileManager
    {
        public AssemblyInfoCsManager()
        {
            FindRegexPattern = "\\[assembly:\\s*?{0}\\(\".*?\"\\)\\]";
            ReadRegexPattern = "[^/]\\[assembly:\\s*?{0}\\(\"(?<content>.+)\"\\)";
            WriteRegexPattern = "[^/]\\[assembly:\\s*?{0}\\(\".*?\"\\)\\]";
            FolderName = "Properties";
            CommonInfoUsingTemplate = @"using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;";
            CommonInfoAnnotationTempalte = "[assembly: Assembly{0}(\"{1}\")]";
        }
    }

    public class AssemblyInfoVbManager : AssemblyInfoFileManager
    {
        public AssemblyInfoVbManager()
        {
            FindRegexPattern = "\\<Assembly:\\s*?{0}\\(\".*?\"\\)\\>";
            ReadRegexPattern = "[^`]\\<Assembly:\\s*?{0}\\(\"(?<content>.+)\"\\)";
            WriteRegexPattern = "[^`]\\<Assembly:\\s*?{0}\\(\".*?\"\\)\\>";
            FolderName = "My Project";
            CommonInfoUsingTemplate = @"Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices";
            CommonInfoAnnotationTempalte = "<Assembly: Assembly{0}(\"{1}\")>";
        }
    }
}