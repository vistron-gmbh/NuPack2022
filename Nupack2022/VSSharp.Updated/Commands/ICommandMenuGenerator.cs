using System.Collections.Generic;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    public interface ICommandMenuGenerator
    {
        IEnumerable<CommandMenu> Generate();
    }
}
