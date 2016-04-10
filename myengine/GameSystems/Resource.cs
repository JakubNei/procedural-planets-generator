using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
    public struct ResourcePath
    {
        public string originalPath;
        public string realPath;

        public static implicit operator string(ResourcePath r)
        {
            return r.realPath;
        }
        public static implicit operator ResourcePath(string originalPath)
        {
            return MakeResource(originalPath);
        }

        public static ResourcePath WithAllPathsAs(string path)
        {
            var r = new ResourcePath();
            r.originalPath = UseCorrectDirectorySeparator(path);
            r.realPath = UseCorrectDirectorySeparator(path);
            return r;
        }

        public static string UseCorrectDirectorySeparator(string path)
        {
            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            path = path.Replace('\\', System.IO.Path.DirectorySeparatorChar);
            return path;
        }

        public static bool ResourceInFolderExists(ResourcePath folder, string childName)
        {
            var lastSlash = folder.originalPath.LastIndexOf("/");
            if (lastSlash == -1) lastSlash = 0;
            var originalPath = folder.originalPath.Substring(0, lastSlash) + childName;

            return File.Exists(MakeRealPath(originalPath));
        }


        public static ResourcePath GetResourceInFolder(ResourcePath folder, string childName)
        {
            var lastSlash = folder.originalPath.LastIndexOf("/");
            if (lastSlash == -1) lastSlash = 0;
            var originalPath = folder.originalPath.Substring(0, lastSlash) + childName;

            return MakeResource(originalPath);
        }

        private static string MakeRealPath(string originalPath)
        {
            return UseCorrectDirectorySeparator("../../../Resources/" + originalPath);
        }

        private static ResourcePath MakeResource(string originalPath)
        {
            var realPath = MakeRealPath(originalPath);
            if (File.Exists(realPath))
            {
                return new ResourcePath() { originalPath = originalPath, realPath = realPath };
            }
            else
            {
                Debug.Error("File " + originalPath + " doesnt exits");
                Debug.Pause();
                return null;
            }
        }

    }








    public class ResourceFactoryManager
    {
        public void AddFactory(ResourceFactory factory)
        {

        }
        public T LoadResource<T>(ResourcePath path) where T : Resource
        {
            return default(T);
        }
    }


    public class ResourceFactory
    {

    }
    public class ResourceFactory<T> : ResourceFactory where T : Resource
    {
        public virtual bool CanCreate(ResourcePath path)
        {
            return false;
        }
        public virtual T Create(ResourcePath path)
        {
            return default(T);
        }
    }


    public class ResourceCubemapFactory : ResourceFactory<ResourceCubemap>
    {
        public override bool CanCreate(ResourcePath path)
        {
            return path.ToString().EndsWith(".cubemap");
        }
        public override ResourceCubemap Create(ResourcePath path)
        {
            return null;
        }
    }

    public class Resource
    {

    }

    public class Resource<T> : Resource
    {

    }

    public class ResourceCubemap : Resource<Cubemap>
    {

    }

}