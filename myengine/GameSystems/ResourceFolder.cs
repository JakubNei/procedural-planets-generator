using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
    public class ResourceFolder
    {
        public string originalPath;
        public string realPath;

        private ResourceFolder()
        {

        }

        public static implicit operator string (ResourceFolder r)
        {
            return r.realPath;
        }
        public static implicit operator ResourceFolder(string originalPath)
        {
            return MakeResource(originalPath);
        }
        private static string MakeRealPath(string originalPath)
        {
            return "../../../Resources/" + originalPath;
        }

        private static ResourceFolder MakeResource(string originalPath)
        {
            var realPath = MakeRealPath(originalPath);
            if (Directory.Exists(realPath))
            {
                return new ResourceFolder() { originalPath = originalPath, realPath = realPath };
            }
            else
            {
                Debug.Error("Directory " + originalPath + " doesnt exits");
                Debug.Pause();
                return null;
            }
        }

    }
}