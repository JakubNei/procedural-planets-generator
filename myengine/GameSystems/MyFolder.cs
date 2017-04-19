using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
    public class FolderExisting
    {
        public string VirtualPath { get; private set; }
        public FolderExisting(string virtualPath)
        {
            this.VirtualPath = virtualPath;
        }
    }
}
