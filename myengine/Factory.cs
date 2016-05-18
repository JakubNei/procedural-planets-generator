using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MyEngine
{
    public class Factory
    {


        static internal Dictionary<string, Shader> allShaders = new Dictionary<string, Shader>();
        public static Shader GetShader(string asset)
        {
            Shader s;
            if (!allShaders.TryGetValue(asset, out s))
            {
                s = new Shader(AssetSystem.Instance.FindAsset(asset));
                allShaders[asset] = s;
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





        internal static Dictionary<string, Mesh> allMeshes = new Dictionary<string, Mesh>();
        public static Mesh GetMesh(string asset, bool allowDuplicates=false)
        {
            //if (!resource.originalPath.EndsWith(".obj")) throw new System.Exception("Resource path does not end with .obj");

            Mesh s;
            if (allowDuplicates || !allMeshes.TryGetValue(asset, out s))
            {
                s = ObjLoader.Load(AssetSystem.Instance.FindAsset(asset));
                allMeshes[asset] = s;
            }
            return s;
        }



        internal static Dictionary<string, Texture2D> allTexture2Ds = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture2D(string asset)
        {
            Texture2D s;
            if (!allTexture2Ds.TryGetValue(asset, out s))
            {
                s = new Texture2D(AssetSystem.Instance.FindAsset(asset));
                allTexture2Ds[asset] = s;
            }
            return s;
        }

        internal static Dictionary<string, Cubemap> allCubeMaps = new Dictionary<string, Cubemap>();

        public static Cubemap GetCubeMap(string[] assets)
        {
            Cubemap s;
            var key = string.Join("###", assets.Select((x)=>x.ToString()));
            if (!allCubeMaps.TryGetValue(key, out s))
            {
                s = new Cubemap(AssetSystem.Instance.FindAssets(assets).ToArray());
                allCubeMaps[key] = s;
            }
            return s;
        }




    }
}
