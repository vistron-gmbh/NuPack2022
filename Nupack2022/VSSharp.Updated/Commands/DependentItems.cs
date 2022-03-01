using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnSharp.VisualStudio.Extensions.Commands
{
    [Flags]
    public enum DependentItems
    {
        None,
        Document,
        SolutionProject
    }
}
