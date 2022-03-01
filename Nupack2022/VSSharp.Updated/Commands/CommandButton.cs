using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    [XmlRoot("button")]
    public class CommandButton : CommandMenu
    {
        //[XmlElement("menu")]
        //public List<CommandMenu> SubMenus { get; set; }

        //[XmlAttribute("sgt")]
        //public string SubGeneratorType { get; set; }

        //public virtual IEnumerable<CommandMenu> GenerateSubMenus()
        //{
        //    if (string.IsNullOrEmpty(SubGeneratorType) || SubGeneratorType.Trim().Length == 0)
        //        return null;
        //    var gen = LoadInstance(SubGeneratorType) as ICommandMenuGenerator;
        //    if (gen == null)
        //        return null;
        //    return gen.Generate();
        //}

        //public virtual void LoadSubMenus()
        //{
        //    if (GenerateSubMenus() == null)
        //        return;
        //    SubMenus = GenerateSubMenus().ToList();
        //}
    }
}