using System.IO;
using System.Reflection;
using EnvDTE;

namespace CnSharp.VisualStudio.Extensions.SourceControl
{
    public class SourceControlManager
    {
        private static readonly string[] SupportTypes = { "CnSharp.VisualStudio.SourceControl.Tfs.TfsSourceControl,CnSharp.VisualStudio.SourceControl.Tfs" };

        public static ISourceControl GetSolutionSourceControl(Solution solution)
        {
            if (solution != null)
            {
                string fileName = solution.FileName;
                if (File.Exists(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".vssscc")))
                {
                    return (LoadInstance(SupportTypes[0]) as ISourceControl);
                }
                //todo:more SCM support
            }
            return null;
        }

        private static object LoadInstance(string typeName)
        {
            if (!typeName.Contains(","))
            {
                return typeof(SourceControlManager).Assembly.CreateInstance(typeName);
            }
            string[] strArray = typeName.Split(new[] { ',' });
            if (strArray.Length < 2)
            {
                return null;
            }
            string assemblyString = strArray[1];
            return Assembly.Load(assemblyString).CreateInstance(strArray[0]);
        }
    }
}
