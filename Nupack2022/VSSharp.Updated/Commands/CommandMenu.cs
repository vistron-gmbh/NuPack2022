using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    [XmlRoot("menu")]
    public class CommandMenu : CommandControl
    {
        private List<CommandMenu> _subMenus;

        public CommandMenu()
        {
            _subMenus = new List<CommandMenu>();
        }

        [XmlElement("menu")]
        public List<CommandMenu> SubMenus
        {
            get
            {
                if (_subMenus.Count == 0 && !string.IsNullOrEmpty(SubGeneratorType))
                {
                    LoadSubMenus();
                }
                return _subMenus;
            }
            set { _subMenus = value; }
        }

        [XmlAttribute("sgt")]
        public string SubGeneratorType { get; set; }

        protected virtual IEnumerable<CommandMenu> GenerateSubMenus()
        {
            if (string.IsNullOrEmpty(SubGeneratorType) || SubGeneratorType.Trim().Length == 0)
                return null;
            var gen = LoadInstance(SubGeneratorType) as ICommandMenuGenerator;
            if (gen == null)
                return null;
            return gen.Generate();
        }

        public virtual void LoadSubMenus()
        {
            if (GenerateSubMenus() == null)
                return;
            _subMenus = GenerateSubMenus().ToList();
        }

    }
}