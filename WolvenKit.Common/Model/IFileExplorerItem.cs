using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolvenKit.Common
{
    public interface IFileExplorerItem
    {
        string DisplayName { get; }

        string DisplayType { get; }

        string Name { get; }


    }
}
