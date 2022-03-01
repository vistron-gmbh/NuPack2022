using System.Collections.Generic;
using System.Xml.Serialization;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    [XmlRoot("root")]
    public class CommandConfig
    {
        public CommandConfig()
        {
            Menus = new List<CommandMenu>();
            Buttons = new List<CommandButton>();
        }

        [XmlArray("menus")]
        [XmlArrayItem("menu")]
        public List<CommandMenu> Menus { get; set; }

        [XmlArray("buttons")]
        [XmlArrayItem("button")]
        public List<CommandButton> Buttons { get; set; }


        [XmlElement("resource")]
        public string ResourceManager { get; set; }
    }
}
