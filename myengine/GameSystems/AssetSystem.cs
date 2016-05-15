using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
    public class AssetSystem : GameSystemBase
    {

        public static AssetSystem Instance { get; private set; }

        string rootResourceFolderPath;

        public Dictionary<string, Type> extensionToTypeAssociation = new Dictionary<string, Type>()
        {
            {"obj", typeof(Mesh)},
            {"glsl", typeof(Shader)},
            {"shader", typeof(Shader)},
        };

        public AssetSystem(string rootResourceFolderPath = "../../../Resources/")
        {
            this.rootResourceFolderPath = rootResourceFolderPath;

            Instance = this;
        }


        public string CombineDirectory(params string[] pathParts)
        {
            return UseCorrectDirectorySeparator(string.Join("/", pathParts));
        }

        public string UseCorrectDirectorySeparator(string path)
        {
            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            path = path.Replace('\\', System.IO.Path.DirectorySeparatorChar);
            return path;
        }

        /*
        public static bool ResourceInFolderExists(AssetPath folder, string childName)
        {
            var lastSlash = folder.originalPath.LastIndexOf("/");
            if (lastSlash == -1) lastSlash = 0;
            var originalPath = folder.originalPath.Substring(0, lastSlash) + childName;

            return File.Exists(MakeRealPath(originalPath));
        }

        
        public static AssetPath GetResourceInFolder(AssetPath folder, string childName)
        {
            var lastSlash = folder.originalPath.LastIndexOf("/");
            if (lastSlash == -1) lastSlash = 0;
            var originalPath = folder.originalPath.Substring(0, lastSlash) + childName;

            return MakeResource(originalPath);
        }
        */

        public bool AssetExists(string virtualPath)
        {
            var realPath = CombineDirectory(rootResourceFolderPath, virtualPath);
            return File.Exists(realPath);
        }

        public bool AssetExists(string virtualPath, AssetFolder startSearchInFolder)
        {
            var realPath = CombineDirectory(rootResourceFolderPath, startSearchInFolder.VirtualPath, virtualPath);
            if (File.Exists(realPath))
            {
                return true;
            }
            else
            {
                if (File.Exists(realPath))
                {
                    return true;
                }
            }
            return false;
        }

        public Asset FindAsset(string virtualPath)
        {
            var realPath = CombineDirectory(rootResourceFolderPath, virtualPath);
            if (File.Exists(realPath))
            {
                return new Asset(this, virtualPath, realPath);
            }
            else
            {
                Debug.Error("File " + virtualPath + " doesnt exits");
                Debug.Pause();
                return null;
            }
        }

        public List<Asset> FindAssets(params string[] virtualPaths)
        {
            var ret = new List<Asset>();
            foreach(var p in virtualPaths)
            {
                ret.Add(FindAsset(p));
            }
            return ret;
        }
        
        public Asset FindAsset(string virtualPath, AssetFolder startSearchInFolder)
        {
            var realPath = CombineDirectory(rootResourceFolderPath, startSearchInFolder.VirtualPath, virtualPath);
            if (File.Exists(realPath))
            {
                return new Asset(this, CombineDirectory(startSearchInFolder.VirtualPath, virtualPath), realPath);
            }
            else
            {
                realPath = CombineDirectory(rootResourceFolderPath, virtualPath);
                if (File.Exists(realPath))
                {
                    return new Asset(this, virtualPath, realPath);
                }
                else
                {
                    Debug.Error("File " + CombineDirectory(startSearchInFolder.VirtualPath, virtualPath) + " doesnt exits");
                    Debug.Pause();
                    return null;
                }
            }
        }



        public AssetFolder GetAssetFolder(Asset asset)
        {
            var virtualDir = Path.GetDirectoryName(asset.VirtualPath);
            return new AssetFolder(virtualDir);
        }


    }
}