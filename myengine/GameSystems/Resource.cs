using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{ 

    public class ResourceFactoryManager
    {
        public void AddFactory(ResourceFactory factory)
        {

        }
        public T LoadResource<T>(FileExisting path) where T : Resource
        {
            return default(T);
        }
    }


    public class ResourceFactory
    {

    }
    public class ResourceFactory<T> : ResourceFactory where T : Resource
    {
        public virtual bool CanCreate(FileExisting path)
        {
            return false;
        }
        public virtual T Create(FileExisting path)
        {
            return default(T);
        }
    }


    public class ResourceCubemapFactory : ResourceFactory<ResourceCubemap>
    {
        public override bool CanCreate(FileExisting path)
        {
            return path.ToString().EndsWith(".cubemap");
        }
        public override ResourceCubemap Create(FileExisting path)
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