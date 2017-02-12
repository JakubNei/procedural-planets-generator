using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
    public class AssetFolder
    {
        public string VirtualPath { get; private set; }
        public AssetFolder(string virtualPath)
        {
            this.VirtualPath = virtualPath;
        }
    }
}
