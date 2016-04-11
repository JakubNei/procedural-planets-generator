using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MyEngine
{
    public class Factory
    {


        static internal Dictionary<string, Shader> allShaders = new Dictionary<string, Shader>();
        public static Shader GetShader(ResourcePath resource)
        {
            Shader s;
            if (!allShaders.TryGetValue(resource, out s))
            {
                s = new Shader(resource);
                allShaders[resource] = s;
                UnloadFactory.Add(s);                
            }
            return s;
        }

        public static void ReloadAllShaders()
        {
            foreach (var s in allShaders)
            {
                s.Value.shouldReload = true;
            }
        }




        //internal Dictionary<string, Mesh> allMeshes = new Dictionary<string, Mesh>();
        public static void AppendMesh(ResourcePath resource, Entity appendTo)
        {
            if (!resource.originalPath.EndsWith(".obj")) throw new System.Exception("Resource path does not end with .obj");

            ObjLoader.Load(resource, appendTo);

            /*Mesh s;
            if (!allMeshes.TryGetValue(resource, out s))
            {
                s = ObjLoader.Load(resource, appendTo);
                allMeshes[resource] = s;
                UnloadFactory.Add(s);
            }
            return s;*/
        }





        internal static Dictionary<string, Mesh> allMeshes = new Dictionary<string, Mesh>();
        public static Mesh GetMesh(ResourcePath resource, bool allowDuplicates=false)
        {
            if (!resource.originalPath.EndsWith(".obj")) throw new System.Exception("Resource path does not end with .obj");

            Mesh s;
            if (allowDuplicates || !allMeshes.TryGetValue(resource, out s))
            {
                s = ObjLoader.Load(resource);
                allMeshes[resource] = s;
                UnloadFactory.Add(s);
            }
            return s;
        }



        internal static Dictionary<string, Texture2D> allTexture2Ds = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture2D(ResourcePath resource)
        {
            Texture2D s;
            if (!allTexture2Ds.TryGetValue(resource, out s))
            {
                s = new Texture2D(resource);
                allTexture2Ds[resource] = s;
            }
            return s;
        }

        internal static Dictionary<string, Cubemap> allCubeMaps = new Dictionary<string, Cubemap>();

        public static Cubemap GetCubeMap(ResourcePath[] resources)
        {
            Cubemap s;
            var key = string.Join("###", resources.Select((x)=>x.ToString()));
            if (!allCubeMaps.TryGetValue(key, out s))
            {
                s = new Cubemap(resources);
                allCubeMaps[key] = s;
            }
            return s;
        }




    }
}
