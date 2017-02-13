using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
    public class MyFolder
    {
        public string VirtualPath { get; private set; }
        public MyFolder(string virtualPath)
        {
            this.VirtualPath = virtualPath;
        }
    }
}
