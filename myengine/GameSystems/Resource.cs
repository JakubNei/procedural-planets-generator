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
        public T LoadResource<T>(MyFile path) where T : Resource
        {
            return default(T);
        }
    }


    public class ResourceFactory
    {

    }
    public class ResourceFactory<T> : ResourceFactory where T : Resource
    {
        public virtual bool CanCreate(MyFile path)
        {
            return false;
        }
        public virtual T Create(MyFile path)
        {
            return default(T);
        }
    }


    public class ResourceCubemapFactory : ResourceFactory<ResourceCubemap>
    {
        public override bool CanCreate(MyFile path)
        {
            return path.ToString().EndsWith(".cubemap");
        }
        public override ResourceCubemap Create(MyFile path)
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