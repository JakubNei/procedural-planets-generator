using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
    public class Asset
    {
        public AssetFolder AssetFolder
        {
            get
            {
                return AssetSystem.GetAssetFolder(this);
            }
        }
        public AssetSystem AssetSystem { get; private set; }
        public string VirtualPath { get; private set; }
        public bool HasRealPath
        {
            get
            {
                return string.IsNullOrWhiteSpace(RealPath) == false;
            }
        }
        public string RealPath { get; private set; }
        public Asset(AssetSystem assetSystem, string virtualPath, string realPath)
        {
            this.AssetSystem = assetSystem;
            this.VirtualPath = virtualPath;
            this.RealPath = realPath;
        }

        public Stream GetDataStream()
        {
            int numTries = 5;
            while (numTries-- > 0)
            {
                try
                {
                    return new FileStream(RealPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            return null;
        }

        public override string ToString()
        {
            return VirtualPath;
        }
    }
}
