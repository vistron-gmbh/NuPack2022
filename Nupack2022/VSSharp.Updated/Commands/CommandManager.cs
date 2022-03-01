using System;
using System.Collections.Generic;
using System.Linq;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    public class CommandManager
    {
        private readonly ICommandBarAccessor _commandBarAccessor;
        private readonly CommandConfig _config;
        private readonly Plugin _plugin;


        public CommandManager(Plugin plugin)
        {
            if (Host.Plugins.Any(m => m.Id == plugin.Id))
            {
                throw new InvalidOperationException($"plugin {plugin.Id} already exists.");
            }

            _config = plugin.CommandConfig;
            _plugin = plugin;
            plugin.CommandManager = this;

            _commandBarAccessor = new CommandBarAccessor();

            Host.Plugins.Add(plugin);
        }

        public CommandConfig CommandConfig
        {
            get { return _config; }
        }

        //public static AddIn AddIn { set; get; }


        public  void Load()
        {
            _config.Buttons.ForEach(m =>
            {
                m.Plugin = _plugin;
                _commandBarAccessor.AddControl(m);
            });
            _config.Menus.ForEach(m =>
            {
                m.Plugin = _plugin;
                _commandBarAccessor.AddControl(m);
            });
            //_config.ContextMenus.ForEach(m =>
            //{
            //    m.Plugin = _plugin;
            //    _commandBarAccessor.AddControl(m);
            //});
        }

        public void Execute(string commandName)
        {
            CommandControl control = FindCommandControl(commandName);
            if (control != null)
            {
                control.Plugin = _plugin;
                control.Execute();
            }
        }

        private CommandControl FindCommandControl(string commandName)
        {
            CommandMenu menu = FindCommandMenu(_config.Menus, commandName);
            if (menu != null)
            {
                return menu;
            }

            CommandMenu button = FindCommandMenu(_config.Buttons.Cast<CommandMenu>(), commandName);
            if (button != null)
            {
                return button;
            }

            //menu = FindCommandMenu(_config.ContextMenus, commandName);

            //if (menu != null)
            //{
            //    return menu;
            //}
            return null;
        }

        private CommandMenu FindCommandMenu(IEnumerable<CommandMenu> menus, string commandName)
        {
            foreach (CommandMenu m in menus)
            {
                if (String.Compare(m.Id, commandName, true) == 0)
                    return m;
                if (m.SubMenus.Count > 0)
                {
                    CommandMenu subMenu = FindCommandMenu(m.SubMenus, commandName);
                    if (subMenu != null)
                        return subMenu;
                }
            }
            return null;
        }

        public void Reset(Type type)
        {
            CommandControl control = FindCommandControl(type);
            if (control == null)
                return;
            _commandBarAccessor.ResetControl(control);
        }

        public void Reset(string commandName)
        {
            CommandControl control = FindCommandControl(commandName);
            if (control == null)
                return;
            _commandBarAccessor.ResetControl(control);
        }


        public void ApplyDependencies(DependentItems items, bool enabled)
        {
            var ids = new List<string>();

            ids.AddRange(GetMatchedMenus(_config.Menus, items));
            ids.AddRange(GetMatchedMenus(_config.Buttons.Cast<CommandMenu>(), items));

            _commandBarAccessor.EnableControls(ids, enabled);
        }


        private IEnumerable<string> GetMatchedMenus(IEnumerable<CommandMenu> menus, DependentItems items)
        {
            foreach (CommandMenu menu in menus)
            {
                foreach (CommandMenu sub in menu.SubMenus)
                {
                    if (sub.DependentItems.HasFlag(items))
                        yield return sub.Id;
                }
                if (menu.DependentItems.HasFlag(items))
                    yield return menu.Id;
            }
        }

        public CommandControl FindCommandControl(Type type)
        {
            string typeName = type.FullName;
            CommandMenu menu = FindCommandMenuByClassName(_config.Menus, typeName);
            if (menu != null)
            {
                return menu;
            }

            CommandMenu button = FindCommandMenuByClassName(_config.Buttons.Cast<CommandMenu>(), typeName);
            if (button != null)
            {
                return button;
            }

            //menu = FindCommandMenuByClassName(_config.ContextMenus, typeName);

            //if (menu != null)
            //{
            //    return menu;
            //}

            return null;
        }


        private CommandMenu FindCommandMenuByClassName(IEnumerable<CommandMenu> menus, string typeName)
        {
            foreach (CommandMenu m in menus)
            {
                if (String.Compare(m.ClassName, typeName, true) == 0)
                    return m;
                if (m.SubMenus.Count > 0)
                    return FindCommandMenu(m.SubMenus, typeName);
            }
            return null;
        }


        public virtual void Disconnect()
        {
            _commandBarAccessor.Delete();
        }

        //private ObjectContextEventHandler _foo;
        //public event ObjectContextEventHandler ObjectContextChanged
        //{
        //    add
        //    {
        //        if (_foo == null || !_foo.GetInvocationList().Contains(value))
        //        {
        //            _foo += value;
        //        }
        //    }
        //    remove
        //    {
        //        _foo -= value;
        //    }
        //}
    }
}