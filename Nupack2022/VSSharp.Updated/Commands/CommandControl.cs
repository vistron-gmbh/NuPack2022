using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using CnSharp.VisualStudio.Extensions.Resources;
using stdole;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    /// <summary>
    ///     Add-in control of command
    /// </summary>
    [XmlInclude(typeof(CommandMenu))]
    [XmlInclude(typeof(CommandButton))]
    public abstract class CommandControl
    {
        private string _arg;
        private ICommand _command;
        private Form _form;
        private Image _image;
        private int _position;

        /// <summary>
        ///     Constructor
        /// </summary>
        protected CommandControl()
        {
            CommandActionType = CommandActionType.Menu;
            Position = 1;
        }

        /// <summary>
        ///     Id,as while as the command Name
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Text
        /// </summary>
        [XmlAttribute("text")]
        public string Text { get; set; }

        /// <summary>
        ///     Tooltip text
        /// </summary>
        [XmlAttribute("tooltip")]
        public string Tooltip { get; set; }

        /// <summary>
        ///     Office style icon face id
        /// </summary>
        [XmlAttribute("faceId")]
        public int FaceId { get; set; }

        /// <summary>
        ///     Relative position in the parent control,can be minus
        /// </summary>
        /// <remarks>
        ///     相对于父控件Child总数n而言，大于等于0则放在末尾n+1的位置，为负数则放在倒数第n-Position的位置
        /// </remarks>
        [XmlAttribute("position")]
        public int Position
        {
            get { return _position; }
            set
            {
                if (value >= 0)
                    value = 1;
                _position = value;
            }
        }

        /// <summary>
        ///     Picture id in ResourceManager
        /// </summary>
        [XmlAttribute("picture")]
        public string Picture { get; set; }

        [XmlIgnore]
        public StdPicture StdPicture
        {
            get
            {
                if (!string.IsNullOrEmpty(Picture) && Plugin != null && Plugin.ResourceManager != null)
                    return Plugin.ResourceManager.LoadPicture(Picture);
                return null;
            }
        }

        /// <summary>
        ///     Image instance from ResourceManager
        /// </summary>
        [XmlIgnore]
        public Image Image
        {
            get
            {
                if (_image == null && !string.IsNullOrEmpty(Picture) && Picture.Trim().Length > 0 && Plugin != null &&
                    Plugin.ResourceManager != null)
                    _image = Plugin.ResourceManager.LoadBitmap(Picture);
                return _image;
            }
            set { _image = value; }
        }


        /// <summary>
        ///     Action class type name
        /// </summary>
        [XmlAttribute("class")]
        public string ClassName { get; set; }

        /// <summary>
        ///     Action type
        /// </summary>
        [XmlAttribute("type")]
        public CommandActionType CommandActionType { get; set; }

        /// <summary>
        ///     Parent control name that the control attach to
        /// </summary>
        [XmlAttribute("attachTo")]
        public string AttachTo { get; set; }

        //[XmlAttribute("hotKey")]
        //public string HotKey { get; set; }

        /// <summary>
        ///     begin group,insert a bar in context menu if set True
        /// </summary>
        [XmlAttribute("beginGroup")]
        public bool BeginGroup { get; set; }

        /// <summary>
        ///     Command instance of <see cref="ClassName" />
        /// </summary>
        [XmlIgnore]
        public ICommand Command
        {
            get { return _command ?? (_command = LoadInstance(ClassName) as ICommand); }
            set { _command = value; }
        }

        /// <summary>
        ///     <see cref="Plugin" /> which the control attach to
        /// </summary>
        [XmlIgnore]
        public Plugin Plugin { get; set; }


        /// <summary>
        ///     Argument for <see cref="ICommand" /> execution
        /// </summary>
        [XmlAttribute("arg")]
        public string Arg
        {
            get { return _arg; }
            set
            {
                _arg = value;
                Tag = _arg;
            }
        }

        /// <summary>
        ///     <see cref="DependentItems" /> name for making the control  enabled or disabled
        /// </summary>
        [XmlAttribute("dependOn")]
        public string DependOn { get; set; }

        private DependentItems _dependentItems = DependentItems.None;
        [XmlIgnore]
        public DependentItems DependentItems
        {
            get
            {
                return  string.IsNullOrWhiteSpace(DependOn)
                    ? _dependentItems
                    : (DependentItems) Enum.Parse(typeof(DependentItems), DependOn);
            }
            set
            {
                _dependentItems = value;
                DependOn = _dependentItems.ToString();
            }
        }

        public ControlUnavailableState UnavailableState { get; set; }

        [XmlIgnore]
        public Func<bool> EnabledFunc { get; set; }

        /// <summary>
        ///     Argument for <see cref="ICommand" /> execution,only be assgined by programming
        /// </summary>
        [XmlIgnore]
        public object Tag { get; set; }

        [XmlIgnore]
        public Action Action { get; set; }

        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        ///     execute action
        /// </summary>
        public virtual void Execute()
        {
            var arg = Arg ?? Tag;
            switch (CommandActionType)
            {
                case CommandActionType.Program:
                    Command?.Execute(arg);
                    break;
                case CommandActionType.Window:
                    var window = GetForm();
                    window.Show();
                    break;
                case CommandActionType.Dialog:
                    var dialog = GetForm();
                    dialog.ShowDialog();
                    break;
            }
        }

        /// <summary>
        ///     load an instance
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public object LoadInstance(string typeName)
        {
            if (typeName.Contains(","))
            {
                var arr = typeName.Split(',');
                if (arr.Length < 2)
                    return null;
                var assemblyName = arr[1];
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    return assembly.CreateInstance(arr[0]);
                }
                catch
                {
                    var file = Path.Combine(Plugin.Location, assemblyName + ".dll");
                    if (File.Exists(file))
                    {
                        var assembly = Assembly.LoadFile(file);
                        return assembly.CreateInstance(arr[0]);
                    }
                }
            }


            return Plugin.Assembly.CreateInstance(typeName);
        }

        private Form GetForm()
        {
            if (_form != null && !_form.IsDisposed)
                return _form;
            _form = (Form) LoadInstance(ClassName);
            return _form;
        }
    }
}