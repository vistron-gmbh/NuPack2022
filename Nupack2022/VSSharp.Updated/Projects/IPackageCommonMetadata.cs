namespace CnSharp.VisualStudio.Extensions.Projects
{
    public interface IPackageCommonMetadata
    {
         string Version { get; set; }
         string Copyright { get; set; }
         string Product { get; set; }
         string Company { get; set; }
         string Trademark { get; set; }
         string Authors { get; set; }
         string Owners { get; set; }
    }
}
