using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CnSharp.VisualStudio.Extensions.Projects;
using CnSharp.VisualStudio.Extensions.Util;
using EnvDTE;
using NuGet;

namespace CnSharp.VisualStudio.Extensions
{
    public static class NuGetExtensions
    {
        public static string GetNuSpecFilePath(this Project project)
        {
            return Path.Combine(project.GetDirectory(), NuGetDomain.NuSpecFileName);
        }

        public static ManifestMetadata GetManifestMetadata(this Project project)
        {
            if (project.IsNetFrameworkProject())
            {
                var ai = project.GetProjectAssemblyInfo();
                //from nuspec
                var nuspecFile = project.GetNuSpecFilePath();
                if (!File.Exists(nuspecFile))
                    return ai.ToManifestMetadata();
                return LoadFromNuspecFile(nuspecFile).CopyFromAssemblyInfo(ai);
            }
            return project.GetPackageProjectProperties().ToManifestMetadata();
        }

        public static ManifestMetadata LoadFromNuspecFile(string file)
        {
            var metadata = new ManifestMetadata();
            var doc = XDocument.Load(file);
            var elements = doc.Element("package").Element("metadata").Elements();
            var props = typeof(ManifestMetadata).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).ToList();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(string))
                {
                    var v = elements.FirstOrDefault(m => m.Name.LocalName.Equals(prop.Name,StringComparison.InvariantCultureIgnoreCase))?.Value;
                    if (v != null)
                    {
                        prop.SetValue(metadata, v, null);
                    }
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    var v = elements.FirstOrDefault(m => m.Name.LocalName.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase))?.Value;
                    if (v != null)
                    {
                        prop.SetValue(metadata, v.Equals("true",StringComparison.InvariantCultureIgnoreCase), null);
                    }
                }
            }
           
            return metadata;
        }

        public static void UpdateNuspec(this Project project, ManifestMetadata metadata)
        {
            var nuspecFile = project.GetNuSpecFilePath();
            if (!File.Exists(nuspecFile))
            {
                return;
            }
            else
            {
                metadata.SaveToNuSpec(nuspecFile);
            }
        }

        public static void SaveToNuSpec(this ManifestMetadata metadata,string nuspecFile)
        {
            var doc = new XmlDocument();
            doc.Load(nuspecFile);
            metadata.SyncToXmlDocument(doc);
            var sc = Host.Instance.SourceControl;
            sc?.CheckOut(Path.GetDirectoryName(Host.Instance.DTE.Solution.FullName), nuspecFile);
            doc.Save(nuspecFile);
        }

        public static void SyncToXmlDocument(this  ManifestMetadata metadata, XmlDocument doc)
        {
            var metadataNode = doc.SelectSingleNode("package/metadata");
            if (metadataNode == null)
                return;
            var props = typeof(ManifestMetadata).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).ToList();
            props.ForEach(p =>
            {
                var val = p.GetValue(metadata, null);
                if (val == null) return;
                var text = p.PropertyType == typeof(bool)
                    ? val.ToString().ToLower()
                    : val.ToString();
                metadataNode.SetXmlNode(p.Name.Substring(0,1).ToLower()+p.Name.Substring(1),text);
            });
            UpdateDependencies( metadata,doc);
        }

        private static void SetXmlNode(this XmlNode metadataNode, string key, string value)
        {
            var idNode = metadataNode.SelectSingleNode(key);
            if (idNode != null)
                idNode.InnerText = value == null ? string.Empty : value;
        }

        public static void UpdateDependencies(this ManifestMetadata metadata, XmlDocument doc)
        {
            var metadataNode = doc.SelectSingleNode("package/metadata");
            if (metadataNode == null)
                return;

            if (metadata.DependencySets != null)
            {
                var depNode = metadataNode.SelectSingleNode("dependencies");
                if (depNode == null)
                {
                    var node = doc.CreateElement("dependencies");
                    metadataNode.AppendChild(node);
                    depNode = node;
                }

                depNode.RemoveAll();
                var tempNode = doc.CreateElement("temp");
                tempNode.InnerXml = XmlSerializerHelper.GetXmlStringFromObject(metadata.DependencySets);
                depNode.InnerXml = tempNode.ChildNodes[0].InnerXml;
            }
        }



        public static bool IsEmptyOrPlaceHolder(this string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.StartsWith("$");
        }

        public static ManifestMetadata ToManifestMetadata(this ProjectAssemblyInfo pai)
        {
            var metadata = new ManifestMetadata
            {
                Id = pai.Title,
                Owners = pai.Company,
                Title = pai.Title,
                Description = pai.Description,
                Authors = pai.Company,
                Copyright = pai.Copyright
            };
            return metadata;
        }

        public static ManifestMetadata CopyFromAssemblyInfo(this ManifestMetadata metadata,ProjectAssemblyInfo pai)
        {
            if (metadata.Id.IsEmptyOrPlaceHolder())
                metadata.Id = pai.Title;
            if (metadata.Title.IsEmptyOrPlaceHolder())
                metadata.Title = pai.Title;
            if (metadata.Owners.IsEmptyOrPlaceHolder())
                metadata.Owners = pai.Company;
            if (metadata.Description.IsEmptyOrPlaceHolder())
                metadata.Description = pai.Description;
            if (metadata.Authors.IsEmptyOrPlaceHolder())
                metadata.Authors = pai.Company;
            if (metadata.Copyright.IsEmptyOrPlaceHolder())
                metadata.Copyright = pai.Copyright;
            return metadata;
        }

        public static ManifestMetadata ToManifestMetadata(this PackageProjectProperties ppp)
        {
            return new ManifestMetadata
            {
                Id = ppp.PackageId,
                Authors = ppp.Authors,
                Copyright = ppp.Copyright,
                Owners = ppp.Company,
                Description = ppp.Description,
                IconUrl = ppp.PackageIconUrl,
                Language = ppp.NeutralLanguage,
                LicenseUrl = ppp.PackageLicenseUrl,
                ReleaseNotes = ppp.PackageReleaseNotes,
                RequireLicenseAcceptance = ppp.PackageRequireLicenseAcceptance,
                ProjectUrl = ppp.PackageProjectUrl,
                Tags = ppp.PackageTags,
                Version = ppp.Version
            };
        }

        public static void SyncToPackageProjectProperties(this ManifestMetadata metadata, PackageProjectProperties ppp)
        {
            ppp.PackageId = metadata.Id;
            ppp.Authors = metadata.Authors;
            ppp.Copyright = metadata.Copyright;
            ppp.Company = metadata.Owners;
            ppp.Description = metadata.Description;
            ppp.PackageIconUrl = metadata.IconUrl;
            ppp.NeutralLanguage = metadata.Language;
            ppp.PackageLicenseUrl = metadata.LicenseUrl;
            ppp.PackageReleaseNotes = metadata.ReleaseNotes;
            ppp.PackageRequireLicenseAcceptance = metadata.RequireLicenseAcceptance;
            ppp.PackageProjectUrl = metadata.ProjectUrl;
            ppp.PackageTags = metadata.Tags;
            ppp.Version = metadata.Version;
        }


        public static List<string> UpdateDependencyInSolution(string packageId, string newVersion)
        {
            var nuspecFiles = new List<string>();
            var projects = Host.Instance.DTE.GetSolutionProjects().ToList();
            projects.ForEach(p =>
            {
                var nuspecFile = p.GetNuSpecFilePath();
                if (File.Exists(nuspecFile))
                {
                    var metadata = LoadFromNuspecFile(nuspecFile);

                    if (metadata.DependencySets != null)
                    {
                        var found = false;
                        foreach (var g in metadata.DependencySets)
                        {
                            foreach (var d in g.Dependencies)
                            {
                                if (d.Id == packageId)
                                {
                                    d.Version = newVersion;
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            metadata.SaveToNuSpec(nuspecFile);
                            nuspecFiles.Add(nuspecFile);
                        }
                    }
                }
            });
            return nuspecFiles;
        }
    }

    public class NuGetDomain
    {
        public const string NuSpecFileName = "package.nuspec";
    }

}
